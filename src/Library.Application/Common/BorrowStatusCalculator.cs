using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Common;

/// <summary>
/// Quá hạn là trạng thái SUY RA lúc đọc (không lưu DB) để luôn đúng mà không cần job nền.
/// </summary>
public static class BorrowStatusCalculator
{
    public static bool IsOverdue(BorrowRecord record, DateTime nowUtc) =>
        record.Status == BorrowRecordStatus.Borrowing
        && record.DueDate < nowUtc
        && record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing);

    public static string EffectiveStatus(BorrowRecord record, DateTime nowUtc) =>
        IsOverdue(record, nowUtc) ? nameof(BorrowRecordStatus.Overdue) : record.Status.ToString();

    public static bool IsDetailOverdue(BorrowRecord record, BorrowDetail detail, DateTime nowUtc) =>
        detail.Status == BorrowDetailStatus.Borrowing && record.DueDate < nowUtc;
}
