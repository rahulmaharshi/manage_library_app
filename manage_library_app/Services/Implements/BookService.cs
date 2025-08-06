using CsvHelper;
using manage_library_app.Models.DTOs;
using manage_library_app.Models.DTOs.Book;
using manage_library_app.Models.Entities;
using manage_library_app.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace manage_library_app.Services.Implements
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public BookService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<(bool success, string message, BookDto? bookDto)> AddBookAsync(AddBookDto bookDto)
        {
            // Kiểm tra xem sách có ISBN đã tồn tại chưa
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == bookDto.ISBN);
            if (existingBook != null)
            {
                return (false, "Sách với ISBN này đã tồn tại.", null);
            }

            // Kiểm tra Author và Publisher có tồn tại không
            var authorExists = await _context.Authors.AnyAsync(a => a.Id == bookDto.AuthorId);
            var publisherExists = await _context.Publishers.AnyAsync(p => p.Id == bookDto.PublisherId);

            if (!authorExists)
            {
                return (false, "ID tác giả không tồn tại.", null);
            }

            if (!publisherExists)
            {
                return (false, "ID nhà xuất bản không tồn tại.", null);
            }

            var newBook = new Book
            {
                Title = bookDto.Title,
                Description = bookDto.Description,
                ISBN = bookDto.ISBN,
                PublicationYear = bookDto.PublicationYear,
                TotalCopies = bookDto.TotalCopies,
                AvailableCopies = bookDto.TotalCopies, // Sách mới thêm vào thì số bản có sẵn bằng tổng số bản
                ImageUrl = bookDto.ImageUrl,
                AuthorId = bookDto.AuthorId,
                PublisherId = bookDto.PublisherId
            };

            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();

            // Lấy thông tin Author và Publisher để tạo DTO trả về
            var author = await _context.Authors.FindAsync(newBook.AuthorId);
            var publisher = await _context.Publishers.FindAsync(newBook.PublisherId);

            var createdBookDto = new BookDto
            {
                Id = newBook.Id,
                Title = newBook.Title,
                Description = newBook.Description,
                ISBN = newBook.ISBN,
                PublicationYear = newBook.PublicationYear,
                TotalCopies = newBook.TotalCopies,
                AvailableCopies = newBook.AvailableCopies,
                ImageUrl = newBook.ImageUrl,
                AuthorName = author.FullName,
                PublisherName = publisher.Name
            };

            return (true, "Sách đã được thêm thành công.", createdBookDto);
        }

        public async Task<int> BulkInsertBooksFromCsvAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "scape", "bulk-book.csv");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File bulk-book.csv không được tìm thấy.");
            }

            var authors = await _context.Authors.ToDictionaryAsync(a => a.FullName);
            var publishers = await _context.Publishers.ToDictionaryAsync(p => p.Name);

            var newAuthors = new List<Author>();
            var newPublishers = new List<Publisher>();
            var newBooks = new List<Book>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CsvBookDto>().ToList();

                foreach (var record in records)
                {
                    // Xử lý Author
                    if (!authors.ContainsKey(record.AuthorName))
                    {
                        var newAuthor = new Author { FullName = record.AuthorName, Biography = "Đây là tiểu sử của tác giả" };
                        authors.Add(record.AuthorName, newAuthor);
                        newAuthors.Add(newAuthor);
                    }

                    // Xử lý Publisher
                    if (!publishers.ContainsKey(record.PublisherName))
                    {
                        var newPublisher = new Publisher { Name = record.PublisherName, Address = "Đây là địa chỉ nhà xuất bảng" };
                        publishers.Add(record.PublisherName, newPublisher);
                        newPublishers.Add(newPublisher);
                    }

                    // Xử lý Book
                    var author = authors[record.AuthorName];
                    var publisher = publishers[record.PublisherName];

                    // Kiểm tra xem sách đã tồn tại chưa
                    if (!await _context.Books.AnyAsync(b => b.ISBN == record.Isbn))
                    {
                        var newBook = new Book
                        {
                            Title = record.Title,
                            Description = "Đây là sách thư viện",
                            ISBN = record.Isbn,
                            PublicationYear = record.PublicationYear,
                            TotalCopies = 20,
                            AvailableCopies = 20,
                            ImageUrl = record.ImageUrlL,
                            Author = author,
                            Publisher = publisher
                        };
                        newBooks.Add(newBook);
                    }
                }
            }

            try
            {
                await _context.Authors.AddRangeAsync(newAuthors);
                await _context.Publishers.AddRangeAsync(newPublishers);
                await _context.Books.AddRangeAsync(newBooks);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // In ra lỗi bên trong để xem chi tiết
                var innerExceptionMessage = ex.InnerException?.Message;
                if (innerExceptionMessage != null)
                {
                    Console.WriteLine("Inner Exception: " + innerExceptionMessage);
                    // Bạn có thể ném một exception mới với thông báo chi tiết hơn
                    throw new Exception("Lỗi khi lưu dữ liệu. Chi tiết: " + innerExceptionMessage, ex);
                }
                throw; // Ném lại lỗi nếu không có inner exception
            }

            return newBooks.Count;
        }

        public async Task<(bool success, string message)> DeleteBookAsync(int id)
        {
            var bookToDelete = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (bookToDelete == null)
            {
                return (false, "Không tìm thấy sách để xóa.");
            }

            _context.Books.Remove(bookToDelete);
            await _context.SaveChangesAsync();

            return (true, "Xóa sách thành công.");
        }

        public async Task<(IEnumerable<BookDto> books, int totalCount)> GetAllBooksAsync(string? searchTerm, int page, int pageSize)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.Title.Contains(searchTerm)
                                      || b.Author.FullName.Contains(searchTerm)
                                      || b.ISBN.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var booksDto = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    ISBN = b.ISBN,
                    PublicationYear = b.PublicationYear,
                    ImageUrl = b.ImageUrl,
                    TotalCopies = b.TotalCopies,
                    AvailableCopies = b.AvailableCopies,
                    AuthorName = b.Author.FullName,
                    PublisherName = b.Publisher.Name
                })
                .ToListAsync();

            return (booksDto, totalCount);
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return null;
            }

            // Ánh xạ từ Entity sang DTO
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear,
                ImageUrl = book.ImageUrl,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                AuthorName = book.Author.FullName,
                PublisherName = book.Publisher.Name
            };
        }

        public async Task<(bool success, string message)> UpdateBookAsync(int id, AddBookDto bookDto)
        {
            var bookToUpdate = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (bookToUpdate == null)
            {
                return (false, "Không tìm thấy sách để cập nhật.");
            }

            // Kiểm tra ISBN trùng lặp (trừ chính cuốn sách đang cập nhật)
            var existingBookWithIsbn = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == bookDto.ISBN && b.Id != id);
            if (existingBookWithIsbn != null)
            {
                return (false, "Sách với ISBN này đã tồn tại.");
            }

            // Kiểm tra Author và Publisher có tồn tại không
            var authorExists = await _context.Authors.AnyAsync(a => a.Id == bookDto.AuthorId);
            var publisherExists = await _context.Publishers.AnyAsync(p => p.Id == bookDto.PublisherId);

            if (!authorExists)
            {
                return (false, "ID tác giả không tồn tại.");
            }

            if (!publisherExists)
            {
                return (false, "ID nhà xuất bản không tồn tại.");
            }

            // Cập nhật các thuộc tính
            bookToUpdate.Title = bookDto.Title;
            bookToUpdate.Description = bookDto.Description;
            bookToUpdate.ISBN = bookDto.ISBN;
            bookToUpdate.PublicationYear = bookDto.PublicationYear;
            bookToUpdate.TotalCopies = bookDto.TotalCopies;
            bookToUpdate.AvailableCopies = bookDto.TotalCopies; // Hoặc có logic khác tùy thuộc vào nghiệp vụ
            bookToUpdate.ImageUrl = bookDto.ImageUrl;
            bookToUpdate.AuthorId = bookDto.AuthorId;
            bookToUpdate.PublisherId = bookDto.PublisherId;

            await _context.SaveChangesAsync();

            return (true, "Cập nhật sách thành công.");
        }
    }
}
