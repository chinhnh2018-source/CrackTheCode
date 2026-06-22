using System;
using System.Threading.Tasks;
using CrackTheCode.Application.DTOs;

namespace CrackTheCode.Application.Interfaces
{
    public interface IStatisticsService
    {
        /// <summary>
        /// Retrieves calculated aggregate player statistics by UserId.
        /// </summary>
        Task<StatisticsDto> GetStatisticsAsync(Guid userId);

        /// <summary>
        /// Records the completion of a game session and updates historic player stats.
        /// </summary>
        Task RecordGameResultAsync(Guid sessionId, bool isWon, int elapsedSeconds, int guessesCount);
    }
}
