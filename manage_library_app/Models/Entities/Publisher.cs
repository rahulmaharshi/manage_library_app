namespace manage_library_app.Models.Entities
{
    public class Publisher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
