using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        [Required]
        [StringLength(20)]
        public string Name {get; set;} = null!;

        [StringLength(255)]
        public string? Description {get; set;}
 
        public ICollection<Book> Books {get; set;} = new List<Book>();
    }
}
