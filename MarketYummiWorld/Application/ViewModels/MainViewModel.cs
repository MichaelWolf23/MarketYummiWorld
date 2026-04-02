using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketYummiWorld.Application.Services;

namespace MarketYummiWorld.Application.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "Вкусный Мир - Интернет-магазин";
    [ObservableProperty] private ObservableObject _currentViewModel;

    public MainViewModel(OrdersViewModel ordersViewModel)
    {
        CurrentViewModel = ordersViewModel; // По умолчанию открываем заказы
    }
}
