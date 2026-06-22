using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Interfaces;
using CrackTheCode.Infrastructure.Data;

namespace CrackTheCode.Infrastructure.Repositories
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly CrackTheCodeDbContext _context;

        public GameSessionRepository(CrackTheCodeDbContext context)
        {
            _context = context;
        }

        public async Task<GameSession?> GetByIdAsync(Guid id)
        {
            return await _context.GameSessions.FindAsync(id);
        }

        public async Task<List<GameSession>> GetAllAsync()
        {
            return await _context.GameSessions.ToListAsync();
        }

        public async Task<List<GameSession>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.GameSessions
                .Where(gs => gs.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateAsync(GameSession session)
        {
            await _context.GameSessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GameSession session)
        {
            _context.GameSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetGamesPlayedAsync(Guid userId)
        {
            return await _context.GameSessions
                .CountAsync(gs => gs.UserId == userId && gs.IsCompleted);
        }

        public async Task<int> GetGamesWonAsync(Guid userId)
        {
            return await _context.GameSessions
                .CountAsync(gs => gs.UserId == userId && gs.IsCompleted && gs.IsWon);
        }

        public async Task<double> GetAverageTimeSecondsAsync(Guid userId)
        {
            var wonGames = await _context.GameSessions
                .Where(gs => gs.UserId == userId && gs.IsCompleted && gs.IsWon)
                .Select(gs => gs.ElapsedSeconds)
                .ToListAsync();

            if (!wonGames.Any()) return 0.0;
            return wonGames.Average();
        }

        public async Task<int> GetCurrentStreakAsync(Guid userId)
        {
            var completedGames = await _context.GameSessions
                .Where(gs => gs.UserId == userId && gs.IsCompleted && gs.EndTime != null)
                .OrderByDescending(gs => gs.EndTime)
                .ToListAsync();

            int streak = 0;
            foreach (var g in completedGames)
            {
                if (g.IsWon)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }
            return streak;
        }

        public async Task<int> GetMaxStreakAsync(Guid userId)
        {
            var completedGames = await _context.GameSessions
                .Where(gs => gs.UserId == userId && gs.IsCompleted && gs.EndTime != null)
                .OrderBy(gs => gs.EndTime)
                .ToListAsync();

            int maxStreak = 0;
            int currentStreak = 0;

            foreach (var g in completedGames)
            {
                if (g.IsWon)
                {
                    currentStreak++;
                    if (currentStreak > maxStreak)
                    {
                        maxStreak = currentStreak;
                    }
                }
                else
                {
                    currentStreak = 0;
                }
            }

            return maxStreak;
        }
    }
}
