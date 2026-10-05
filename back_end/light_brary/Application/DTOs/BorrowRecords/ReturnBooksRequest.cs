using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class ReturnBooksRequest
    {
        [Required(ErrorMessage = "Danh sách chi tiết không được để trống.")]
        [MinLength(1, ErrorMessage = "Danh sách chi tiết không được để trống.")]
        public List<int> BorrowDetailIds { get; set; } = new List<int>();
    }
}