using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

        public PendingPaymentsViewModel(
            IRepository<Client> repositoryClient,
            IRepository<Order> repositoryOrder,
            IMessenger messenger) : base(messenger)
        {
            _repositoryClient = repositoryClient;
            _repositoryOrder = repositoryOrder;

            DebtorClients = new ObservableCollection<Client>();
            ClientOrders = new ObservableCollection<Order>();
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
        private decimal paymentAmount;

        [ObservableProperty]
        private bool isLoading;

        #endregion

        #region Comandos

        [RelayCommand]
        private async Task LoadPendingDataAsync()
        {
            IsLoading = true;

            try
            {
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
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanReceivePayment))]
        private async Task ReceivePaymentAsync()
        {
            if (SelectedClient == null || PaymentAmount <= 0) return;

            if (PaymentAmount > SelectedClient.OutstandingBalance)
            {
                Messenger.Send(new StatusMessage("O valor do pagamento não pode ser maior que a dívida.", false));
                return;
            }

            try
            {
                IsLoading = true;

                SelectedClient.OutstandingBalance -= PaymentAmount;
                await _repositoryClient.UpdateAsync(SelectedClient);

                if (SelectedClient.OutstandingBalance == 0)
                {
                    var allOrders = await _repositoryOrder.GetAllAsync();

                    var pendingOrders = allOrders
                        .Where(order => order.ClientId == SelectedClient.Id &&
                                        order.PaymentMethod == PaymentMethod.Credit &&
                                        !order.IsPaid)
                        .ToList();

                    foreach (var order in pendingOrders)
                    {
                        order.IsPaid = true;
                        await _repositoryOrder.UpdateAsync(order);
                    }
                }

                Messenger.Send(new StatusMessage($"Pagamento de {PaymentAmount:C} recebido com sucesso!", false));

                var clientId = SelectedClient.Id;
                PaymentAmount = 0;

                await LoadPendingDataAsync();
                await LoadClientOrderHistoryAsync(clientId);
            }
            catch (Exception)
            {
                Messenger.Send(new StatusMessage($"Erro ao processar pagamento.", false));
            }
            finally 
            { 
                IsLoading = false;
            }
        }

        #endregion

        #region Lógica de Apoio

        private bool CanReceivePayment() => SelectedClient != null && SelectedClient.OutstandingBalance > 0;

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
            var orders = await _repositoryOrder.GetAllAsync();

            var history = orders
                .Where(order => order.ClientId == clientId && order.PaymentMethod == PaymentMethod.Credit)
                .OrderByDescending(order => order.OrderDate)
                .ToList();

            ClientOrders = new ObservableCollection<Order>(history);
        }

        #endregion
    }
}
