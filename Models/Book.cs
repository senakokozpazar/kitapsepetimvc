using System;
using System.ComponentModel.DataAnnotations;

namespace kitapsepetimvc.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Author { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        public string ImageUrl { get; set; }

        public Category Category { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

}

