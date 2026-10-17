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
        // User ........................................................................................................................................................

        // USER xem thong tin ca nhan
        [HttpGet("api/me")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await userService.GetByIdAsync(userId);
            return Ok(profile);
        }

        // USER cap nhat thong tin ca nhan
        [HttpPut("api/me")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> UpdateMyProfile(UpdateProfileRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updated = await userService.UpdateProfileAsync(userId, request);
            return Ok(updated);
        }

         // Admin: ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,

        // ADMIN : xoa user
        [HttpDelete("api/users/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await userService.DeleteAsync(id);
            return NoContent();
        }


        // Thủ thư và Admin: tra cứu và hỗ trợ người dùng .....................................................................................................

        // ADMIN % LIBRARIAN : lay tat ca user (role = USER)
        [HttpGet("api/users")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(
            [FromQuery] string? keyword = null,
            [FromQuery] string? role = null)
        {
            var users = await userService.GetAllAsync(keyword, role);
            return Ok(users);
        }

        // ADMIN % LIBRARIAN : lay user theo id
        [HttpGet("api/users/{id:guid}")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<UserResponse>> GetById(Guid id)
        {
            var user = await userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("api/users/{id:guid}/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetUserBorrowRecords(Guid id)
        {
            var records = await userService.GetUserBorrowRecordsAsync(id);
            return Ok(records);
        }

    }
}
