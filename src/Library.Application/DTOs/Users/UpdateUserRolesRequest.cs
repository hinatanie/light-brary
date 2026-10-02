using System.ComponentModel.DataAnnotations;
namespace Library.Application.DTOs.Users;
public record UpdateUserRolesRequest(
    [Required] List<string> RoleNames
);
