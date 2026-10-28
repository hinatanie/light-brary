using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Entities
{
    [Index(nameof(ISBN), IsUnique = true)]
    public class Book
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Title {get; set;} = null!;

        [Required]
        [StringLength(100)]
        public string Author {get; set;} = null!;

        [Required]
        [StringLength(50)]
        public string ISBN {get; set;} = null!;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int? PublishYear {get; set;}

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        public Guid CategoryId {get; set;}

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Category Category {get; set;} = null!;

        public ICollection<BookCopy> BookCopies {get; set;} = new List<BookCopy>();
    }
}
