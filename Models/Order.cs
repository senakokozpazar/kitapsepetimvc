﻿using System;
namespace kitapsepetimvc.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }

        public User User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

}

