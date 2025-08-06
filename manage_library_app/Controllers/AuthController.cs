using manage_library_app.Models.DTOs.Auth;
using manage_library_app.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace manage_library_app.Controllers
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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", response = ModelState });
            }
            var result = await _authService.RegisterMemberAsync(request);
            if (result.Succeeded)
            {
                return Ok(new { success = true, message = "Đăng ký thành công", response = (object)null });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { success = false, message = "Đăng ký thất bại", response = errors });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", response = ModelState });
            }
            var authResponse = await _authService.LoginAsync(request);
            if (authResponse == null)
            {
                return Unauthorized(new { success = false, message = "Email hoặc mật khẩu không đúng", response = (object)null });
            }
            return Ok(new { success = true, message = "Đăng nhập thành công", response = authResponse });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Không tìm thấy thông tin người dùng", response = (object)null });
            }
            var userInfo = await _authService.GetUserInfoAsync(userId);
            if (userInfo == null)
            {
                return NotFound(new { success = false, message = "Người dùng không tồn tại", response = (object)null });
            }
            return Ok(new { success = true, message = "Lấy thông tin người dùng thành công", response = userInfo });
        }

        [HttpPost("admin/users/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUserByAdmin([FromBody] CreateUserRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", response = ModelState });
            }
            var result = await _authService.CreateUserByAdminAsync(request);
            if (result.Succeeded)
            {
                return Ok(new { success = true, message = $"Tài khoản {request.Role} đã được tạo thành công.", response = (object)null });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { success = false, message = "Tạo người dùng thất bại", response = errors });
            }
        }
    }
}
