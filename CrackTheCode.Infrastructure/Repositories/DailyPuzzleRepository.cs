using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Interfaces;
using CrackTheCode.Infrastructure.Data;

namespace CrackTheCode.Infrastructure.Repositories
{
    public class DailyPuzzleRepository : IDailyPuzzleRepository
    {
        private readonly CrackTheCodeDbContext _context;

        public DailyPuzzleRepository(CrackTheCodeDbContext context)
        {
            _context = context;
        }

        public async Task<DailyPuzzle?> GetByDateAsync(DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.DailyPuzzles
                .FirstOrDefaultAsync(dp => dp.Date == dateOnly);
        }

        public async Task CreateAsync(DailyPuzzle dailyPuzzle)
        {
            dailyPuzzle.Date = dailyPuzzle.Date.Date; // Truncate time to midnight
            await _context.DailyPuzzles.AddAsync(dailyPuzzle);
            await _context.SaveChangesAsync();
        }
    }
}