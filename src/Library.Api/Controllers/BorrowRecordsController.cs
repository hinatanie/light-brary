using Library.Application.DTOs.BorrowRecords;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class BorrowRecordsController : ControllerBase
{
    private readonly IBorrowRecordService _borrowRecordService;

    public BorrowRecordsController(IBorrowRecordService borrowRecordService)
    {
        _borrowRecordService = borrowRecordService;
    }

    /// <summary>
    /// Lấy danh sách tất cả phiếu mượn
    /// </summary>
    [HttpGet("borrow-records")]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetAll()
    {
        var records = await _borrowRecordService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một phiếu mượn (kèm danh sách sách đã mượn)
    /// </summary>
    [HttpGet("borrow-records/{id:int}")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> GetById(int id)
    {
        var record = await _borrowRecordService.GetByIdAsync(id);
        return Ok(record);
    }

    /// <summary>
    /// Lấy lịch sử mượn sách của một người dùng cụ thể
    /// </summary>
    [HttpGet("users/{userId}/borrow-records")]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetByUserId(string userId)
    {
        var records = await _borrowRecordService.GetByUserIdAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Tạo phiếu mượn sách mới (một hoặc nhiều cuốn cùng lúc)
    /// </summary>
    [HttpPost("borrow-records")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> Create([FromBody] CreateBorrowRequest request)
    {
        var created = await _borrowRecordService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Xử lý trả sách (một hoặc nhiều cuốn trong cùng phiếu mượn)
    /// </summary>
    [HttpPatch("borrow-records/{id:int}/return")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> ReturnBooks(int id, [FromBody] ReturnBooksRequest request)
    {
        var updated = await _borrowRecordService.ReturnBooksAsync(id, request);
        return Ok(updated);
    }
}
