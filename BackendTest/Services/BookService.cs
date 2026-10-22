using BackendTest.Data;
using BackendTest.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Services
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            await ValidateSaveAsync(book, null);

            book.Id = 0;                       // the database generates the Id
            book.Title = book.Title.Trim();
            book.Status = Status.ToRead;       // rule 5.d: always ToRead on creation

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            return book;
        }

        public async Task DeleteBooksAsync(List<int> ids)
        {
            var books = await _context.Books.Where(b => ids.Contains(b.Id)).ToListAsync();

            if (books.Count != ids.Count)
                throw new KeyNotFoundException("One or more books not found.");

            // Rule 8.e: a book being read cannot be deleted
            var reading = books.Where(b => b.Status == Status.Reading)
                               .Select(b => b.Title)
                               .ToList();

            if (reading.Count > 0)
                throw new InvalidOperationException(
                    $"Cannot delete books with status 'Reading': {string.Join(", ", reading)}.");

            _context.Books.RemoveRange(books);
            await _context.SaveChangesAsync();
        }

        

        public async Task<List<Book>> GetBooksAsync(string? search, int skip = 0, int take = 5)
        {
            var query = _context.Books.AsQueryable();

            // Title OR Author contains the search text
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(s)
                                      || (b.Author != null && b.Author.ToLower().Contains(s)));
            }

            return await query
                .OrderByDescending(b => b.CreatedDateTime)   // sort BEFORE Skip/Take
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
            if (existingBook == null)
      
                throw new KeyNotFoundException("Book not found.");
            await ValidateSaveAsync(book, book.Id);

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Genre = book.Genre;
            existingBook.Status = book.Status;
            existingBook.ReleaseDate = book.ReleaseDate;
            existingBook.Notes = book.Notes;

            await _context.SaveChangesAsync();
            return existingBook;
        }
        private async Task ValidateSaveAsync(Book book, int? excludeId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                errors.Add("Title is required.");
            }
            else
            {
                var lower = book.Title.Trim().ToLower();
                bool exists = await _context.Books
                    .AnyAsync(b => b.Title.ToLower() == lower && b.Id != excludeId);

                if (exists)
                    errors.Add("A book with the same title already exists.");
            }

            if (book.Genre == null)
                errors.Add("Cannot create a book without a genre.");

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors));
        }
    }
}
