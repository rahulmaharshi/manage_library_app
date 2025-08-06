using System.ComponentModel.DataAnnotations;

namespace manage_library_app.Models.DTOs.Book
{
    public class AddBookDto
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        public int PublicationYear { get; set; }

        public int TotalCopies { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int PublisherId { get; set; }
    }
}
