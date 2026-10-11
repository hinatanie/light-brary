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
    public class RolesController(RoleService roleService) : ControllerBase
    {
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            var roles = await roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("roles/{id:guid}")]
        public async Task<ActionResult<RoleResponse>> GetById(Guid id)
        {
            var role = await roleService.GetByIdAsync(id);
            return Ok(role);
        }

        [HttpPost("roles")]
        public async Task<ActionResult<RoleResponse>> Create(SaveRoleRequest request)
        {
            var created = await roleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("roles/{id:guid}")]
        public async Task<ActionResult<RoleResponse>> Update(Guid id, SaveRoleRequest request)
        {
            var updated = await roleService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("roles/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await roleService.DeleteAsync(id);
            return NoContent();
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

