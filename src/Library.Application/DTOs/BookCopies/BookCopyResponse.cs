namespace Library.Application.DTOs.BookCopies;

public record BookCopyResponse(
    int Id,
    int BookId,
    string BookTitle,
    string BookAuthor,
    string Barcode,
    string Status,
    string? ConditionNote
);
