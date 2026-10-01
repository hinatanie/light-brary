using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.BorrowRecords;

public record CreateBorrowRequest(
    [Required(ErrorMessage = "Mã người dùng không được để trống")]
    string UserId,

    [Required(ErrorMessage = "Hạn trả sách không được để trống")]
    DateTime DueDate,

    [Required(ErrorMessage = "Phải chọn ít nhất một bản sao sách để mượn")]
    [MinLength(1, ErrorMessage = "Phải chọn ít nhất một bản sao sách để mượn")]
    List<int> BookCopyIds,

    [MaxLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
    string? Note
);
