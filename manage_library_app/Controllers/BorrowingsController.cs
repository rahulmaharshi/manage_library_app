using manage_library_app.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace manage_library_app.Controllers
{
    [ApiController]
    [Route("api/borrowings")]
    public class BorrowingsController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;
        public BorrowingsController(IBorrowingService borrowingService)
        {
            _borrowingService = borrowingService; 
        }

        [HttpPost("request")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RequestBorrowing([FromQuery] int bookId, [FromQuery] int loanDurationInDays = 28)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc thiếu thông tin người dùng" });
            }

            var (success, message) = await _borrowingService.RequestBorrowingAsync(bookId, userId, loanDurationInDays);

            if (success)
            {
                return Ok(new { success = true, message = message });
            }

            return BadRequest(new { success = false, message = message });
        }

        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetAllBorrowings()
        {
            var borrowings = await _borrowingService.GetAllBorrowingsAsync();
            return Ok(new { success = true, response = borrowings });
        }

        [HttpGet("my-records")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMyBorrowings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc thiếu thông tin người dùng" });
            }

            var borrowings = await _borrowingService.GetMyBorrowingsAsync(userId);
            return Ok(new { success = true, response = borrowings });
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> ApproveBorrowing(int id)
        {
            var (success, message) = await _borrowingService.ApproveBorrowingAsync(id);

            if (success)
            {
                return Ok(new { success = true, message = message });
            }

            return BadRequest(new { success = false, message = message });
        }

        [HttpPost("{id}/return")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> ReturnBorrowing(int id)
        {
            var (success, message) = await _borrowingService.ReturnBorrowingAsync(id);

            if (success)
            {
                return Ok(new { success = true, message = message });
            }

            return BadRequest(new { success = false, message = message });
        }
    }
}
