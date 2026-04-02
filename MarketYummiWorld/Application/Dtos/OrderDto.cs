using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MarketYummiWorld.Application.Extensions;

namespace MarketYummiWorld.Application.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string OrderDate { get; set; } = ""; // Хранится как строка YYYY-MM-DD
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public PaymentStatus PaymentStatus { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; } // Фиксируем цену на момент заказа
}