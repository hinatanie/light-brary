using System.ComponentModel.DataAnnotations;

namespace light_brary.DTOs.Requests
{
    public record ReturnBooksRequest
    {
        [Required(ErrorMessage = "Danh sách chi tiết không được để trống.")]
        [MinLength(1, ErrorMessage = "Danh sách chi tiết không được để trống.")]
        public List<Guid> BorrowDetailIds { get; set; } = new List<Guid>();
    }
}