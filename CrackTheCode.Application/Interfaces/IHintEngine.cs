using System.Collections.Generic;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Application.Interfaces
{
    public interface IHintEngine
    {
        /// <summary>
        /// Level 1 Hint: Reveals a digit that exists in the secret code.
        /// </summary>
        string GetHintLevel1(Puzzle puzzle, string userGuess);

        /// <summary>
        /// Level 2 Hint: Reveals the exact position of a digit.
        /// </summary>
        string GetHintLevel2(Puzzle puzzle, string userGuess);

        /// <summary>
        /// Level 3 Hint: Returns a list of digits (0-9) that CANNOT possibly appear in the secret code.
        /// </summary>
        List<int> GetHintLevel3(Puzzle puzzle);
    }
}