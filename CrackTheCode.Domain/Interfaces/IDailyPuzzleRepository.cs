using System;
using System.Threading.Tasks;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Domain.Interfaces
{
    public interface IDailyPuzzleRepository
    {
        Task<DailyPuzzle?> GetByDateAsync(DateTime date);
        Task CreateAsync(DailyPuzzle dailyPuzzle);
    }
}