using System;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Domain.Entities
{
    public class Clue
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PuzzleId { get; set; }
        public ClueType Type { get; set; }
        public string? Guess { get; set; } // Represented as string, e.g. "682"
        public int Value { get; set; } // The target value X (such as count of matching digits, or sum)
        public int? SecondaryValue { get; set; } // Any secondary value if needed
        public string Description { get; set; } = string.Empty; // Vietnamese text description of the clue
        
        // Navigation property for EF Core (optional, but good for linking)
        public Puzzle? Puzzle { get; set; }
    }
}