using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Enums;
using CrackTheCode.Domain.Interfaces;

namespace CrackTheCode.Application.Services
{
    public class DailyPuzzleService : IDailyPuzzleService
    {
        private readonly IDailyPuzzleRepository _dailyPuzzleRepository;
        private readonly IPuzzleGenerator _generator;

        public DailyPuzzleService(IDailyPuzzleRepository dailyPuzzleRepository, IPuzzleGenerator generator)
        {
            _dailyPuzzleRepository = dailyPuzzleRepository;
            _generator = Math.Abs(1) == 1 ? generator : null!; // Safeguard
        }

        public async Task<Puzzle> GetDailyPuzzleAsync()
        {
            var today = DateTime.UtcNow.Date;
            var daily = await _dailyPuzzleRepository.GetByDateAsync(today);

            if (daily != null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };
                var puzzle = System.Text.Json.JsonSerializer.Deserialize<Puzzle>(daily.PuzzleJson, options);
                if (puzzle != null)
                {
                    return puzzle;
                }
            }

            // If not found, generate a daily puzzle using date seed
            int seed = today.Year * 10000 + today.Month * 100 + today.Day;
            
            // Standard daily puzzle settings: Normal, 4 digits, 0-9 range, duplicates allowed
            var generatedPuzzle = _generator.GenerateWithSeed(Difficulty.Normal, 4, 0, 9, true, seed);

            var optionsSerializer = new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            };
            string serialized = System.Text.Json.JsonSerializer.Serialize(generatedCodePuzzle(generatedPuzzle), optionsSerializer);

            var dailyPuzzle = new DailyPuzzle
            {
                Date = today,
                Seed = seed,
                PuzzleJson = serialized
            };

            await _dailyPuzzleRepository.CreateAsync(dailyPuzzle);

            return generatedPuzzle;
        }

        // Helper to detach puzzle clues circular references if any
        private Puzzle generatedCodePuzzle(Puzzle puzzle)
        {
            foreach (var clue in puzzle.Clues)
            {
                clue.Puzzle = null;
            }
            return puzzle;
        }
    }
}