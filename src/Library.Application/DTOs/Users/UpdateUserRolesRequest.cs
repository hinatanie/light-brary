using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Users;

public record UpdateUserRolesRequest(
    [Required(ErrorMessage = "Danh sách vai trò không được để trống")]
    [MinLength(1, ErrorMessage = "Người dùng phải có ít nhất một vai trò")]
    List<string> RoleNames
);
