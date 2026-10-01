using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.BookCopies;

public record CreateBookCopyRequest(
    [Required(ErrorMessage = "Mã đầu sách không được để trống")]
    int BookId,

    [Required(ErrorMessage = "Mã vạch không được để trống")]
    [MaxLength(50, ErrorMessage = "Mã vạch không được vượt quá 50 ký tự")]
    string Barcode,

    [MaxLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
    string? ConditionNote
);
