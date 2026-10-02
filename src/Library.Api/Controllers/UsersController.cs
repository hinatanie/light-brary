using Library.Application.DTOs.Users;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng
    /// </summary>
    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một người dùng theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(user);
    }

    [HttpPut("{id}/roles")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequest request)
    {
        await _userService.UpdateUserRolesAsync(id, request);
        return NoContent();
    }

    [HttpGet("{id}/roles")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<string>>> GetUserRoles(string id)
    {
        var roles = await _userService.GetUserRolesAsync(id);
        return Ok(roles);
    }

    [HttpPut("{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<UserResponse>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) 
                            ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && currentUserId != id) return Forbid();

        var updated = await _userService.UpdateUserAsync(id, request);
        return Ok(updated);
    }
}
