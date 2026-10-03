namespace Library.Application.DTOs.Categories;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    int BookCount
);
