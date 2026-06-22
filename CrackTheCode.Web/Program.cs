using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Application.Services;
using CrackTheCode.Domain.Interfaces;
using CrackTheCode.Infrastructure.Data;
using CrackTheCode.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<CrackTheCodeDbContext>(options =>
    options.UseSqlite("Data Source=../crackthecode.db"));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPuzzleRepository, PuzzleRepository>();
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
builder.Services.AddScoped<IDailyPuzzleRepository, DailyPuzzleRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPuzzleSolver, PuzzleSolver>();
builder.Services.AddScoped<IPuzzleGenerator, PuzzleGenerator>();
builder.Services.AddScoped<IHintEngine, HintEngine>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDailyPuzzleService, DailyPuzzleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------------------------------------------
// DATABASE INIT + AUTO-MIGRATION (safe, non-destructive)
// -------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CrackTheCodeDbContext>();

    // Create tables that do not yet exist
    context.Database.EnsureCreated();

    // Patch existing DB with any new columns added after first deployment
    RunMigrations(context);
}

static void RunMigrations(CrackTheCodeDbContext context)
{
    var conn = context.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open)
        conn.Open();

    // -- 1. GameSessions.UserId (added in v2 for multi-player) --
    if (!ColumnExists(conn, "GameSessions", "UserId"))
    {
        Exec(conn, @"ALTER TABLE ""GameSessions"" ADD COLUMN ""UserId"" TEXT NULL");
        Console.WriteLine("[Migration] GameSessions.UserId column added.");
    }

    // -- 2. Users table (added in v2) --
    if (!TableExists(conn, "Users"))
    {
        Exec(conn, @"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""Id""           TEXT NOT NULL PRIMARY KEY,
                ""Username""     TEXT NOT NULL,
                ""PasswordHash"" TEXT NOT NULL,
                ""CreatedAt""    TEXT NOT NULL
            )");
        Exec(conn, @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Username"" ON ""Users"" (""Username"")");
        Console.WriteLine("[Migration] Users table created.");
    }

    conn.Close();
}

static void Exec(System.Data.Common.DbConnection conn, string sql)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    cmd.ExecuteNonQuery();
}

static bool ColumnExists(System.Data.Common.DbConnection conn, string table, string column)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = $"PRAGMA table_info(\"{table}\")";
    using var r = cmd.ExecuteReader();
    while (r.Read())
        if (r["name"]?.ToString() == column) return true;
    return false;
}

static bool TableExists(System.Data.Common.DbConnection conn, string table)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'";
    return cmd.ExecuteScalar() != null;
}

// -------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.Run();
