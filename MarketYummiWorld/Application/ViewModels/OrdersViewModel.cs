using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarketYummiWorld.Application.Dtos;
using MarketYummiWorld.Application.Extensions;
using MarketYummiWorld.Application.Services;

namespace MarketYummiWorld.Application.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly IOrderService _orderService;

    public OrdersViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Orders = new ObservableCollection<OrderDto>();
        LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
        ConfirmOrderCommand = new AsyncRelayCommand<OrderDto>(async o => await ChangeStatusAsync(o, OrderStatus.Confirmed), o => o?.Status == OrderStatus.New);
        CancelOrderCommand = new AsyncRelayCommand<OrderDto>(async o => await ChangeStatusAsync(o, OrderStatus.Cancelled), o => o?.Status == OrderStatus.New || o?.Status == OrderStatus.Confirmed);
    }

    [ObservableProperty] private ObservableCollection<OrderDto> _orders;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private OrderStatus _filterStatus = OrderStatus.New;

    public IAsyncRelayCommand LoadOrdersCommand { get; }
    public IAsyncRelayCommand<OrderDto> ConfirmOrderCommand { get; }
    public IAsyncRelayCommand<OrderDto> CancelOrderCommand { get; }

    partial void OnFilterStatusChanged(OrderStatus value) => LoadOrdersCommand.ExecuteAsync(null);

    private async Task LoadOrdersAsync()
    {
        IsLoading = true;
        ErrorMessage = "";
        try
        {
            var all = await _orderService.GetAllAsync();
            var filtered = all.Where(o => o.Status == FilterStatus).ToList();

            Orders.Clear();
            foreach (var o in filtered) Orders.Add(o);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ChangeStatusAsync(OrderDto order, OrderStatus newStatus)
    {
        try
        {
            await _orderService.UpdateStatusAsync(order.Id, newStatus);
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    partial void OnOrdersChanged(ObservableCollection<OrderDto> value)
    {
        ConfirmOrderCommand.NotifyCanExecuteChanged();
        CancelOrderCommand.NotifyCanExecuteChanged();
    }
}