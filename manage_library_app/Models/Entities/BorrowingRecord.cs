using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manage_library_app.Models.Entities
{
    public enum BorrowingStatus
    {
        Pending,
        Approved,
        Returned,
        Overdue
    }

    public class BorrowingRecord
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }

        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
        public BorrowingStatus Status { get; set; }
        public decimal Fines { get; set; }
    }
}
