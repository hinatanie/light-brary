using light_brary.Services;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Policy = "AdminOnly")]
    public class RolesController(RoleService roleService) : ControllerBase
    {
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            var roles = await roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("users/{userId:guid}/roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetUserRoles(Guid userId)
        {
            var roles = await roleService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
        public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
        {
            await roleService.AssignRoleToUserAsync(userId, roleId);
            return NoContent();
        }

        [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
        public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
        {
            await roleService.RemoveRoleFromUserAsync(userId, roleId);
            return NoContent();
        }
    }
}

