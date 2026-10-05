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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        [Required]
        [StringLength(250)]
        public string Title {get; set;} = null!;

        [Required]
        [StringLength(150)]
        public string Author {get; set;} = null!;

        [Required]
        [StringLength(50)]
        public string ISBN {get; set;} = null!;

        public int? PublishYear {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        [ForeignKey(nameof(Category))]
        public int CategoryId {get; set;}
 
        public Category Category {get; set;} = null!;
        public ICollection<BookCopy> BookCopies {get; set;} = new List<BookCopy>();
    }
}
