using light_brary.Application.DTOs.BookCopies;
using light_brary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api")]
    public class BookCopiesController : ControllerBase
    {
        private readonly IBookCopyService _bookCopyService;

        public BookCopiesController(IBookCopyService bookCopyService)
        {
            _bookCopyService = bookCopyService;
        }

        /// <summary>
        /// Xem danh sách các bản sao của một đầu sách
        /// </summary>
        [HttpGet("books/{bookId:int}/copies")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(IEnumerable<BookCopyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookCopyResponse>>> GetByBookId(int bookId)
        {
            var copies = await _bookCopyService.GetByBookIdAsync(bookId);
            return Ok(copies);
        }

        /// <summary>
        /// Thêm một bản sao vật lý mới cho đầu sách
        /// </summary>
        [HttpPost("books/{bookId:int}/copies")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> Create(int bookId, [FromBody] SaveBookCopyRequest request)
        {
            var created = await _bookCopyService.CreateAsync(bookId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Xem chi tiết một bản sao sách theo ID
        /// </summary>
        [HttpGet("book-copies/{id:int}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookCopyResponse>> GetById(int id)
        {
            var copy = await _bookCopyService.GetByIdAsync(id);
            return Ok(copy);
        }

        /// <summary>
        /// Tìm bản sao sách bằng mã vạch (Barcode)
        /// </summary>
        [HttpGet("book-copies/by-barcode/{barcode}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookCopyResponse>> GetByBarcode(string barcode)
        {
            var copy = await _bookCopyService.GetByBarcodeAsync(barcode);
            return Ok(copy);
        }

        /// <summary>
        /// Sửa barcode hoặc condition note của bản sao
        /// </summary>
        [HttpPut("book-copies/{id:int}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> Update(int id, [FromBody] SaveBookCopyRequest request)
        {
            var updated = await _bookCopyService.UpdateAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// Xóa BookCopy nếu chưa có lịch sử mượn
        /// </summary>
        [HttpDelete("book-copies/{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            await _bookCopyService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Khôi phục sách mất (Lost) trở lại trạng thái Available
        /// </summary>
        [HttpPost("book-copies/{id:int}/mark-available")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> MarkAvailable(int id)
        {
            var copy = await _bookCopyService.MarkAvailableAsync(id);
            return Ok(copy);
        }

        /// <summary>
        /// Đánh dấu sách mất ngoài nghiệp vụ mượn (Available -> Lost)
        /// </summary>
        [HttpPost("book-copies/{id:int}/mark-lost")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> MarkLost(int id)
        {
            var copy = await _bookCopyService.MarkLostAsync(id);
            return Ok(copy);
        }
    }
}

