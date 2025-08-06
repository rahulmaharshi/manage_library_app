using CsvHelper.Configuration.Attributes;

namespace manage_library_app.Models.DTOs
{
    public class CsvBookDto
    {
        [Name("ISBN")]
        public string Isbn { get; set; }

        [Name("Book-Title")]
        public string Title { get; set; }

        [Name("Book-Author")]
        public string AuthorName { get; set; }

        [Name("Year-Of-Publication")]
        public int PublicationYear { get; set; }

        [Name("Publisher")]
        public string PublisherName { get; set; }

        [Name("Image-URL-L")]
        public string ImageUrlL { get; set; }
    }
}
