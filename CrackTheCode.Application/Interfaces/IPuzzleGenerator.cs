using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Enums;

namespace CrackTheCode.Application.Interfaces
{
    public interface IPuzzleGenerator
    {
        /// <summary>
        /// Generates a puzzle of the given difficulty with exactly one unique solution.
        /// </summary>
        Puzzle Generate(Difficulty difficulty, int digitsCount, int minDigit, int maxDigit, bool allowDuplicates);

        /// <summary>
        /// Generates a puzzle procedurally with a specific random seed, ensuring a single unique solution.
        /// </summary>
        Puzzle GenerateWithSeed(Difficulty difficulty, int digitsCount, int minDigit, int maxDigit, bool allowDuplicates, int seed);
    }
}