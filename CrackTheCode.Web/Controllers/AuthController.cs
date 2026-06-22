using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CrackTheCode.Application.Interfaces;

namespace CrackTheCode.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Username và Password không được để trống." });
            }

            var user = await _authService.RegisterAsync(request.Username, request.Password);
            if (user == null)
            {
                return BadRequest(new { error = "Username đã tồn tại hoặc không hợp lệ." });
            }

            return Ok(new { userId = user.Id, username = user.Username, message = "Đăng ký tài khoản thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Username và Password không được để trống." });
            }

            var user = await _authService.LoginAsync(request.Username, request.Password);
            if (user == null)
            {
                return BadRequest(new { error = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            return Ok(new { userId = user.Id, username = user.Username, message = "Đăng nhập thành công!" });
        }
    }

    public class AuthRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
