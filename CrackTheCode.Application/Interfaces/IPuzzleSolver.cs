using System.Collections.Generic;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Application.Interfaces
{
    public interface IPuzzleSolver
    {
        /// <summary>
        /// Solves a puzzle with the specified configuration and list of clues.
        /// Returns all candidate secret codes that satisfy ALL clues.
        /// </summary>
        List<string> Solve(int digitsCount, int minDigit, int maxDigit, bool allowDuplicates, List<Clue> clues);

        /// <summary>
        /// Validates whether a specific candidate code satisfies all the clues.
        /// </summary>
        bool IsValid(string candidate, List<Clue> clues);
    }
}