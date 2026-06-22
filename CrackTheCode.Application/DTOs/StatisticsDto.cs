namespace CrackTheCode.Application.DTOs
{
    public class StatisticsDto
    {
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public double WinRate { get; set; } // Percentage from 0 to 100
        public double AverageTimeSeconds { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
    }
}