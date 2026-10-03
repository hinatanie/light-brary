namespace Library.Application.DTOs.Books;

public record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string? Isbn,
    int? PublishYear,
    string? ImageUrl,
    Guid CategoryId,
    string? CategoryName,
    DateTime CreatedAt
);
