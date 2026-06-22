using Microsoft.EntityFrameworkCore;
using CrackTheCode.Domain.Entities;

namespace CrackTheCode.Infrastructure.Data
{
    public class CrackTheCodeDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<Clue> Clues { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<DailyPuzzle> DailyPuzzles { get; set; }

        public CrackTheCodeDbContext(DbContextOptions<CrackTheCodeDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Clue -> Puzzle relationship
            modelBuilder.Entity<Clue>()
                .HasOne(c => c.Puzzle)
                .WithMany(p => p.Clues)
                .HasForeignKey(c => c.PuzzleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index on date for daily puzzles
            modelBuilder.Entity<DailyPuzzle>()
                .HasIndex(dp => dp.Date)
                .IsUnique();

            // Unique index on username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
