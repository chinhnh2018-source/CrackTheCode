using System.Threading.Tasks;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Application.Interfaces
{
    public interface IDailyPuzzleService
    {
        /// <summary>
        /// Retrieves or generates the standard seeded Daily Puzzle for the current calendar date.
        /// </summary>
        Task<Puzzle> GetDailyPuzzleAsync();
    }
}