using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.Books
{
    public class SaveBookRequest
    {
        [Required(ErrorMessage = "Tựa đề không được để trống")]
        [StringLength(250, MinimumLength = 1)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(150, MinimumLength = 1)]
        public string Author { get; set; } = null!;

        [Required(ErrorMessage = "ISBN không được để trống")]
        [StringLength(50, MinimumLength = 1)]
        public string ISBN { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Đường dẫn ảnh không được vượt quá 500 ký tự")]
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Năm xuất bản phải lớn hơn 0")]
        public int? PublishYear { get; set; }

        [Required(ErrorMessage = "Thể loại không được để trống")]
        public Guid CategoryId { get; set; }
    }
}