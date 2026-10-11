using light_brary.Services;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController(BookService bookService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll(
            [FromQuery] string? keyword = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? author = null,
            [FromQuery] bool? onlyAvailable = null)
        {
            var books = await bookService.GetAllAsync(keyword, categoryId, author, onlyAvailable);
            return Ok(books);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookResponse>> GetById(Guid id)
        {
            var book = await bookService.GetByIdAsync(id);
            return Ok(book);
        }

        [HttpPost]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookResponse>> Create(SaveBookRequest request)
        {
            var created = await bookService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookResponse>> Update(Guid id, SaveBookRequest request)
        {
            var updated = await bookService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await bookService.DeleteAsync(id);
            return NoContent();
        }
    }
}

