using System.ComponentModel.DataAnnotations;
using Library.Application.Common;

namespace Library.Application.DTOs.BorrowRecords;

public record ReturnBooksRequest(
    [Required(ErrorMessage = "Phải chọn ít nhất một cuốn để trả")]
    [MinLength(1, ErrorMessage = "Phải chọn ít nhất một cuốn để trả")]
    List<ReturnBookItem> Items
);

public record ReturnBookItem(
    [Required(ErrorMessage = "Mã chi tiết mượn không được để trống")]
    int BorrowDetailId,

    [Range(0, AuthConstants.MaxFineAmount, ErrorMessage = "Tiền phạt phải từ 0 đến 10.000.000")]
    decimal FineAmount
);

/// <summary>Báo mất một cuốn sách trong phiếu mượn.</summary>
public record MarkLostRequest(
    [Range(0, AuthConstants.MaxFineAmount, ErrorMessage = "Tiền phạt phải từ 0 đến 10.000.000")]
    decimal FineAmount
);
