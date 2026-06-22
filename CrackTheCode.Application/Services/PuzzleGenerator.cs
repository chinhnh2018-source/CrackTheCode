using System;
using System.Collections.Generic;
using System.Linq;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Application.Services
{
    public class PuzzleGenerator : IPuzzleGenerator
    {
        private readonly IPuzzleSolver _solver;

        public PuzzleGenerator(IPuzzleSolver solver)
        {
            _solver = solver;
        }

        public Puzzle Generate(Difficulty difficulty, int digitsCount, int minDigit, int maxDigit, bool allowDuplicates)
        {
            return GenerateWithSeed(difficulty, digitsCount, minDigit, maxDigit, allowDuplicates, new Random().Next());
        }

        public Puzzle GenerateWithSeed(Difficulty difficulty, int digitsCount, int minDigit, int maxDigit, bool allowDuplicates, int seed)
        {
            var random = new Random(seed);

            for (int attempt = 0; attempt < 300; attempt++)
            {
                string secret = GenerateSecretCode(digitsCount, minDigit, maxDigit, allowDuplicates, random);
                var clues = TryBuildUniquePuzzle(secret, difficulty, digitsCount, minDigit, maxDigit, allowDuplicates, random);
                if (clues != null)
                {
                    return new Puzzle
                    {
                        SecretCode = secret,
                        Difficulty = difficulty,
                        DigitsCount = digitsCount,
                        MinDigit = minDigit,
                        MaxDigit = maxDigit,
                        AllowDuplicates = allowDuplicates,
                        Clues = clues,
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }

            return Fallback(difficulty, digitsCount, minDigit, maxDigit, allowDuplicates);
        }

        // -------------------------------------------------------
        // SECRET CODE GENERATOR
        // -------------------------------------------------------
        private string GenerateSecretCode(int len, int min, int max, bool dupes, Random rng)
        {
            var digits = new List<int>();
            int safety = 0;
            while (digits.Count < len && safety++ < 1000)
            {
                int d = rng.Next(min, max + 1);
                if (dupes || !digits.Contains(d))
                    digits.Add(d);
            }
            return string.Join("", digits);
        }

        // -------------------------------------------------------
        // CORE: build a set of clues that uniquely identifies the secret
        // -------------------------------------------------------
        private List<Clue>? TryBuildUniquePuzzle(
            string secret, Difficulty diff, int len,
            int min, int max, bool dupes, Random rng)
        {
            // 1. Generate full candidate pool
            var pool = GeneratePool(secret, len, min, max, dupes, rng);

            // 2. Separate guess-based vs property-based
            var guessClues  = pool.Where(c => c.Guess != null).ToList();
            var propClues   = pool.Where(c => c.Guess == null).ToList();

            // 3. Shuffle each group independently
            guessClues = guessClues.OrderBy(_ => rng.Next()).ToList();
            propClues  = propClues .OrderBy(_ => rng.Next()).ToList();

            // 4. Build ordered candidate list per difficulty
            List<Clue> orderedPool;
            switch (diff)
            {
                case Difficulty.Easy:
                    // Mostly guess-based first (very readable)
                    orderedPool = guessClues.Concat(propClues).ToList();
                    break;
                case Difficulty.Normal:
                    // Interleave 2 guess : 1 prop
                    orderedPool = Interleave(guessClues, propClues, 2, 1, rng);
                    break;
                case Difficulty.Hard:
                    // Interleave 1:1
                    orderedPool = Interleave(guessClues, propClues, 1, 1, rng);
                    break;
                case Difficulty.Expert:
                    // Mostly property-based (requires deduction)
                    orderedPool = propClues.Concat(guessClues).ToList();
                    break;
                case Difficulty.Nightmare:
                    // Pure property-based, shuffled
                    orderedPool = propClues.Concat(guessClues).ToList();
                    break;
                default:
                    orderedPool = guessClues.Concat(propClues).ToList();
                    break;
            }

            // 5. Target clue count per difficulty
            int target = diff switch
            {
                Difficulty.Easy      => 3,
                Difficulty.Normal    => 4,
                Difficulty.Hard      => 5,
                Difficulty.Expert    => 5,
                Difficulty.Nightmare => 3,
                _                    => 4
            };

            // 6. Greedy pick with TYPE DIVERSITY LIMIT (max 1 per ClueType)
            var selected = new List<Clue>();
            var usedTypes = new HashSet<ClueType>();

            foreach (var clue in orderedPool)
            {
                // Enforce diversity: skip if we already have this ClueType
                if (usedTypes.Contains(clue.Type)) continue;

                selected.Add(clue);
                usedTypes.Add(clue.Type);

                var solutions = _solver.Solve(len, min, max, dupes, selected);

                if (solutions.Count == 0)
                {
                    // This clue conflicts — remove it
                    selected.RemoveAt(selected.Count - 1);
                    usedTypes.Remove(clue.Type);
                    continue;
                }

                if (solutions.Count == 1 && solutions[0] == secret)
                {
                    // Found unique solution!
                    if (selected.Count >= target)
                        return selected;

                    // Need more clues to hit target count → keep going
                }
            }

            // 7. Check if final selected set still uniquely solves
            if (selected.Count >= 2)
            {
                var final = _solver.Solve(len, min, max, dupes, selected);
                if (final.Count == 1 && final[0] == secret)
                    return selected;
            }

            return null;
        }

        // -------------------------------------------------------
        // POOL GENERATOR — all clues that are TRUE for this secret
        // -------------------------------------------------------
        private List<Clue> GeneratePool(string secret, int len, int min, int max, bool dupes, Random rng)
        {
            var clues = new List<Clue>();

            // --- GUESS-BASED CLUES ---
            // Mutate 1 digit at each position
            for (int i = 0; i < len; i++)
            {
                // Pick 3 random digit replacements per position
                var used = new HashSet<int> { secret[i] - '0' };
                for (int k = 0; k < 3; k++)
                {
                    int d;
                    int safety = 0;
                    do { d = rng.Next(min, max + 1); } while (used.Contains(d) && safety++ < 20);
                    if (used.Contains(d)) continue;
                    used.Add(d);

                    var chars = secret.ToCharArray();
                    chars[i] = (char)('0' + d);
                    string guess = new string(chars);
                    if (!dupes && guess.Distinct().Count() != len) continue;
                    AddGuessClue(clues, secret, guess);
                }
            }

            // Swapped pairs
            for (int i = 0; i < len - 1; i++)
            {
                var c = secret.ToCharArray();
                (c[i], c[i + 1]) = (c[i + 1], c[i]);
                AddGuessClue(clues, secret, new string(c));
            }

            // 10 fully random wrong guesses
            for (int k = 0; k < 10; k++)
            {
                var g = GenerateSecretCode(len, min, max, true, rng);
                if (g != secret) AddGuessClue(clues, secret, g);
            }

            // --- PROPERTY-BASED CLUES ---
            int sum  = secret.Sum(c => c - '0');
            int maxD = secret.Max(c => c - '0');
            int minD = secret.Min(c => c - '0');
            int evenCount = secret.Count(c => (c - '0') % 2 == 0);
            int oddCount  = len - evenCount;
            int primeCount = secret.Count(c => IsPrime(c - '0'));
            int gt5Count   = secret.Count(c => (c - '0') > 5);

            // SumEquals
            clues.Add(new Clue { Type = ClueType.SumEquals, Value = sum,
                Description = $"Tổng các chữ số = {sum}" });

            // SumGreaterThan / SumLessThan (offset randomly 1-3)
            int lo = rng.Next(1, 4); int hi = rng.Next(1, 4);
            clues.Add(new Clue { Type = ClueType.SumGreaterThan, Value = sum - lo,
                Description = $"Tổng các chữ số > {sum - lo}" });
            clues.Add(new Clue { Type = ClueType.SumLessThan, Value = sum + hi,
                Description = $"Tổng các chữ số < {sum + hi}" });

            // HasEven / HasOdd
            clues.Add(new Clue { Type = ClueType.HasEven, Value = evenCount,
                Description = $"Có đúng {evenCount} chữ số chẵn" });
            clues.Add(new Clue { Type = ClueType.HasOdd,  Value = oddCount,
                Description = $"Có đúng {oddCount} chữ số lẻ" });

            // Prime digits
            clues.Add(new Clue { Type = ClueType.ExactlyXPrimeDigits, Value = primeCount,
                Description = $"Có đúng {primeCount} chữ số nguyên tố (2,3,5,7)" });

            // > 5 digits
            clues.Add(new Clue { Type = ClueType.ExactlyXDigitsGreaterThanFive, Value = gt5Count,
                Description = $"Có đúng {gt5Count} chữ số lớn hơn 5" });

            // Max / Min
            clues.Add(new Clue { Type = ClueType.MaxDigitEquals, Value = maxD,
                Description = $"Chữ số lớn nhất là {maxD}" });
            clues.Add(new Clue { Type = ClueType.MinDigitEquals, Value = minD,
                Description = $"Chữ số nhỏ nhất là {minD}" });

            // Sum pos 1+2
            if (len >= 2)
            {
                int p12 = (secret[0] - '0') + (secret[1] - '0');
                clues.Add(new Clue { Type = ClueType.SumPositionsOneAndTwoEquals, Value = p12,
                    Description = $"Tổng chữ số thứ 1 + thứ 2 = {p12}" });
            }

            // First > Last
            if (len >= 2 && (secret[0] - '0') > (secret[len - 1] - '0'))
                clues.Add(new Clue { Type = ClueType.FirstDigitGreaterThanLast,
                    Description = "Chữ số đầu tiên lớn hơn chữ số cuối cùng" });

            // Middle is prime
            int mid = len / 2;
            if (IsPrime(secret[mid] - '0'))
                clues.Add(new Clue { Type = ClueType.MiddleDigitIsPrime,
                    Description = "Chữ số ở giữa là số nguyên tố" });

            // Palindrome
            bool isPal = Enumerable.Range(0, len / 2).All(i => secret[i] == secret[len - 1 - i]);
            if (isPal)
                clues.Add(new Clue { Type = ClueType.IsPalindrome,
                    Description = "Mã đọc xuôi hay đọc ngược đều giống nhau" });

            // Consecutive
            bool hasConsec = Enumerable.Range(0, len - 1).Any(i => Math.Abs(secret[i] - secret[i + 1]) == 1);
            if (hasConsec)
                clues.Add(new Clue { Type = ClueType.HasConsecutiveDigits,
                    Description = "Có ít nhất một cặp số liên tiếp cạnh nhau" });

            // Repeating
            if (secret.Distinct().Count() < len)
                clues.Add(new Clue { Type = ClueType.AtLeastOneRepeatingDigit,
                    Description = "Có ít nhất một chữ số xuất hiện nhiều hơn 1 lần" });

            return clues;
        }

        // -------------------------------------------------------
        // HELPERS
        // -------------------------------------------------------
        private void AddGuessClue(List<Clue> clues, string secret, string guess)
        {
            int bulls = CountBulls(secret, guess);
            int cows  = CountCows(secret, guess);

            string fmt = string.Join(" ", guess.ToCharArray());

            if (bulls == 0 && cows == 0)
            {
                clues.Add(new Clue { Type = ClueType.AllWrong, Guess = guess, Value = 0,
                    Description = $"[ {fmt} ] — Không có chữ số nào đúng" });
            }
            else if (bulls > 0 && cows == 0)
            {
                clues.Add(new Clue { Type = ClueType.CorrectDigitsAndPositions, Guess = guess, Value = bulls,
                    Description = $"[ {fmt} ] — Có {bulls} số đúng vị trí, 0 số sai vị trí" });
            }
            else if (bulls == 0 && cows > 0)
            {
                clues.Add(new Clue { Type = ClueType.CorrectDigitsWrongPositions, Guess = guess, Value = cows,
                    Description = $"[ {fmt} ] — Có 0 số đúng vị trí, {cows} số sai vị trí" });
            }
            else
            {
                // Both bulls and cows: add as a combined clue (use CorrectDigitsAndPositions type)
                clues.Add(new Clue { Type = ClueType.CorrectDigitsAndPositions, Guess = guess, Value = bulls,
                    Description = $"[ {fmt} ] — Có {bulls} số đúng vị trí, {cows} số sai vị trí" });
            }
        }

        private List<Clue> Interleave(List<Clue> a, List<Clue> b, int ratioA, int ratioB, Random rng)
        {
            var result = new List<Clue>();
            int ia = 0, ib = 0;
            while (ia < a.Count || ib < b.Count)
            {
                for (int k = 0; k < ratioA && ia < a.Count; k++) result.Add(a[ia++]);
                for (int k = 0; k < ratioB && ib < b.Count; k++) result.Add(b[ib++]);
            }
            return result;
        }

        private int CountBulls(string s, string g)
        {
            int n = 0;
            for (int i = 0; i < s.Length && i < g.Length; i++)
                if (s[i] == g[i]) n++;
            return n;
        }

        private int CountCows(string s, string g)
        {
            int cows = 0;
            var sv = new bool[s.Length]; var gv = new bool[g.Length];
            for (int i = 0; i < s.Length && i < g.Length; i++)
                if (s[i] == g[i]) { sv[i] = gv[i] = true; }
            for (int i = 0; i < s.Length; i++)
            {
                if (sv[i]) continue;
                for (int j = 0; j < g.Length; j++)
                {
                    if (!gv[j] && s[i] == g[j]) { cows++; gv[j] = true; break; }
                }
            }
            return cows;
        }

        private bool IsPrime(int d) => d == 2 || d == 3 || d == 5 || d == 7;

        private Puzzle Fallback(Difficulty diff, int len, int min, int max, bool dupes)
        {
            string secret = string.Join("", Enumerable.Range(min + 1, len).Reverse());
            return new Puzzle
            {
                SecretCode = secret, Difficulty = diff, DigitsCount = len,
                MinDigit = min, MaxDigit = max, AllowDuplicates = dupes,
                Clues = new List<Clue>
                {
                    new Clue { Type = ClueType.SumEquals,     Value = secret.Sum(c=>c-'0'), Description = $"Tổng các chữ số = {secret.Sum(c=>c-'0')}" },
                    new Clue { Type = ClueType.MaxDigitEquals, Value = secret.Max(c=>c-'0'), Description = $"Chữ số lớn nhất là {secret.Max(c=>c-'0')}" },
                    new Clue { Type = ClueType.MinDigitEquals, Value = secret.Min(c=>c-'0'), Description = $"Chữ số nhỏ nhất là {secret.Min(c=>c-'0')}" }
                },
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
