using System;

namespace CrackTheCode.Domain.Entities
{
    public class DailyPuzzle
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } // Representing the specific calendar date (UTC or local midnight)
        public int Seed { get; set; } // Mathematical seed used for procedural puzzle generation
        public string PuzzleJson { get; set; } = string.Empty; // Serialized Puzzle data
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}