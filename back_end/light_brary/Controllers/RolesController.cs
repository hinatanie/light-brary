using light_brary.Application.DTOs.Roles;
using light_brary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Policy = "AdminOnly")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
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
        [HttpGet("roles/{id:int}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleResponse>> GetById(int id)
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
        [HttpPut("roles/{id:int}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RoleResponse>> Update(int id, [FromBody] SaveRoleRequest request)
        {
            var updated = await _roleService.UpdateAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// Xóa vai trò (Chỉ Admin, cấm xóa role hệ thống hoặc role đang gán cho user)
        /// </summary>
        [HttpDelete("roles/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách các vai trò của một người dùng (Chỉ Admin)
        /// </summary>
        [HttpGet("users/{userId:int}/roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetUserRoles(int userId)
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        /// <summary>
        /// Gán vai trò cho người dùng (Chỉ Admin)
        /// </summary>
        [HttpPost("users/{userId:int}/roles/{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            await _roleService.AssignRoleToUserAsync(userId, roleId);
            return NoContent();
        }

        /// <summary>
        /// Gỡ vai trò khỏi người dùng (Chỉ Admin, chặn gỡ Admin cuối cùng)
        /// </summary>
        [HttpDelete("users/{userId:int}/roles/{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole(int userId, int roleId)
        {
            await _roleService.RemoveRoleFromUserAsync(userId, roleId);
            return NoContent();
        }
    }
}

