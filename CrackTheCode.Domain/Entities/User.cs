using System;

namespace CrackTheCode.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // PBKDF2 (SHA-256) with per-user salt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
