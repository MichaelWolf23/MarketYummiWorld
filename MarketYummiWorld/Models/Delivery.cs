using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketYummiWorld.Models
{
    public class Delivery { public int DeliveryId { get; set; } public int OrderId { get; set; } public int? CourierId { get; set; } public int? AddressId { get; set; } public DateTime? PlannedTime { get; set; } public DateTime? ActualTime { get; set; } public string DeliveryStatus { get; set; } = "ожидает"; }

}
