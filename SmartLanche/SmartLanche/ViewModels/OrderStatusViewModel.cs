using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SmartLanche.Data;
using SmartLanche.Helpers;
using SmartLanche.Messages;
using SmartLanche.Models;
using System.Collections.ObjectModel;

using static SmartLanche.Helpers.EnumValuesExtension;

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

        [ObservableProperty]
        private EnumValue? selectedFilterStatus;

        [ObservableProperty]
        private List<EnumValue> filterStatuses = new();

        [RelayCommand]
        public async Task LoadOrdersAsync()
        {
            try
            {
                IsBusy = true;

                using var context = await _contextFactory.CreateDbContextAsync();

                var query = context.Orders
                    .Include(order => order.OrderItems).ThenInclude(orderItem => orderItem.Product)
                    .Include(order => order.Client)
                    .AsNoTracking();

                query = SelectedFilterStatus?.Value switch
                {
                    OrderStatus status => query.Where(o => o.Status == status),
                    FilterOptions.All => query,
                    _ => query.Where(order => order.Status != OrderStatus.Ready &&
                                              order.Status != OrderStatus.Completed &&
                                              order.Status != OrderStatus.Cancelled)
                };

                Orders = new ObservableCollection<Order>(await query.OrderByDescending(o => o.OrderDate).ToListAsync());                
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar: {ex.Message}", false));
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task ChangeStatusAsync(Order order)
        {
            if(order == null) return;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var dbOrder = await context.Orders.FindAsync(order.Id);

                if (dbOrder == null) return;

                dbOrder.Status = order.Status;

                await context.SaveChangesAsync();

                if (!ShouldOrderBeVisible(order))
                {
                    Orders.Remove(order);
                }
                
                string statusPtBr = EnumValuesExtension.GetDisplayName(order.Status);

                Messenger.Send(new StatusMessage($"Pedido #{order.Id} atualizado para {statusPtBr}!", true));
            }
            catch (Exception ex)
            {   
                Messenger.Send(new StatusMessage("Erro ao salvar: " + ex.Message, false));
            }
        }

        private bool ShouldOrderBeVisible(Order order)
        {            
            if (SelectedFilterStatus?.Value is FilterOptions.All)
                return true;
            
            if (SelectedFilterStatus?.Value is OrderStatus filterStatus)
                return order.Status == filterStatus;
            
            var finalizedStatuses = new[] { OrderStatus.Ready, OrderStatus.Completed, OrderStatus.Cancelled };

            return !finalizedStatuses.Contains(order.Status);
        }

        public void InitializeFilters()
        {
            var list = new List<EnumValue>();

            list.Add(new EnumValue { DisplayName = "Todos os Status", Value = FilterOptions.All });

            var enumValues = new EnumValuesExtension(typeof(OrderStatus)).ProvideValue(null!) as List<EnumValue>;

            if (enumValues != null) list.AddRange(enumValues);

            FilterStatuses = list;

            SelectedFilterStatus = null;
        }

        partial void OnSelectedFilterStatusChanged(EnumValue? value) => _ = LoadOrdersAsync();
        
    }
}
