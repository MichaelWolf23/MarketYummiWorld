using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketYummiWorld.Models
{
    public class Order { public int OrderId { get; set; } public int? CustomerId { get; set; } public DateTime? OrderDate { get; set; } public decimal TotalAmount { get; set; } public string? PaymentMethod { get; set; } public string PaymentStatus { get; set; } = "не оплачен"; public string OrderStatus { get; set; } = "новый"; }

}
