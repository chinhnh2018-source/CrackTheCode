using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrackTheCode.Application.Interfaces;
using CrackTheCode.Domain.Entities;
using CrackTheCode.Domain.Enums;
using CrackTheCode.Domain.Interfaces;

namespace CrackTheCode.Web.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly IPuzzleGenerator _generator;
        private readonly IPuzzleRepository _puzzleRepository;
        private readonly IGameSessionRepository _sessionRepository;
        private readonly IHintEngine _hintEngine;
        private readonly IStatisticsService _statisticsService;
        private readonly IDailyPuzzleService _dailyPuzzleService;
        private readonly IUserRepository _userRepository;

        public GameController(
            IPuzzleGenerator generator,
            IPuzzleRepository puzzleRepository,
            IGameSessionRepository sessionRepository,
            IHintEngine hintEngine,
            IStatisticsService statisticsService,
            IDailyPuzzleService dailyPuzzleService,
            IUserRepository userRepository)
        {
            _generator = generator;
            _puzzleRepository = puzzleRepository;
            _sessionRepository = sessionRepository;
            _hintEngine = hintEngine;
            _statisticsService = statisticsService;
            _dailyPuzzleService = dailyPuzzleService;
            _userRepository = userRepository;
        }

        private Guid? GetUserIdFromHeaders()
        {
            // Identity comes from the verified JWT (see [Authorize] + JwtBearer),
            // not a client-supplied header — so it cannot be spoofed.
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(sub, out var userId))
            {
                return userId;
            }
            return null;
        }

        // Resolves the caller's user id AND verifies the user still exists in the DB.
        // Prevents inserting a GameSession that references a non-existent user
        // (which would otherwise throw "FOREIGN KEY constraint failed").
        private async Task<(Guid? userId, IActionResult? error)> ResolveValidUserAsync()
        {
            var userId = GetUserIdFromHeaders();
            if (userId == null)
            {
                return (null, Unauthorized(new { error = "Yêu cầu đăng nhập trước khi chơi.", code = "NOT_AUTHENTICATED" }));
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                // Stale client identity (e.g. localStorage userId after the DB was reset).
                return (null, Unauthorized(new { error = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", code = "INVALID_USER" }));
            }

            return (userId, null);
        }

        // GET /api/newgame
        [HttpGet("newgame")]
        public async Task<IActionResult> NewGame(
            [FromQuery] string difficulty = "Normal",
            [FromQuery] int digitsCount = 3,
            [FromQuery] string mode = "Classic",
            [FromQuery] int timeLimit = 60,
            [FromQuery] bool allowDuplicates = true,
            [FromQuery] int minDigit = 0,
            [FromQuery] int maxDigit = 9)
        {
            var (userId, authError) = await ResolveValidUserAsync();
            if (authError != null) return authError;

            try
            {
                if (!Enum.TryParse<Difficulty>(difficulty, true, out var diffEnum))
                {
                    diffEnum = Difficulty.Normal;
                }

                if (!Enum.TryParse<GameMode>(mode, true, out var modeEnum))
                {
                    modeEnum = GameMode.Classic;
                }

                if (digitsCount < 3) digitsCount = 3;
                if (digitsCount > 5) digitsCount = 5;
                if (minDigit < 0) minDigit = 0;
                if (maxDigit > 9) maxDigit = 9;
                if (minDigit > maxDigit) minDigit = maxDigit;

                var puzzle = _generator.Generate(diffEnum, digitsCount, minDigit, maxDigit, allowDuplicates);
                await _puzzleRepository.CreateAsync(puzzle);

                var session = new GameSession
                {
                    PuzzleId = puzzle.Id,
                    PuzzleSecretCode = puzzle.SecretCode,
                    Mode = modeEnum,
                    Difficulty = diffEnum,
                    DigitsCount = digitsCount,
                    TimeLimitSeconds = modeEnum == GameMode.TimeAttack ? timeLimit : 0,
                    StartTime = DateTime.UtcNow,
                    IsCompleted = false,
                    IsWon = false,
                    GuessesCount = 0,
                    UserId = userId.Value
                };
                await _sessionRepository.CreateAsync(session);

                return Ok(new
                {
                    sessionId = session.Id,
                    digitsCount = digitsCount,
                    difficulty = diffEnum.ToString(),
                    mode = modeEnum.ToString(),
                    timeLimitSeconds = session.TimeLimitSeconds,
                    clues = puzzle.Clues.Select(c => new
                    {
                        id = c.Id,
                        type = c.Type.ToString(),
                        description = c.Description,
                        guess = c.Guess,
                        value = c.Value
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST /api/checkanswer
        [HttpPost("checkanswer")]
        public async Task<IActionResult> CheckAnswer([FromBody] CheckAnswerRequest request)
        {
            var userId = GetUserIdFromHeaders();
            if (userId == null) return Unauthorized(new { error = "Chưa đăng nhập." });

            if (request == null || string.IsNullOrWhiteSpace(request.Guess))
            {
                return BadRequest(new { error = "Guess is required." });
            }

            var session = await _sessionRepository.GetByIdAsync(request.SessionId);
            if (session == null || session.UserId != userId)
            {
                return NotFound(new { error = "Game session not found." });
            }

            if (session.IsCompleted)
            {
                return BadRequest(new { error = "This game session is already finished." });
            }

            session.GuessesCount++;
            bool isCorrect = request.Guess == session.PuzzleSecretCode;
            var now = DateTime.UtcNow;
            int elapsed = (int)(now - session.StartTime).TotalSeconds;

            if (isCorrect)
            {
                await _statisticsService.RecordGameResultAsync(session.Id, true, elapsed, session.GuessesCount);
                return Ok(new
                {
                    isCorrect = true,
                    message = "Chính xác! Bạn đã giải được mật mã bí mật!",
                    isCompleted = true,
                    secretCode = session.PuzzleSecretCode
                });
            }
            else
            {
                int bulls = CountBulls(session.PuzzleSecretCode, request.Guess);
                int cows = CountCows(session.PuzzleSecretCode, request.Guess);

                await _sessionRepository.UpdateAsync(session);

                return Ok(new
                {
                    isCorrect = false,
                    message = $"Sai rồi! Phản hồi: {bulls} số đúng vị trí, {cows} số đúng nhưng sai vị trí.",
                    isCompleted = false,
                    bulls = bulls,
                    cows = cows
                });
            }
        }

        // POST /api/gethint
        [HttpPost("gethint")]
        public async Task<IActionResult> GetHint([FromBody] GetHintRequest request)
        {
            var userId = GetUserIdFromHeaders();
            if (userId == null) return Unauthorized();

            if (request == null) return BadRequest();

            var session = await _sessionRepository.GetByIdAsync(request.SessionId);
            if (session == null || session.UserId != userId) return NotFound(new { error = "Session not found." });

            var puzzle = await _puzzleRepository.GetByIdAsync(session.PuzzleId);
            if (puzzle == null) return NotFound(new { error = "Puzzle not found." });

            if (request.Level == 1)
            {
                string hint1 = _hintEngine.GetHintLevel1(puzzle, request.Guess ?? string.Empty);
                return Ok(new { level = 1, hint = hint1 });
            }
            else if (request.Level == 2)
            {
                string hint2 = _hintEngine.GetHintLevel2(puzzle, request.Guess ?? string.Empty);
                return Ok(new { level = 2, hint = hint2 });
            }
            else if (request.Level == 3)
            {
                var eliminatedDigits = _hintEngine.GetHintLevel3(puzzle);
                return Ok(new { level = 3, eliminatedDigits = eliminatedDigits });
            }

            return BadRequest(new { error = "Invalid hint level." });
        }

        // GET /api/showanswer
        [HttpGet("showanswer")]
        public async Task<IActionResult> ShowAnswer([FromQuery] Guid sessionId)
        {
            var userId = GetUserIdFromHeaders();
            if (userId == null) return Unauthorized();

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.UserId != userId) return NotFound(new { error = "Session not found." });

            if (!session.IsCompleted)
            {
                int elapsed = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                await _statisticsService.RecordGameResultAsync(session.Id, false, elapsed, session.GuessesCount);
            }

            return Ok(new { secretCode = session.PuzzleSecretCode });
        }

        // GET /api/dailypuzzle
        [HttpGet("dailypuzzle")]
        public async Task<IActionResult> GetDailyPuzzle()
        {
            var (userId, authError) = await ResolveValidUserAsync();
            if (authError != null) return authError;

            try
            {
                var puzzle = await _dailyPuzzleService.GetDailyPuzzleAsync();

                var today = DateTime.UtcNow.Date;
                var sessions = await _sessionRepository.GetAllByUserIdAsync(userId.Value);
                var dailySession = sessions
                    .FirstOrDefault(s => s.Mode == GameMode.Daily && s.StartTime.Date == today && !s.IsCompleted);

                if (dailySession == null)
                {
                    dailySession = new GameSession
                    {
                        PuzzleId = puzzle.Id,
                        PuzzleSecretCode = puzzle.SecretCode,
                        Mode = GameMode.Daily,
                        Difficulty = puzzle.Difficulty,
                        DigitsCount = puzzle.DigitsCount,
                        StartTime = DateTime.UtcNow,
                        IsCompleted = false,
                        IsWon = false,
                        GuessesCount = 0,
                        UserId = userId.Value
                    };
                    await _sessionRepository.CreateAsync(dailySession);
                }

                return Ok(new
                {
                    sessionId = dailySession.Id,
                    digitsCount = puzzle.DigitsCount,
                    difficulty = puzzle.Difficulty.ToString(),
                    mode = "Daily",
                    timeLimitSeconds = 0,
                    clues = puzzle.Clues.Select(c => new
                    {
                        id = c.Id,
                        type = c.Type.ToString(),
                        description = c.Description,
                        guess = c.Guess,
                        value = c.Value
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var userId = GetUserIdFromHeaders();
            if (userId == null) return Unauthorized();

            var stats = await _statisticsService.GetStatisticsAsync(userId.Value);
            return Ok(stats);
        }

        private int CountBulls(string candidate, string guess)
        {
            int bulls = 0;
            for (int i = 0; i < candidate.Length; i++)
            {
                if (i < guess.Length && candidate[i] == guess[i])
                {
                    bulls++;
                }
            }
            return bulls;
        }

        private int CountCows(string candidate, string guess)
        {
            int cows = 0;
            var candArr = candidate.ToCharArray();
            var guessArr = guess.ToCharArray();
            var candVisited = new bool[candArr.Length];
            var guessVisited = new bool[guessArr.Length];

            for (int i = 0; i < candArr.Length; i++)
            {
                if (i < guessArr.Length && candArr[i] == guessArr[i])
                {
                    candVisited[i] = true;
                    guessVisited[i] = true;
                }
            }

            for (int i = 0; i < candArr.Length; i++)
            {
                if (!candVisited[i])
                {
                    for (int j = 0; j < guessArr.Length; j++)
                    {
                        if (!guessVisited[j] && candArr[i] == guessArr[j])
                        {
                            cows++;
                            guessVisited[j] = true;
                            break;
                        }
                    }
                }
            }

            return cows;
        }
    }


    public class CheckAnswerRequest
    {
        public Guid SessionId { get; set; }
        public string Guess { get; set; } = string.Empty;
    }

    public class GetHintRequest
    {
        public Guid SessionId { get; set; }
        public int Level { get; set; }
        public string? Guess { get; set; }
    }
}
