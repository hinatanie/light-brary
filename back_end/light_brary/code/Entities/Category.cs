using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Entities
{
    [Table("Categories")]
    [Index(nameof(Name), IsUnique = true)]
    public class Category
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        [Required]
        [StringLength(150)]
        public string Name {get; set;} = null!;

        [StringLength(255)]
        public string? Description {get; set;}
 
        [InverseProperty(nameof(Book.Category))]
        public ICollection<Book> Books {get; set;} = new List<Book>();
    }
}
