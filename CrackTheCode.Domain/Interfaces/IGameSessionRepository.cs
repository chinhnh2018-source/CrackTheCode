using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Domain.Interfaces
{
    public interface IGameSessionRepository
    {
        Task<GameSession?> GetByIdAsync(Guid id);
        Task<List<GameSession>> GetAllAsync();
        Task<List<GameSession>> GetAllByUserIdAsync(Guid userId);
        Task CreateAsync(GameSession session);
        Task UpdateAsync(GameSession session);
        Task<int> GetGamesPlayedAsync(Guid userId);
        Task<int> GetGamesWonAsync(Guid userId);
        Task<double> GetAverageTimeSecondsAsync(Guid userId);
        Task<int> GetCurrentStreakAsync(Guid userId);
        Task<int> GetMaxStreakAsync(Guid userId);
    }
}
