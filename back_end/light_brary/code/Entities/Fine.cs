using System.ComponentModel.DataAnnotations;

namespace light_brary.Entities
{
    public class Fine
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid BorrowRecordId { get; set; }

        public BorrowRecord BorrowRecord { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public ICollection<FineDetail> Details { get; set; } = new List<FineDetail>();
    }
}
