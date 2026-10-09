using System.Security.Claims;
using light_brary.Application.DTOs.BorrowRecords;
using light_brary.Application.Exceptions;
using light_brary.Application.Interfaces;
using light_brary.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly IBorrowRecordService _borrowRecordService;

        public BorrowRecordsController(IBorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        /// <summary>
        /// Độc giả đăng nhập tự mượn sách trực tuyến (hệ thống tự chọn bản sao khả dụng trên kệ)
        /// </summary>
        [HttpPost("api/me/borrow")]
        [HttpPost("api/borrow-records/borrow")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BorrowRecordResponse>> MemberBorrow([FromBody] MemberBorrowRequest request)
        {
            var userId = GetCurrentUserId();
            var created = await _borrowRecordService.BorrowForMemberAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Tạo phiếu mượn sách mới bởi thủ thư/quản trị viên (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BorrowRecordResponse>> Create([FromBody] CreateBorrowRecordRequest request)
        {
            var created = await _borrowRecordService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Lấy danh sách phiếu mượn, hỗ trợ lọc theo userId, status, overdue (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(IEnumerable<BorrowRecordSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowRecordSummaryResponse>>> GetAll(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? overdue = null)
        {
            var records = await _borrowRecordService.GetAllAsync(userId, status, overdue);
            return Ok(records);
        }

        /// <summary>
        /// Xem chi tiết một phiếu mượn theo ID (Admin, Librarian hoặc chính độc giả sở hữu)
        /// </summary>
        [HttpGet("api/borrow-records/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Trả một cuốn sách theo ID chi tiết mượn (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/borrow-details/{id:guid}/return")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BorrowDetailActionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BorrowDetailActionResponse>> ReturnSingleBook(Guid id)
        {
            var result = await _borrowRecordService.ReturnSingleBookAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Trả nhiều cuốn sách trong một lượt (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/returns")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(ReturnBooksResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ReturnBooksResponse>> ReturnMultipleBooks([FromBody] ReturnBooksRequest request)
        {
            var result = await _borrowRecordService.ReturnMultipleBooksAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Báo mất một cuốn sách đang mượn theo ID chi tiết mượn (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/borrow-details/{id:guid}/mark-lost")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(BorrowDetailActionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
