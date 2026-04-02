using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketYummiWorld.Application.Dtos;
using MarketYummiWorld.Application.Extensions;

namespace MarketYummiWorld.Application.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken ct = default);
    Task UpdateStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct = default);
}

public class OrderCreateRequest
{
    public int CustomerId { get; set; }
    public string PaymentMethod { get; set; } = "cash"; // cash / card
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}