using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketYummiWorld.Application.Services;
using MarketYummiWorld.Application.ViewModels;

namespace MarketYummiWorld.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Сервисы бизнес-логики
        services.AddScoped<IOrderService, OrderService>();
        // services.AddScoped<IDeliveryService, DeliveryService>(); // добавить когда будет готово

        // ViewModel'и (создаются при открытии вкладок)
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<CustomersViewModel>(); // заглушка, создай по аналогии с OrdersViewModel
        services.AddTransient<ProductsViewModel>();  // заглушка

        return services;
    }
}