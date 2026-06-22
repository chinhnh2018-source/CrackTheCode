using System;
using System.Collections.Generic;
using System.Linq;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Application.Services
{
    public class PuzzleSolver : IPuzzleSolver
    {
        public List<string> Solve(int digitsCount, int minDigit, int maxDigit, bool allowDuplicates, List<Clue> clues)
        {
            var solutions = new List<string>();
            GenerateAndSolve(string.Empty, digitsCount, minDigit, maxDigit, allowDuplicates, clues, solutions);
            return solutions;
        }

        private void GenerateAndSolve(string current, int length, int minDigit, int maxDigit, bool allowDuplicates, List<Clue> clues, List<string> solutions)
        {
            if (current.Length == length)
            {
                if (IsValid(current, clues))
                {
                    solutions.Add(current);
                }
                return;
            }

            for (int d = minDigit; d <= maxDigit; d++)
            {
                string digitStr = d.ToString();
                if (!allowDuplicates && current.Contains(digitStr))
                {
                    continue;
                }

                GenerateAndSolve(current + digitStr, length, minDigit, maxDigit, allowDuplicates, clues, solutions);
            }
        }

        public bool IsValid(string candidate, List<Clue> clues)
        {
            foreach (var clue in clues)
            {
                if (!CheckClue(candidate, clue))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckClue(string candidate, Clue clue)
        {
            switch (clue.Type)
            {
                case ClueType.CorrectDigitsAndPositions:
                    return CountBulls(candidate, clue.Guess!) == clue.Value;

                case ClueType.CorrectDigitsWrongPositions:
                    return CountCows(candidate, clue.Guess!) == clue.Value;

                case ClueType.AllWrong:
                    // Bulls and Cows both must be 0
                    return CountBulls(candidate, clue.Guess!) == 0 && CountCows(candidate, clue.Guess!) == 0;

                case ClueType.SumEquals:
                    return SumDigits(candidate) == clue.Value;

                case ClueType.SumGreaterThan:
                    return SumDigits(candidate) > clue.Value;

                case ClueType.SumLessThan:
                    return SumDigits(candidate) < clue.Value;

                case ClueType.HasEven:
                    if (clue.Value == -1) // At least one
                        return candidate.Any(c => (c - '0') % 2 == 0);
                    return candidate.Count(c => (c - '0') % 2 == 0) == clue.Value;

                case ClueType.HasOdd:
                    if (clue.Value == -1) // At least one
                        return candidate.Any(c => (c - '0') % 2 != 0);
                    return candidate.Count(c => (c - '0') % 2 != 0) == clue.Value;

                case ClueType.ExactlyXPrimeDigits:
                    return candidate.Count(c => IsPrime(c - '0')) == clue.Value;

                case ClueType.ExactlyXDigitsGreaterThanFive:
                    return candidate.Count(c => (c - '0') > 5) == clue.Value;

                case ClueType.AtLeastOneRepeatingDigit:
                    return candidate.Distinct().Count() < candidate.Length;

                case ClueType.MaxDigitEquals:
                    return candidate.Max(c => c - '0') == clue.Value;

                case ClueType.MinDigitEquals:
                    return candidate.Min(c => c - '0') == clue.Value;

                case ClueType.HasConsecutiveDigits:
                    for (int i = 0; i < candidate.Length - 1; i++)
                    {
                        if (Math.Abs(candidate[i] - candidate[i + 1]) == 1)
                        {
                            return true;
                        }
                    }
                    return false;

                case ClueType.IsPalindrome:
                    for (int i = 0; i < candidate.Length / 2; i++)
                    {
                        if (candidate[i] != candidate[candidate.Length - 1 - i])
                        {
                            return false;
                        }
                    }
                    return true;

                case ClueType.SumPositionsOneAndTwoEquals:
                    if (candidate.Length < 2) return false;
                    return (candidate[0] - '0') + (candidate[1] - '0') == clue.Value;

                case ClueType.FirstDigitGreaterThanLast:
                    if (candidate.Length < 2) return false;
                    return (candidate[0] - '0') > (candidate[candidate.Length - 1] - '0');

                case ClueType.MiddleDigitIsPrime:
                    if (candidate.Length == 0) return false;
                    int midIndex = candidate.Length / 2;
                    return IsPrime(candidate[midIndex] - '0');

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int CountBulls(string candidate, string guess)
        {
            int bulls = 0;
            for (int i = 0; i < candidate.Length; i++)
            {
                if (i < guess.Length && candidate[i] == guess[i])
                {
                    bulls++;
                }
            }
            return bulls;
        }

        private int CountCows(string candidate, string guess)
        {
            int cows = 0;
            var candArr = candidate.ToCharArray();
            var guessArr = guess.ToCharArray();
            var candVisited = new bool[candArr.Length];
            var guessVisited = new bool[guessArr.Length];

            // Mark Bulls first
            for (int i = 0; i < candArr.Length; i++)
            {
                if (i < guessArr.Length && candArr[i] == guessArr[i])
                {
                    candVisited[i] = true;
                    guessVisited[i] = true;
                }
            }

            // Count Cows
            for (int i = 0; i < candArr.Length; i++)
            {
                if (!candVisited[i])
                {
                    for (int j = 0; j < guessArr.Length; j++)
                    {
                        if (!guessVisited[j] && candArr[i] == guessArr[j])
                        {
                            cows++;
                            guessVisited[j] = true;
                            break;
                        }
                    }
                }
            }

            return cows;
        }

        private int SumDigits(string candidate)
        {
            return candidate.Sum(c => c - '0');
        }

        private bool IsPrime(int digit)
        {
            return digit == 2 || digit == 3 || digit == 5 || digit == 7;
        }
    }
}