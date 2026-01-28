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
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .Include(o => o.Client)
                    .AsNoTracking();

                query = SelectedFilterStatus?.Value switch
                {
                    OrderStatus status => query.Where(o => o.Status == status),
                    FilterOptions.All => query,

                    _ => query.Where(o => o.Status != OrderStatus.Ready &&
                                          o.Status != OrderStatus.Completed &&
                                          o.Status != OrderStatus.Cancelled)
                };

                Orders = new ObservableCollection<Order>(
                    await query.OrderByDescending(o => o.OrderDate).ToListAsync()
                );                
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

                if (dbOrder != null)
                {
                    dbOrder.Status = order.Status;

                    await context.SaveChangesAsync();

                    string statusPtBr = EnumValuesExtension.GetDisplayName(order.Status);

                    if (SelectedFilterStatus?.Value != null)
                    {
                        var filtroAtivo = (OrderStatus)SelectedFilterStatus.Value;
                        if (order.Status != filtroAtivo)
                        {
                            Orders.Remove(order);
                        }
                    }
                    else
                    {                        
                        if (order.Status == OrderStatus.Ready ||
                            order.Status == OrderStatus.Completed ||
                            order.Status == OrderStatus.Cancelled)
                        {                           
                            await LoadOrdersAsync();
                        }
                    }

                    Messenger.Send(new StatusMessage($"Pedido #{order.Id} atualizado para {statusPtBr}!", true));
                }
            }
            catch (Exception ex)
            {   
                Messenger.Send(new StatusMessage("Erro ao salvar: " + ex.Message, false));
            }
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
