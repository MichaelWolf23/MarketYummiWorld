using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketYummiWorld.Models
{
    public class OrderItem { public int ItemId { get; set; } public int OrderId { get; set; } public int ProductId { get; set; } public int Quantity { get; set; } public decimal PriceAtPurchase { get; set; } }

}
