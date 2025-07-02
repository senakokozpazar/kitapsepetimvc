using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kitapsepetimvc.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
