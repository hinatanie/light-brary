using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Domain.Entities
{
    [Table("Categories")]
    public class Category
    {
        [Key] public int Id {get; set;}
        public string Name {get; set;} = string.Empty;
        public string? Description {get; set;}
 
        // Navigation property
        public ICollection<Book> Books {get; set;} = new List<Book>();
    }
}