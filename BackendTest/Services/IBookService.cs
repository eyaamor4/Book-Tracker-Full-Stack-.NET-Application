using BackendTest.Models;

namespace BackendTest.Services
{
    public interface IBookService
    {
        Task<Book> CreateBookAsync(Book book);
        Task<Book?> GetBookByIdAsync(int id);
        Task<List<Book>> GetBooksAsync(string? search, int skip=0, int take = 5);
        Task<Book> UpdateBookAsync(Book book);
        Task DeleteBooksAsync(List<int> ids);   

    }
}
