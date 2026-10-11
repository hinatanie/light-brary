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

        /// <summary>
        /// Xem danh sách các bản sao của một đầu sách
        /// </summary>
        [HttpGet("books/{bookId:guid}/copies")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<BookCopyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookCopyResponse>>> GetByBookId(Guid bookId)
        {
            var copies = await _bookCopyService.GetByBookIdAsync(bookId);
            return Ok(copies);
        }

        /// <summary>
        /// Thêm một bản sao vật lý mới cho đầu sách
        /// </summary>
        [HttpPost("books/{bookId:guid}/copies")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> Create(Guid bookId, [FromBody] SaveBookCopyRequest request)
        {
            var created = await _bookCopyService.CreateAsync(bookId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Xem chi tiết một bản sao sách theo ID
        /// </summary>
        [HttpGet("book-copies/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookCopyResponse>> GetById(Guid id)
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
        [HttpPut("book-copies/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> Update(Guid id, [FromBody] SaveBookCopyRequest request)
        {
            var updated = await _bookCopyService.UpdateAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// Xóa BookCopy nếu chưa có lịch sử mượn
        /// </summary>
        [HttpDelete("book-copies/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bookCopyService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Khôi phục sách mất (Lost) trở lại trạng thái Available
        /// </summary>
        [HttpPost("book-copies/{id:guid}/mark-available")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> MarkAvailable(Guid id)
        {
            var copy = await _bookCopyService.MarkAvailableAsync(id);
            return Ok(copy);
        }

        /// <summary>
        /// Đánh dấu sách mất ngoài nghiệp vụ mượn (Available -> Lost)
        /// </summary>
        [HttpPost("book-copies/{id:guid}/mark-lost")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookCopyResponse>> MarkLost(Guid id)
        {
            var copy = await _bookCopyService.MarkLostAsync(id);
            return Ok(copy);
        }
    }
}

