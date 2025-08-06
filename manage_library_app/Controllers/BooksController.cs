using manage_library_app.Models.DTOs.Book;
using manage_library_app.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace manage_library_app.Controllers
{
    [ApiController]
    [Route("api/book")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] AddBookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", response = ModelState });
            }

            var (success, message) = await _bookService.UpdateBookAsync(id, bookDto);

            if (success)
            {
                return Ok(new { success = true, message = message, response = (object)null });
            }

            return BadRequest(new { success = false, message = message, response = (object)null });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var (success, message) = await _bookService.DeleteBookAsync(id);

            if (success)
            {
                return Ok(new { success = true, message = message, response = (object)null });
            }

            return NotFound(new { success = false, message = message, response = (object)null });
        }

        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> AddBook([FromBody] AddBookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", response = ModelState });
            }

            var (success, message, createdBookDto) = await _bookService.AddBookAsync(bookDto);

            if (success)
            {
                return Ok(new { success = true, message = message, response = createdBookDto });
            }

            return BadRequest(new { success = false, message = message, response = (object)null });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sách.", response = (object)null });
            }

            return Ok(new { success = true, message = "Lấy thông tin sách thành công.", response = book });
        }

        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetAllBooks([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { success = false, message = "Tham số phân trang không hợp lệ.", response = (object)null });
            }

            var (books, totalCount) = await _bookService.GetAllBooksAsync(searchTerm, page, pageSize);

            var response = new
            {
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                totalItems = totalCount,
                currentPage = page,
                items = books
            };

            return Ok(new { success = true, message = "Lấy danh sách sách thành công.", response = response });
        }

        [HttpPost("bulk-insert")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> BulkInsertBooksFromCsv()
        {
            try
            {
                var insertedCount = await _bookService.BulkInsertBooksFromCsvAsync();
                return Ok(new { success = true, message = $"{insertedCount} sách đã được thêm thành công." });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi thêm sách.", error = ex.Message });
            }
        }
    }
}
