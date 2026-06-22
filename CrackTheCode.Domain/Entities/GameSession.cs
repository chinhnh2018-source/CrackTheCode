using System;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Domain.Entities
{
    public class GameSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PuzzleId { get; set; }
        public string PuzzleSecretCode { get; set; } = string.Empty;
        public GameMode Mode { get; set; }
        public Difficulty Difficulty { get; set; }
        public int DigitsCount { get; set; }
        public int TimeLimitSeconds { get; set; } // Only for TimeAttack mode
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsWon { get; set; }
        public int ElapsedSeconds { get; set; }
        public int GuessesCount { get; set; }
        public Guid? UserId { get; set; } // Foreign key to User
        public User? User { get; set; }
    }
}
