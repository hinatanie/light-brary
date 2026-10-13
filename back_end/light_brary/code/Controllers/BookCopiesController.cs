using light_brary.Services;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api")]
    public class BookCopiesController(BookCopyService bookCopyService) : ControllerBase
    {
        [HttpGet("books/{bookId:guid}/copies")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookCopyResponse>>> GetByBookId(Guid bookId)
        {
            var copies = await bookCopyService.GetByBookIdAsync(bookId);
            return Ok(copies);
        }

        [HttpPost("books/{bookId:guid}/copies")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> Create(Guid bookId, SaveBookCopyRequest request)
        {
            var created = await bookCopyService.CreateAsync(bookId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("book-copies/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> GetById(Guid id)
        {
            var copy = await bookCopyService.GetByIdAsync(id);
            return Ok(copy);
        }

        [HttpGet("book-copies/by-barcode/{barcode}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> GetByBarcode(string barcode)
        {
            var copy = await bookCopyService.GetByBarcodeAsync(barcode);
            return Ok(copy);
        }

        [HttpPut("book-copies/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BookCopyResponse>> Update(Guid id, SaveBookCopyRequest request)
        {
            var updated = await bookCopyService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("book-copies/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await bookCopyService.DeleteAsync(id);
            return NoContent();
        }
    }
}

