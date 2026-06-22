using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Interfaces;

namespace CrackTheCode.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var existing = await _userRepository.GetByUsernameAsync(username);
            if (existing != null)
            {
                return null; // Username already taken
            }

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);
            return user;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return null;
            }

            if (VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        // -----------------------------------------------------------------
        // Password hashing — PBKDF2 (SHA-256) with a random per-user salt.
        // Stored format:  pbkdf2.<iterations>.<saltBase64>.<hashBase64>
        // -----------------------------------------------------------------
        private const int Pbkdf2Iterations = 100_000;
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32;  // 256-bit derived key

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, KeySize);
            return $"pbkdf2.{Pbkdf2Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private bool VerifyPassword(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            var parts = stored.Split('.');
            if (parts.Length == 4 && parts[0] == "pbkdf2")
            {
                int iterations = int.Parse(parts[1], CultureInfo.InvariantCulture);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }

            // Legacy fallback: unsalted hex SHA-256 (pre-#2 accounts) so they still log in.
            return LegacySha256(password) == stored;
        }

        private static string LegacySha256(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
