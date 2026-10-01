using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Books;

public record UpdateBookRequest(
    [Required(ErrorMessage = "Tiêu đề sách không được để trống")]
    [MaxLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự")]
    string Title,

    [Required(ErrorMessage = "Tác giả không được để trống")]
    [MaxLength(150, ErrorMessage = "Tên tác giả không được vượt quá 150 ký tự")]
    string Author,

    [MaxLength(50, ErrorMessage = "ISBN không được vượt quá 50 ký tự")]
    string? Isbn,

    [Range(1000, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
    int? PublishYear,

    [Required(ErrorMessage = "Thể loại sách không được để trống")]
    int CategoryId
);
