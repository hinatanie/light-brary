using light_brary.Services;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Policy = "AdminOnly")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Lấy danh sách tất cả các vai trò (Chỉ Admin)
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Xem chi tiết một vai trò theo ID (Chỉ Admin)
        /// </summary>
        [HttpGet("roles/{id:guid}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleResponse>> GetById(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            return Ok(role);
        }

        /// <summary>
        /// Tạo vai trò mới (Chỉ Admin)
        /// </summary>
        [HttpPost("roles")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RoleResponse>> Create([FromBody] SaveRoleRequest request)
        {
            var created = await _roleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Đổi tên vai trò (Chỉ Admin, cấm đổi tên Admin/Librarian/User)
        /// </summary>
        [HttpPut("roles/{id:guid}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RoleResponse>> Update(Guid id, [FromBody] SaveRoleRequest request)
        {
            var updated = await _roleService.UpdateAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// Xóa vai trò (Chỉ Admin, cấm xóa role hệ thống hoặc role đang gán cho user)
        /// </summary>
        [HttpDelete("roles/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách các vai trò của một người dùng (Chỉ Admin)
        /// </summary>
        [HttpGet("users/{userId:guid}/roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetUserRoles(Guid userId)
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        /// <summary>
        /// Gán vai trò cho người dùng (Chỉ Admin)
        /// </summary>
        [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
        {
            await _roleService.AssignRoleToUserAsync(userId, roleId);
            return NoContent();
        }

        /// <summary>
        /// Gỡ vai trò khỏi người dùng (Chỉ Admin, chặn gỡ Admin cuối cùng)
        /// </summary>
        [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
        {
            await _roleService.RemoveRoleFromUserAsync(userId, roleId);
            return NoContent();
        }
    }
}

