using System;
using System.ComponentModel.DataAnnotations;

namespace kitapsepetimvc.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public ICollection<Order> Orders { get; set; }
    }

}

