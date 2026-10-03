using Library.Application.Common;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.UnitTests;

public class BorrowStatusCalculatorTests
{
    [Fact]
    public void EffectiveStatus_WhenBorrowingAndPastDueDateWithActiveDetail_ReturnsOverdue()
    {
        var record = new BorrowRecord
        {
            Status = BorrowRecordStatus.Borrowing,
            DueDate = DateTime.UtcNow.AddDays(-2),
            BorrowDetails = new List<BorrowDetail>
            {
                new() { Status = BorrowDetailStatus.Borrowing }
            }
        };

        var result = BorrowStatusCalculator.EffectiveStatus(record, DateTime.UtcNow);
        Assert.Equal("Overdue", result);
        Assert.True(BorrowStatusCalculator.IsOverdue(record, DateTime.UtcNow));
    }

    [Fact]
    public void EffectiveStatus_WhenBorrowingAndFutureDueDate_ReturnsBorrowing()
    {
        var record = new BorrowRecord
        {
            Status = BorrowRecordStatus.Borrowing,
            DueDate = DateTime.UtcNow.AddDays(5),
            BorrowDetails = new List<BorrowDetail>
            {
                new() { Status = BorrowDetailStatus.Borrowing }
            }
        };

        var result = BorrowStatusCalculator.EffectiveStatus(record, DateTime.UtcNow);
        Assert.Equal("Borrowing", result);
        Assert.False(BorrowStatusCalculator.IsOverdue(record, DateTime.UtcNow));
    }

    [Fact]
    public void EffectiveStatus_WhenReturned_ReturnsReturned()
    {
        var record = new BorrowRecord
        {
            Status = BorrowRecordStatus.Returned,
            DueDate = DateTime.UtcNow.AddDays(-5),
            BorrowDetails = new List<BorrowDetail>
            {
                new() { Status = BorrowDetailStatus.Returned }
            }
        };

        var result = BorrowStatusCalculator.EffectiveStatus(record, DateTime.UtcNow);
        Assert.Equal("Returned", result);
        Assert.False(BorrowStatusCalculator.IsOverdue(record, DateTime.UtcNow));
    }
}
