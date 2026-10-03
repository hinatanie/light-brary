namespace Library.Application.DTOs.BookCopies;

public record BookCopyResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    string Barcode,
    string Status,
    string? ConditionNote
);
