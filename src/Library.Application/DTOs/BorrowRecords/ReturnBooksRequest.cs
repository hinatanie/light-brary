using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.BorrowRecords;

public record ReturnBooksRequest(
    [Required(ErrorMessage = "Phải chọn ít nhất một cuốn để trả")]
    [MinLength(1, ErrorMessage = "Phải chọn ít nhất một cuốn để trả")]
    List<ReturnBookItem> Items
);

public record ReturnBookItem(
    [Required(ErrorMessage = "Mã chi tiết mượn không được để trống")]
    int BorrowDetailId,

    [Range(0, double.MaxValue, ErrorMessage = "Tiền phạt không được âm")]
    decimal FineAmount
);
