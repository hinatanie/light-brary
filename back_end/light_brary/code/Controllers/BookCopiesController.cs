using light_brary.Services;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api")]
    public class BookCopiesController : ControllerBase
    {
        private readonly BookCopyService _bookCopyService;

        public BookCopiesController(BookCopyService bookCopyService)
        {
            _bookCopyService = bookCopyService;
        }

        [HttpGet("books/{bookId:guid}/copies")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookCopyResponse>>> GetByBookId(Guid bookId)
        {
            var copies = await _bookCopyService.GetByBookIdAsync(bookId);
            return Ok(copies);
        }

        [HttpPost("books/{bookId:guid}/copies")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> Create(Guid bookId, [FromBody] SaveBookCopyRequest request)
        {
            var created = await _bookCopyService.CreateAsync(bookId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("book-copies/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> GetById(Guid id)
        {
            var copy = await _bookCopyService.GetByIdAsync(id);
            return Ok(copy);
        }

        [HttpGet("book-copies/by-barcode/{barcode}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> GetByBarcode(string barcode)
        {
            var copy = await _bookCopyService.GetByBarcodeAsync(barcode);
            return Ok(copy);
        }

        [HttpPut("book-copies/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> Update(Guid id, [FromBody] SaveBookCopyRequest request)
        {
            var updated = await _bookCopyService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("book-copies/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bookCopyService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("book-copies/{id:guid}/mark-available")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> MarkAvailable(Guid id)
        {
            var copy = await _bookCopyService.MarkAvailableAsync(id);
            return Ok(copy);
        }

        [HttpPost("book-copies/{id:guid}/mark-lost")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> MarkLost(Guid id)
        {
            var copy = await _bookCopyService.MarkLostAsync(id);
            return Ok(copy);
        }
    }
}

