using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.BookCopies;

public record UpdateBookCopyConditionRequest(
    [MaxLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
    string? ConditionNote
);
