using System.ComponentModel.DataAnnotations;
using light_brary.Enums;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Entities
{
    public class FineDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FineId { get; set; }

        public Fine Fine { get; set; } = null!;

        public Guid BorrowDetailId { get; set; }

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public BorrowDetail BorrowDetail { get; set; } = null!;

        public ReasonForFine Reason { get; set; }

        [Precision(18, 0)]
        public decimal Amount { get; set; }
    }
}
