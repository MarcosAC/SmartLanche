using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SmartLanche.Data;
using SmartLanche.Helpers;
using SmartLanche.Messages;
using SmartLanche.Models;
using SmartLanche.Services;
using System.Collections.ObjectModel;

namespace SmartLanche.ViewModels
{
    public partial class PendingPaymentsViewModel : BaseViewModel
    {
        private readonly IRepository<Client> _repositoryClient;
        private readonly IRepository<Order> _repositoryOrder;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PendingPaymentsViewModel(
            IRepository<Client> repositoryClient,
            IRepository<Order> repositoryOrder,
            IDbContextFactory<AppDbContext> contextFactory,
            IMessenger messenger) : base(messenger)
        {
            _repositoryClient = repositoryClient;
            _repositoryOrder = repositoryOrder;
            _contextFactory = contextFactory;

            DebtorClients = new ObservableCollection<Client>();
            ClientOrders = new ObservableCollection<Order>();

            Messenger.Register<OrderCreatedMessage>(this, (r, m) => _ = LoadPendingDataAsync());
            Messenger.Register<ClientsChangedMessage>(this, (r, m) => _ = LoadPendingDataAsync());

            _ = LoadPendingDataAsync();
        }

        #region Propriedades Observáveis

        [ObservableProperty]
        private ObservableCollection<Client> debtorClients;

        [ObservableProperty]
        private ObservableCollection<Order> clientOrders;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReceivePaymentCommand))]
        private Client? selectedClient;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReceivePaymentCommand))]
        private decimal paymentAmount;

        [ObservableProperty]
        private bool isLoading;

        #endregion

        #region Comandos

        [RelayCommand]
        private async Task LoadPendingDataAsync()
        {
            try
            {
                IsBusy = true;

                var allClients = await _repositoryClient.GetAllAsync();

                var debtors = allClients
                    .Where(client => client.IsActive && client.OutstandingBalance > 0)
                    .OrderByDescending(client => client.OutstandingBalance)
                    .ToList();

                DebtorClients = new ObservableCollection<Client>(debtors);
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar pendências: {ex.Message}", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanReceivePayment))]
        private async Task ReceivePaymentAsync()
        {
            if (SelectedClient == null || PaymentAmount <= 0) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                IsBusy = true;
                                
                var clientDb = await context.Clients.FindAsync(SelectedClient.Id);

                if (clientDb == null) return;

                if (PaymentAmount > clientDb.OutstandingBalance)
                {
                    Messenger.Send(new StatusMessage("Valor maior que a dívida.", false));
                    return;
                }

                clientDb.OutstandingBalance -= PaymentAmount;

                if (clientDb.OutstandingBalance == 0)
                {
                    var pendingOrders = await context.Orders
                        .Where(order => order.ClientId == clientDb.Id &&
                                    order.PaymentMethod == PaymentMethod.Credit &&
                                    !order.IsPaid)
                        .ToListAsync();

                    foreach (var order in pendingOrders)
                    {
                        order.IsPaid = true;
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                Messenger.Send(new StatusMessage($"Pagamento de {PaymentAmount:C} recebido!", true));
                Messenger.Send(new ClientsChangedMessage());

                var clientId = clientDb.Id;
                PaymentAmount = 0;

                await LoadPendingDataAsync();

                if (clientDb.OutstandingBalance > 0)
                    await LoadClientOrderHistoryAsync(clientId);
                else
                    SelectedClient = null;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                Messenger.Send(new StatusMessage("Erro ao processar pagamento.", false));
            }
            finally 
            { 
                IsBusy = false;
            }
        }

        #endregion

        #region Lógica de Apoio

        private bool CanReceivePayment() => 
            SelectedClient != null && 
            SelectedClient.OutstandingBalance > 0 && 
            PaymentAmount > 0;

        partial void OnSelectedClientChanged(Client? value)
        {
            ReceivePaymentCommand.NotifyCanExecuteChanged();

            if (value != null)
            {
                _ = LoadClientOrderHistoryAsync(value.Id);
            }
            else 
            {
                ClientOrders.Clear();
            }
        }

        private async Task LoadClientOrderHistoryAsync(int clientId)
        {           
            using var context = await _contextFactory.CreateDbContextAsync();          

            var history = await context.Orders
                .Where(o => o.ClientId == clientId && o.PaymentMethod == PaymentMethod.Credit)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ClientOrders = new ObservableCollection<Order>(history);
        }

        #endregion
    }
}
