namespace Library.Application.DTOs.BorrowRecords;

public record BorrowRecordResponse(
    int Id,
    Guid UserId,
    string UserFullName,
    DateTime BorrowDate,
    DateTime DueDate,
    string Status,
    string? Note,
    List<BorrowDetailResponse> Details
);

public record BorrowDetailResponse(
    int Id,
    int BookCopyId,
    string Barcode,
    string BookTitle,
    string BookAuthor,
    DateTime? ReturnDate,
    string Status,
    decimal FineAmount,
    bool IsOverdue
);
