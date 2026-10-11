using light_brary.Services;
using System.Security.Claims;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly BorrowRecordService _borrowRecordService;

        public BorrowRecordsController(BorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        [HttpPost("api/me/borrow")]
        [HttpPost("api/borrow-records/borrow")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BorrowRecordResponse>> MemberBorrow([FromBody] MemberBorrowRequest request)
        {
            var userId = GetCurrentUserId();
            var created = await _borrowRecordService.BorrowForMemberAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost("api/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BorrowRecordResponse>> Create([FromBody] CreateBorrowRecordRequest request)
        {
            var created = await _borrowRecordService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("api/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<IEnumerable<BorrowRecordSummaryResponse>>> GetAll(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? overdue = null)
        {
            var records = await _borrowRecordService.GetAllAsync(userId, status, overdue);
            return Ok(records);
        }

        [HttpGet("api/borrow-records/{id:guid}")]
        [Authorize]
        public async Task<ActionResult<BorrowRecordResponse>> GetById(Guid id)
        {
            var record = await _borrowRecordService.GetByIdAsync(id);
            var userId = GetCurrentUserId();
            var isStaffOrAdmin = User.IsInRole(Role.ADMIN) || User.IsInRole(Role.LIBRARIAN);
            if (!isStaffOrAdmin && record.User.Id != userId)
            {
                return Forbid();
            }
            return Ok(record);
        }

        [HttpPost("api/borrow-details/{id:guid}/return")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BorrowDetailActionResponse>> ReturnSingleBook(Guid id)
        {
            var result = await _borrowRecordService.ReturnSingleBookAsync(id);
            return Ok(result);
        }

        [HttpPost("api/returns")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<ReturnBooksResponse>> ReturnMultipleBooks([FromBody] ReturnBooksRequest request)
        {
            var result = await _borrowRecordService.ReturnMultipleBooksAsync(request);
            return Ok(result);
        }

        [HttpPost("api/borrow-details/{id:guid}/mark-lost")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BorrowDetailActionResponse>> MarkLostBook(Guid id)
        {
            var result = await _borrowRecordService.MarkLostBookAsync(id);
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var userId))
                throw new UnauthorizedException("Không xác định được danh tính người dùng.");

            return userId;
        }
    }
}
