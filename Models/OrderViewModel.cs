using System;
using System.Collections.Generic;

namespace kitapsepetimvc.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        // DeliveryStatus bilgileri
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveryLastUpdated { get; set; }

        // PaymentDetail bilgileri
        public decimal? PaymentAmount { get; set; }
        public string PaymentMethod { get; set; }
        public bool? IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
