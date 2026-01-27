using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SmartLanche.Data;
using SmartLanche.Messages;
using SmartLanche.Models;
using System.Collections.ObjectModel;

namespace SmartLanche.ViewModels
{
    public partial class OrderStatusViewModel : BaseViewModel
    {       
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public OrderStatusViewModel(IDbContextFactory<AppDbContext> contextFactory, IMessenger messenger) : base(messenger)
        {            
            _contextFactory = contextFactory;

            Messenger.Register<OrderCreatedMessage>(this, async (r, m) => await LoadOrdersAsync());
        }

        [ObservableProperty]
        private ObservableCollection<Order> orders = new();

        [RelayCommand]
        public async Task LoadOrdersAsync()
        {
            try
            {
                IsBusy = true;

                using var context = await _contextFactory.CreateDbContextAsync();

                var list = await context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .Include(o => o.Client)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                Orders = new ObservableCollection<Order>(list);
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar pedidos: {ex.Message}", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ChangeStatusAsync(Order order)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var dbOrder = await context.Orders.FindAsync(order.Id);
                if (dbOrder != null)
                {
                    dbOrder.Status = order.Status;

                    await context.SaveChangesAsync();

                    Messenger.Send(new StatusMessage($"Pedido #{order.Id} atualizado para {order.status}!", true));
                }
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage("Erro ao salvar: " + ex.Message, false));
            }
        }
    }
}
