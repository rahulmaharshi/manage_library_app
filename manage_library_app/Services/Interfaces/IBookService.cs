using manage_library_app.Models.DTOs.Book;
using manage_library_app.Models.Entities;

namespace manage_library_app.Services.Interfaces
{
    public interface IBookService
    {
        Task<int> BulkInsertBooksFromCsvAsync();
        Task<(IEnumerable<BookDto> books, int totalCount)> GetAllBooksAsync(string? searchTerm, int page, int pageSize);
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<(bool success, string message, BookDto? bookDto)> AddBookAsync(AddBookDto bookDto);
        Task<(bool success, string message)> UpdateBookAsync(int id, AddBookDto bookDto);
        Task<(bool success, string message)> DeleteBookAsync(int id);
    }
}
