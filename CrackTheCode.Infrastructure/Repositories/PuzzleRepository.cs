using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Interfaces;
using CrackTheCode.Infrastructure.Data;

namespace CrackTheCode.Infrastructure.Repositories
{
    public class PuzzleRepository : IPuzzleRepository
    {
        private readonly CrackTheCodeDbContext _context;

        public PuzzleRepository(CrackTheCodeDbContext context)
        {
            _context = context;
        }

        public async Task<Puzzle?> GetByIdAsync(Guid id)
        {
            return await _context.Puzzles
                .Include(p => p.Clues)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(Puzzle puzzle)
        {
            await _context.Puzzles.AddAsync(puzzle);
            await _context.SaveChangesAsync();
        }
    }
}