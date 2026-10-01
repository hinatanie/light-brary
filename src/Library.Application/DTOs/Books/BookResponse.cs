namespace Library.Application.DTOs.Books;

public record BookResponse(
    int Id,
    string Title,
    string Author,
    string? Isbn,
    int? PublishYear,
    int CategoryId,
    string? CategoryName,
    DateTime CreatedAt
);
