using System;
using System.Collections.Generic;
using Xunit;
using CrackTheCode.Application.Services;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Tests
{
    public class SolverAndGeneratorTests
    {
        [Fact]
        public void Solver_ShouldCorrectlyIdentifyBullsAndCows()
        {
            // Arrange
            var solver = new PuzzleSolver();
            var clues = new List<Clue>
            {
                new Clue { Type = ClueType.CorrectDigitsAndPositions, Guess = "682", Value = 1 },
                new Clue { Type = ClueType.CorrectDigitsWrongPositions, Guess = "614", Value = 1 }
            };

            // Act & Assert
            // If the secret is "682", then guess "682" should have 3 bulls (value is 1, so candidate "389" would fit clue 1 with 1 bull)
            // Let's test a simple IsValid check:
            // Secret is "682"
            // Guess "682" has 1 bull -> If secret is "604": "682" has 1 bull ('6'), "614" has 2 bulls ('6' and '4').
            // Let's verify our Bulls/Cows logic mathematically:
            // Candidate: "206"
            // Guess: "682" -> Bulls = 0, Cows = 2 ('2' and '6')
            // Let's check candidate "081" against clues:
            // Clue 1: Guess "682", Value 1 bull. "081" and "682" has bull at index 1 ('8') -> Match.
            // Clue 2: Guess "614", Value 1 cow. "081" and "614" has cow '1' (at index 2 in "081", index 1 in "614") -> Match.
            
            bool isValid = solver.IsValid("081", clues);
            Assert.True(isValid);
        }

        [Fact]
        public void Solver_AllWrongClue_ShouldFilterOutCandidate()
        {
            // Arrange
            var solver = new PuzzleSolver();
            var clues = new List<Clue>
            {
                new Clue { Type = ClueType.AllWrong, Guess = "738", Value = 0 }
            };

            // Act & Assert
            // Candidate "123" has '3' which is in "738" -> Invalid
            Assert.False(solver.IsValid("123", clues));
            // Candidate "124" has no common digits with "738" -> Valid
            Assert.True(solver.IsValid("124", clues));
        }

        [Fact]
        public void Solver_SumEqualsClue_ShouldValidateCorrectly()
        {
            // Arrange
            var solver = new PuzzleSolver();
            var clues = new List<Clue>
            {
                new Clue { Type = ClueType.SumEquals, Value = 15 }
            };

            // Act & Assert
            // "555" has sum 15 -> Valid
            Assert.True(solver.IsValid("555", clues));
            // "123" has sum 6 -> Invalid
            Assert.False(solver.IsValid("123", clues));
        }

        [Fact]
        public void Solver_EvenDigitsClue_ShouldValidateCorrectly()
        {
            // Arrange
            var solver = new PuzzleSolver();
            var clues = new List<Clue>
            {
                new Clue { Type = ClueType.HasEven, Value = 2 } // Exactly 2 even digits
            };

            // Act & Assert
            // "241" has exactly 2 evens (2, 4) -> Valid
            Assert.True(solver.IsValid("241", clues));
            // "135" has 0 evens -> Invalid
            Assert.False(solver.IsValid("135", clues));
        }

        [Fact]
        public void Generator_ShouldCreateUniquelySolvablePuzzles()
        {
            // Arrange
            var solver = new PuzzleSolver();
            var generator = new PuzzleGenerator(solver);

            // Act
            // Generate a random 3-digit Easy puzzle
            var puzzle = generator.Generate(Difficulty.Easy, 3, 0, 9, true);

            // Assert
            Assert.NotNull(puzzle);
            Assert.NotEmpty(puzzle.SecretCode);
            Assert.Equal(3, puzzle.SecretCode.Length);
            Assert.NotEmpty(puzzle.Clues);

            // Use solver to solve this puzzle and verify exactly ONE solution exists
            var solutions = solver.Solve(3, 0, 9, true, puzzle.Clues);
            Assert.Single(solutions);
            Assert.Equal(puzzle.SecretCode, solutions[0]);
        }
    }
}