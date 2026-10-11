using System.Security.Claims;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        #region Admin / Librarian User Management

        /// <summary>
        /// Lấy danh sách người dùng (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/users")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(
            [FromQuery] string? keyword = null,
            [FromQuery] string? role = null)
        {
            var users = await _userService.GetAllAsync(keyword, role);
            return Ok(users);
        }

        /// <summary>
        /// Xem chi tiết thông tin một người dùng theo ID (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/users/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        /// <summary>
        /// Tạo mới người dùng bởi quản trị viên (Chỉ Admin)
        /// </summary>
        [HttpPost("api/users")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
        {
            var created = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cập nhật thông tin người dùng (Chỉ Admin)
        /// </summary>
        [HttpPut("api/users/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var updated = await _userService.UpdateAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// Xóa người dùng (Chỉ Admin, chặn nếu có lịch sử mượn hoặc là Admin cuối cùng)
        /// </summary>
        [HttpDelete("api/users/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Xem lịch sử mượn của một người dùng (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/users/{id:guid}/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(IEnumerable<BorrowRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetUserBorrowRecords(Guid id)
        {
            var records = await _userService.GetUserBorrowRecordsAsync(id);
            return Ok(records);
        }

        /// <summary>
        /// Xem những cuốn sách mà người dùng đang mượn (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/users/{id:guid}/borrowed-books")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(typeof(IEnumerable<BorrowedBookResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BorrowedBookResponse>>> GetUserBorrowedBooks(Guid id)
        {
            var books = await _userService.GetUserBorrowedBooksAsync(id);
            return Ok(books);
        }

        /// <summary>
        /// Cập nhật / Đặt lại mật khẩu của người dùng (Admin hoặc Librarian)
        /// </summary>
        [HttpPut("api/users/{id:guid}/password")]
        [HttpPost("api/users/{id:guid}/reset-password")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdminUpdatePassword(Guid id, [FromBody] AdminUpdatePasswordRequest request)
        {
            await _userService.AdminUpdatePasswordAsync(id, request);
            return Ok(new { message = "Cập nhật mật khẩu người dùng thành công." });
        }

        #endregion

        #region /api/me Endpoints

        /// <summary>
        /// Độc giả/Nhân viên xem hồ sơ cá nhân của chính mình
        /// </summary>
        [HttpGet("api/me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserResponse>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân của chính mình
        /// </summary>
        [HttpPut("api/me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponse>> UpdateMyProfile([FromBody] UpdateUserRequest request)
        {
            var userId = GetCurrentUserId();
            var updated = await _userService.UpdateProfileAsync(userId, request);
            return Ok(updated);
        }

        /// <summary>
        /// Người dùng tự đổi mật khẩu của chính mình
        /// </summary>
        [HttpPut("api/me/password")]
        [HttpPost("api/me/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        /// <summary>
        /// Xem lịch sử mượn sách của bản thân
        /// </summary>
        [HttpGet("api/me/borrow-records")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<BorrowRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetMyBorrowRecords()
        {
            var userId = GetCurrentUserId();
            var records = await _userService.GetUserBorrowRecordsAsync(userId);
            return Ok(records);
        }

        /// <summary>
        /// Xem danh sách sách bản thân đang mượn
        /// </summary>
        [HttpGet("api/me/borrowed-books")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<BorrowedBookResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<BorrowedBookResponse>>> GetMyBorrowedBooks()
        {
            var userId = GetCurrentUserId();
            var books = await _userService.GetUserBorrowedBooksAsync(userId);
            return Ok(books);
        }

        #endregion

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

