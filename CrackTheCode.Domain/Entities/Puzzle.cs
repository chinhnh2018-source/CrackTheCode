using System;
using System.Collections.Generic;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Domain.Entities
{
    public class Puzzle
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SecretCode { get; set; } = string.Empty; // e.g. "682"
        public Difficulty Difficulty { get; set; }
        public int DigitsCount { get; set; }
        public int MinDigit { get; set; } = 0; // Usually 0 or 1
        public int MaxDigit { get; set; } = 9; // Usually 9
        public bool AllowDuplicates { get; set; } = true;
        public List<Clue> Clues { get; set; } = new List<Clue>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}