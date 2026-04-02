using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketYummiWorld.Models
{
    public class Customer { public int CustomerId { get; set; } public string FullName { get; set; } = ""; public string Phone { get; set; } = ""; public string? Email { get; set; } public DateTime? RegistrationDate { get; set; } }

}
