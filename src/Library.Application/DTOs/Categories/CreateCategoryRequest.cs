using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Categories;

public record CreateCategoryRequest(
    [Required(ErrorMessage = "Tên thể loại không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên thể loại không được vượt quá 100 ký tự")]
    string Name,

    [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
    string? Description
);
