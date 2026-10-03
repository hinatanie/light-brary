namespace Library.Domain.Enums;

public enum BorrowRecordStatus : byte
{
    Borrowing = 0,
    Returned = 1,

    /// <summary>
    /// Trạng thái hiển thị (suy ra khi đọc): phiếu còn sách chưa trả và đã quá hạn.
    /// Giá trị này KHÔNG được lưu xuống DB, xem BorrowStatusCalculator trong tầng Application.
    /// </summary>
    Overdue = 2
}
