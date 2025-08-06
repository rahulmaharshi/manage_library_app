using manage_library_app.Models.Entities;

namespace manage_library_app.Services.Interfaces
{
    public interface IBorrowingService
    {
        Task<(bool success, string message)> RequestBorrowingAsync(int bookId, string userId, int loanDurationInDays = 14);
        Task<IEnumerable<BorrowingRecord>> GetAllBorrowingsAsync();
        Task<IEnumerable<BorrowingRecord>> GetMyBorrowingsAsync(string userId);
        Task<(bool success, string message)> ApproveBorrowingAsync(int id);
        Task<(bool success, string message)> ReturnBorrowingAsync(int id);
    }
}
