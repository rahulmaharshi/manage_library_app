using manage_library_app.Models.Entities;
using manage_library_app.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace manage_library_app.Services.Implements
{
    public class BorrowingService : IBorrowingService
    {
        private readonly ApplicationDbContext _context;
        public BorrowingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> ApproveBorrowingAsync(int id)
        {
            var borrowingRecord = await _context.BorrowingRecords.FindAsync(id);
            if (borrowingRecord == null)
            {
                return (false, "Không tìm thấy phiếu mượn.");
            }

            if (borrowingRecord.Status != BorrowingStatus.Pending)
            {
                return (false, "Phiếu mượn không ở trạng thái chờ duyệt.");
            }

            borrowingRecord.Status = BorrowingStatus.Approved;
            await _context.SaveChangesAsync();

            return (true, "Đã duyệt yêu cầu mượn sách thành công.");
        }

        public async Task<IEnumerable<BorrowingRecord>> GetAllBorrowingsAsync()
        {
            return await _context.BorrowingRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetMyBorrowingsAsync(string userId)
        {
            return await _context.BorrowingRecords
                .Where(b => b.UserId == userId)
                .Include(b => b.Book)
                .ToListAsync();
        }

        public async Task<(bool success, string message)> RequestBorrowingAsync(int bookId, string userId, int loanDurationInDays = 28)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return (false, "Không tìm thấy sách.");
            }

            if (book.AvailableCopies <= 0)
            {
                return (false, "Sách đã hết bản cho mượn.");
            }

            book.AvailableCopies--;

            var newBorrowing = new BorrowingRecord
            {
                BookId = bookId,
                UserId = userId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(loanDurationInDays), // Thiết lập ngày đến hạn
                Status = BorrowingStatus.Pending,
                Fines = 0 // Mặc định số tiền phạt là 0
            };

            _context.BorrowingRecords.Add(newBorrowing);
            await _context.SaveChangesAsync();

            return (true, "Yêu cầu mượn sách đã được gửi thành công. Vui lòng chờ xác nhận.");
        }

        public async Task<(bool success, string message)> ReturnBorrowingAsync(int id)
        {
            var borrowingRecord = await _context.BorrowingRecords
                                                .Include(b => b.Book)
                                                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrowingRecord == null)
            {
                return (false, "Không tìm thấy phiếu mượn.");
            }

            if (borrowingRecord.Status == BorrowingStatus.Returned)
            {
                return (false, "Sách đã được trả trước đó.");
            }

            if (borrowingRecord.Status != BorrowingStatus.Approved)
            {
                return (false, "Phiếu mượn chưa được duyệt hoặc không hợp lệ để trả.");
            }

            // Cập nhật trạng thái và ngày trả
            borrowingRecord.Status = BorrowingStatus.Returned;
            borrowingRecord.ReturnDate = DateTime.UtcNow;

            // Tăng số bản sách có sẵn
            borrowingRecord.Book.AvailableCopies++;

            // Xử lý phạt tiền nếu quá hạn
            if (borrowingRecord.ReturnDate > borrowingRecord.DueDate)
            {
                // Logic tính tiền phạt (ví dụ: 10000 VNĐ/ngày quá hạn)
                var overdueDays = (borrowingRecord.ReturnDate.Value - borrowingRecord.DueDate).TotalDays;
                borrowingRecord.Fines = (decimal)overdueDays * 10000;
            }

            await _context.SaveChangesAsync();

            return (true, "Xác nhận trả sách thành công.");
        }
    }
}
