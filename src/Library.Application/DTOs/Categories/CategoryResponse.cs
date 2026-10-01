namespace Library.Application.DTOs.Categories;

public record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    int BookCount
);
