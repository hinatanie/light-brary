using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Domain.Entities
{
    [Table("Books")]
    [Index(nameof(ISBN), IsUnique = true)]
    public class Book
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        [Required]
        [StringLength(250)]
        public string Title {get; set;} = null!;

        [Required]
        [StringLength(150)]
        public string Author {get; set;} = null!;

        [Required]
        [StringLength(50)]
        public string ISBN {get; set;} = null!;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int? PublishYear {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        public Guid CategoryId {get; set;}
 
        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(Category.Books))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Category Category {get; set;} = null!;

        [InverseProperty(nameof(BookCopy.Book))]
        public ICollection<BookCopy> BookCopies {get; set;} = new List<BookCopy>();
    }
}
