using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketYummiWorld.Application.Dtos;
using MarketYummiWorld.Application.Extensions;

namespace MarketYummiWorld.Application.Services;

// ⚠️ Эти интерфейсы предоставит Разработчик 1. Пока просто создай пустые файлы с такими же именами.
public interface IOrderRepository
{
    Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken ct);
    Task<OrderDto> GetByIdAsync(int id, CancellationToken ct);
    Task CreateAsync(OrderCreateRequest request, OrderDto created, CancellationToken ct);
    Task UpdateStatusAsync(int id, OrderStatus status, CancellationToken ct);
}

public interface IProductRepository
{
    Task<decimal> GetPriceAsync(int productId, CancellationToken ct);
    Task<int> GetStockAsync(int productId, CancellationToken ct);
    Task ReduceStockAsync(int productId, int quantity, CancellationToken ct);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken ct = default) =>
        await _orderRepo.GetAllAsync(ct);

    public async Task<OrderDto> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _orderRepo.GetByIdAsync(id, ct);

    public async Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("Заказ не может быть пустым.");

        decimal total = 0;
        foreach (var item in request.Items)
        {
            var price = await _productRepo.GetPriceAsync(item.ProductId, ct);
            var stock = await _productRepo.GetStockAsync(item.ProductId, ct);

            if (stock < item.Quantity)
                throw new InvalidOperationException($"Недостаточно товара #{item.ProductId} на складе.");

            total += price * item.Quantity;
        }

        var order = new OrderDto
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.Now.ToDbDate(),
            Status = OrderStatus.New,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = request.PaymentMethod == "card" ? PaymentStatus.Paid : PaymentStatus.Unpaid,
            TotalAmount = total
        };

        // Транзакция: сохранение заказа + списание остатков
        await _orderRepo.CreateAsync(request, order, ct);

        foreach (var item in request.Items)
            await _productRepo.ReduceStockAsync(item.ProductId, item.Quantity, ct);

        return order;
    }

    public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct = default)
    {
        var current = await _orderRepo.GetByIdAsync(orderId, ct);
        if (!IsValidTransition(current.Status, newStatus))
            throw new InvalidOperationException($"Недопустимый переход: {current.Status} → {newStatus}");

        await _orderRepo.UpdateStatusAsync(orderId, newStatus, ct);
    }

    private bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        var matrix = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.New] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
            [OrderStatus.Confirmed] = new[] { OrderStatus.Assembling },
            [OrderStatus.Assembling] = new[] { OrderStatus.OutForDelivery },
            [OrderStatus.OutForDelivery] = new[] { OrderStatus.Delivered },
            [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
        };
        return matrix.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}
