using Library.Application.DTOs.BookCopies;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "StaffOrAdmin")]
public class BookCopiesController : ControllerBase
{
    private readonly IBookCopyService _bookCopyService;

    public BookCopiesController(IBookCopyService bookCopyService)
    {
        _bookCopyService = bookCopyService;
    }

    /// <summary>
    /// Lấy danh sách tất cả bản sao vật lý của một đầu sách
    /// </summary>
    [HttpGet("books/{bookId:int}/copies")]
    [ProducesResponseType(typeof(List<BookCopyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookCopyResponse>>> GetByBookId(int bookId)
    {
        var copies = await _bookCopyService.GetByBookIdAsync(bookId);
        return Ok(copies);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một bản sao sách theo ID
    /// </summary>
    [HttpGet("book-copies/{id:int}")]
    [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookCopyResponse>> GetById(int id)
    {
        var copy = await _bookCopyService.GetByIdAsync(id);
        return Ok(copy);
    }

    /// <summary>
    /// Thêm một bản sao sách mới vào kệ thư viện
    /// </summary>
    [HttpPost("book-copies")]
    [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookCopyResponse>> Create([FromBody] CreateBookCopyRequest request)
    {
        var created = await _bookCopyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Cập nhật ghi chú tình trạng bản sao sách (mới, cũ, rách bìa...)
    /// </summary>
    [HttpPatch("book-copies/{id:int}/condition")]
    [ProducesResponseType(typeof(BookCopyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookCopyResponse>> UpdateCondition(int id, [FromBody] UpdateBookCopyConditionRequest request)
    {
        var updated = await _bookCopyService.UpdateConditionAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// Xóa bản sao sách khỏi hệ thống (chặn nếu đang được mượn)
    /// </summary>
    [HttpDelete("book-copies/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _bookCopyService.DeleteAsync(id);
        return NoContent();
    }
}
