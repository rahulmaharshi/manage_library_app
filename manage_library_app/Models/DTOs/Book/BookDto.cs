namespace manage_library_app.Models.DTOs.Book
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ISBN { get; set; }
        public int PublicationYear { get; set; }
        public string ImageUrl { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        public string AuthorName { get; set; }
        public string PublisherName { get; set; }
    }
}
