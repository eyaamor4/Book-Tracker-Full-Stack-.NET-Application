namespace FrontendTest.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public Genre? Genre { get; set; }
        public Status Status { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }
    }
}
