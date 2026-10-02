using Library.Application.DTOs.BorrowRecords;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetAll()
    {
        var records = await _borrowRecordService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một phiếu mượn
    /// </summary>
    [HttpGet("borrow-records/{id:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "StaffOrAdmin")]
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
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(List<BorrowRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetByUserId(string userId)
    {
        var records = await _borrowRecordService.GetByUserIdAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Lấy lịch sử mượn sách của bản thân
    /// </summary>
    [HttpGet("my/borrow-records")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<List<BorrowRecordResponse>>> GetMyBorrowRecords()
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) 
                     ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var records = await _borrowRecordService.GetByUserIdAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Tạo phiếu mượn sách mới
    /// </summary>
    [HttpPost("borrow-records")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "StaffOrAdmin")]
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
    [HttpPatch("borrow-records/{id:int}/return")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "StaffOrAdmin")]
    [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowRecordResponse>> ReturnBooks(int id, [FromBody] ReturnBooksRequest request)
    {
        var updated = await _borrowRecordService.ReturnBooksAsync(id, request);
        return Ok(updated);
    }
}
