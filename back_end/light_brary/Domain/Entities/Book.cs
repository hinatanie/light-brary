using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Domain.Entities
{
    [Table("Books")]
    public class Book
    {
        [Key] public int Id {get; set;}
        public string Title {get; set;} = string.Empty;
        public string Author {get; set;} = string.Empty;
        public string ISBN {get; set;} = string.Empty;
        public int? PublishYear {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
        public int CategoryId {get; set;}
 
        // Navigation property
        public Category? Category {get; set;}
        public ICollection<BookCopy> BookCopies {get; set;} = new List<BookCopy>();
    }
}