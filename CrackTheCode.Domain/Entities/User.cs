using System;

namespace CrackTheCode.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // SHA-256 with salt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
