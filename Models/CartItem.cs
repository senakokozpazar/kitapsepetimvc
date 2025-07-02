using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kitapsepetimvc.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        // Sepetteki kitap
        [Required]
        public int BookId { get; set; }

        public Book Book { get; set; }

        public int Quantity { get; set; }
    }
}
