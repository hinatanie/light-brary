using System.Security.Claims;
using Library.Application.DTOs.BorrowRecords;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
    /// Lấy danh sách tất cả phiếu mượn (hỗ trợ lọc status: Borrowing, Returned, Overdue)
    /// </summary>
    [HttpGet("borrow-records")]
    [Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetAll([FromQuery] string? status = null)
    {
        var records = await _borrowRecordService.GetAllAsync(status);
        return Ok(records);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một phiếu mượn
    /// </summary>
    [HttpGet("borrow-records/{id:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> GetById(Guid id)
    {
        var record = await _borrowRecordService.GetByIdAsync(id);
        return Ok(record);
    }

    /// <summary>
    /// Lấy lịch sử mượn sách của một người dùng cụ thể
    /// </summary>
    [HttpGet("users/{userId:guid}/borrow-records")]
    [Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetByUserId(Guid userId)
    {
        var records = await _borrowRecordService.GetByUserIdAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Lấy lịch sử mượn sách của bản thân
    /// </summary>
    [HttpGet("my/borrow-records")]
    [Authorize]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetMyBorrowRecords()
    {
        var userId = GetCurrentUserId();
        var records = await _borrowRecordService.GetByUserIdAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Tạo phiếu mượn sách mới
    /// </summary>
    [HttpPost("borrow-records")]
    [Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> Create([FromBody] CreateBorrowRequest request)
    {
        var created = await _borrowRecordService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Xử lý trả sách
    /// </summary>
    [HttpPatch("borrow-records/{id:guid}/return")]
    [Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> ReturnBooks(Guid id, [FromBody] ReturnBooksRequest request)
    {
        var updated = await _borrowRecordService.ReturnBooksAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// Báo mất sách trong một phiếu mượn
    /// </summary>
    [HttpPatch("borrow-records/{id:guid}/details/{detailId:guid}/lost")]
    [Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> MarkLost(Guid id, Guid detailId, [FromBody] MarkLostRequest request)
    {
        var updated = await _borrowRecordService.MarkLostAsync(id, detailId, request);
        return Ok(updated);
    }

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var guid))
            throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        return guid;
    }
}
