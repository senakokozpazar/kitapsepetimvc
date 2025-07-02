using System;
namespace kitapsepetimvc.Models
{
    public class DeliveryStatus
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

