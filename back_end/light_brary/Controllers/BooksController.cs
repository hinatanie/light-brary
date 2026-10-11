using light_brary.Services;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;

        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Lấy danh sách đầu sách, hỗ trợ tìm kiếm và lọc (Công khai)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll(
            [FromQuery] string? keyword = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? author = null,
            [FromQuery] bool? onlyAvailable = null)
        {
            var books = await _bookService.GetAllAsync(keyword, categoryId, author, onlyAvailable);
            return Ok(books);
        }

        /// <summary>
        /// Xem chi tiết một đầu sách theo ID (Công khai)
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookResponse>> GetById(Guid id)
        {
            var book = await _bookService.GetByIdAsync(id);
            return Ok(book);
        }

        /// <summary>
        /// Thêm mới đầu sách (Librarian hoặc Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookResponse>> Create([FromBody] SaveBookRequest request)
        {
            var created = await _bookService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cập nhật thông tin đầu sách (Librarian hoặc Admin)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookResponse>> Update(Guid id, [FromBody] SaveBookRequest request)
        {
            var updated = await _bookService.UpdateAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// Xóa đầu sách (Chỉ Admin, chặn nếu còn BookCopy)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bookService.DeleteAsync(id);
            return NoContent();
        }
    }
}

