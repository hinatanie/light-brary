using light_brary.Services;
using System.Security.Claims;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    public class UsersController(UserService userService) : ControllerBase
    {
        #region Admin / Librarian User Management

        [HttpGet("api/users")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(
            [FromQuery] string? keyword = null,
            [FromQuery] string? role = null)
        {
            var users = await userService.GetAllAsync(keyword, role);
            return Ok(users);
        }

        [HttpGet("api/users/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<UserResponse>> GetById(Guid id)
        {
            var user = await userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpPost("api/users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
        {
            var created = await userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("api/users/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request)
        {
            var updated = await userService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("api/users/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await userService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("api/users/{id:guid}/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetUserBorrowRecords(Guid id)
        {
            var records = await userService.GetUserBorrowRecordsAsync(id);
            return Ok(records);
        }

        [HttpPut("api/users/{id:guid}/password")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<IActionResult> AdminUpdatePassword(Guid id, AdminUpdatePasswordRequest request)
        {
            await userService.AdminUpdatePasswordAsync(id, request);
            return Ok(new { message = "Cập nhật mật khẩu người dùng thành công." });
        }

        #endregion

        #region /api/me Endpoints

        [HttpGet("api/me")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("api/me")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> UpdateMyProfile(UpdateUserRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updated = await userService.UpdateProfileAsync(userId, request);
            return Ok(updated);
        }

        [HttpPut("api/me/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await userService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpGet("api/me/borrow-records")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetMyBorrowRecords()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var records = await userService.GetUserBorrowRecordsAsync(userId);
            return Ok(records);
        }

        #endregion

    }
}

