using BackendTest.Models;
using BackendTest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET api/books?search=abc&skip=0&take=5
        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 5)
        {
            return Ok(await _bookService.GetBooksAsync(search, skip, take));
        }

        // GET api/books/3
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound(new { message = "Book not found." });
            return Ok(book);
        }

        // POST api/books
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            try
            {
                var created = await _bookService.CreateBookAsync(book);
                return CreatedAtAction(nameof(GetBookById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/books/3
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            book.Id = id;
            try
            {
                return Ok(await _bookService.UpdateBookAsync(book));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/books/delete   body: [3, 7, 12]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteBooks([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest(new { message = "No book selected." });

            try
            {
                await _bookService.DeleteBooksAsync(ids);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}