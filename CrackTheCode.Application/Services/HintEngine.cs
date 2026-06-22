using System;
using System.Collections.Generic;
using System.Linq;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Application.Services
{
    public class HintEngine : IHintEngine
    {
        public string GetHintLevel1(Puzzle puzzle, string userGuess)
        {
            var secret = puzzle.SecretCode;
            
            // Try to find a digit in the secret that is NOT correctly guessed by position in userGuess
            var candidateDigits = new List<char>();
            for (int i = 0; i < secret.Length; i++)
            {
                if (i >= userGuess.Length || secret[i] != userGuess[i])
                {
                    candidateDigits.Add(secret[i]);
                }
            }

            // Fallback: If candidateDigits is empty, just take any digit from the secret
            if (!candidateDigits.Any())
            {
                candidateDigits = secret.ToList();
            }

            // Select a random digit from candidate digits to reveal
            var rand = new Random();
            char revealedDigit = candidateDigits[rand.Next(candidateDigits.Count)];
            
            return $"Số '{revealedDigit}' có xuất hiện trong mật mã bí mật.";
        }

        public string GetHintLevel2(Puzzle puzzle, string userGuess)
        {
            var secret = puzzle.SecretCode;
            
            // Try to find a position that is NOT correctly guessed by userGuess
            var incorrectIndices = new List<int>();
            for (int i = 0; i < secret.Length; i++)
            {
                if (i >= userGuess.Length || secret[i] != userGuess[i])
                {
                    incorrectIndices.Add(i);
                }
            }

            // Fallback: If everything is guessed, pick any index
            if (!incorrectIndices.Any())
            {
                incorrectIndices = Enumerable.Range(0, secret.Length).ToList();
            }

            var rand = new Random();
            int selectedIndex = incorrectIndices[rand.Next(incorrectIndices.Count)];
            char correctDigit = secret[selectedIndex];

            return $"Chữ số thứ {selectedIndex + 1} trong dãy mã mật là số '{correctDigit}'.";
        }

        public List<int> GetHintLevel3(Puzzle puzzle)
        {
            var secretDigits = puzzle.SecretCode.Select(c => c - '0').Distinct().ToList();
            var min = puzzle.MinDigit;
            var max = puzzle.MaxDigit;

            // Collect all digits in range [min, max] that do NOT appear in the secret
            var eliminated = new List<int>();
            for (int d = min; d <= max; d++)
            {
                if (!secretDigits.Contains(d))
                {
                    eliminated.Add(d);
                }
            }

            return eliminated;
        }
    }
}