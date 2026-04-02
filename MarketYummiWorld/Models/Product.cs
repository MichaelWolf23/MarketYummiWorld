using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketYummiWorld.Models
{
    public class Product { public int ProductId { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; } public decimal Price { get; set; } public int? CategoryId { get; set; } public string? Unit { get; set; } public int StockQuantity { get; set; } public DateTime? ProductionDate { get; set; } public DateTime? ExpiryDate { get; set; } }

}
