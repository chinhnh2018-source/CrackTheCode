using System;
using System.Threading.Tasks;
using CrackTheCode.Application.DTOs;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Interfaces;

namespace CrackTheCode.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IGameSessionRepository _sessionRepository;

        public StatisticsService(IGameSessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<StatisticsDto> GetStatisticsAsync(Guid userId)
        {
            int played = await _sessionRepository.GetGamesPlayedAsync(userId);
            int won = await _sessionRepository.GetGamesWonAsync(userId);
            double avgTime = await _sessionRepository.GetAverageTimeSecondsAsync(userId);
            int currentStreak = await _sessionRepository.GetCurrentStreakAsync(userId);
            int maxStreak = await _sessionRepository.GetMaxStreakAsync(userId);

            double winRate = played > 0 ? ((double)won / played) * 100.0 : 0.0;

            return new StatisticsDto
            {
                GamesPlayed = played,
                GamesWon = won,
                WinRate = Math.Round(winRate, 1),
                AverageTimeSeconds = Math.Round(avgTime, 1),
                CurrentStreak = currentStreak,
                MaxStreak = maxStreak
            };
        }

        public async Task RecordGameResultAsync(Guid sessionId, bool isWon, int elapsedSeconds, int guessesCount)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session != null)
            {
                session.IsCompleted = true;
                session.IsWon = isWon;
                session.ElapsedSeconds = elapsedSeconds;
                session.GuessesCount = guessesCount;
                session.EndTime = DateTime.UtcNow;

                await _sessionRepository.UpdateAsync(session);
            }
        }
    }
}
