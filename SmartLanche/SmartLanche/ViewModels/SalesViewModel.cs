using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SmartLanche.Data;
using SmartLanche.Helpers;
using SmartLanche.Messages;
using SmartLanche.Models;
using SmartLanche.Services;
using System.Collections.ObjectModel;

namespace SmartLanche.ViewModels
{
    public partial class SalesViewModel : BaseViewModel
    {
        private readonly IRepository<Product> _repositoryProduct;
        private readonly IRepository<Client> _repositoryClient;
        private readonly AppDbContext _dbContext;

        public SalesViewModel(
            IRepository<Product> repostoryProduct,
            IRepository<Client> repositoryClient,
            AppDbContext dbContext,
            IMessenger messenger) : base(messenger)
        {
            _repositoryProduct = repostoryProduct;
            _repositoryClient = repositoryClient;
            _dbContext = dbContext;

            CartItems = new ObservableCollection<OrderItem>();           
        }

        #region Propriedades Observáveis

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<Client> clients = new();

        [ObservableProperty]
        private ObservableCollection<OrderItem> cartItems = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanBeCredit))]
        private Client? selectedClient;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanBeCredit))]
        private PaymentMethod selectedPaymentMethod = PaymentMethod.Cash;        

        public decimal TotalOrderAmount => CartItems.Sum(item => item.Subtotal);
        public bool CanBeCredit => SelectedPaymentMethod == PaymentMethod.Credit;

        #endregion

        #region Comandos

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                var productList = (await _repositoryProduct.GetAllAsync()).ToList();
                Products = new ObservableCollection<Product>(productList);

                var clientList = (await _repositoryClient.GetAllAsync()).ToList();
                Clients = new ObservableCollection<Client>(clientList);
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar dados iniciais: {ex.Message}", isSuccess: false));
            }
        }

        [RelayCommand]
        private void AddProductToCart(Product? product)
        {
            if (product == null) return;

            var existingItem = CartItems.FirstOrDefault(item => item.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                OnPropertyChanged(nameof(TotalOrderAmount));
            }
            else
            {
                CartItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                });
            }

            OnPropertyChanged(nameof(TotalOrderAmount));
            FinalizeOrderCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void RemoveItemFromCart(OrderItem? item)
        {
            if (item == null) return;

            CartItems.Remove(item);

            OnPropertyChanged(nameof(TotalOrderAmount));
            FinalizeOrderCommand.NotifyCanExecuteChanged();
        }

        private bool CanFinalize() => CartItems?.Any() ?? false;

        [RelayCommand(CanExecute = nameof(CanFinalize))]
        private async Task FinalizeOrderAsync()
        {
            if (!CartItems.Any()) return;

            if (SelectedPaymentMethod == PaymentMethod.Credit && SelectedClient == null)
            {
                Messenger.Send(new Messages.StatusMessage("Selecione um cliente para pedidos no Fiado.", isSuccess: false));
                return;
            }

            var totalAmount = TotalOrderAmount;

            var orderItem = CartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();

            var newOrder = new Order
            {
                TotalAmount = TotalOrderAmount,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                PaymentMethod = SelectedPaymentMethod,
                ClientId = SelectedClient?.Id,
                OrderItems = orderItem
            };

            Client? clientToUpdate = null;

            if (SelectedPaymentMethod == PaymentMethod.Credit && SelectedClient != null)
            {
                clientToUpdate = SelectedClient;
                clientToUpdate.OutstandingBalance += totalAmount;
            }

            using (var transiction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    _dbContext.Orders.Add(newOrder);

                    await _dbContext.SaveChangesAsync();

                    if (clientToUpdate != null)
                    {
                        _dbContext.Clients.Update(clientToUpdate);

                        await _dbContext.SaveChangesAsync();
                    }

                    await transiction.CommitAsync();

                    CartItems.Clear();
                    SelectedClient = null;
                    SelectedPaymentMethod = PaymentMethod.Cash;

                    OnPropertyChanged(nameof(TotalOrderAmount));
                    OnPropertyChanged(nameof(CanBeCredit));
                    FinalizeOrderCommand.NotifyCanExecuteChanged();

                    Messenger.Send(new Messages.StatusMessage($"Pedido Nº{newOrder.Id} finalizado com sucesso! Total: {newOrder.TotalAmount:C}", isSuccess: true));
                }
                catch (Exception ex)
                {
                    await transiction.RollbackAsync();

                    if (clientToUpdate != null)
                    {
                        clientToUpdate.OutstandingBalance -= totalAmount;
                    }

                    Messenger.Send(new StatusMessage($"Falha crítica ao finalizar pedido. Transação revertida. Erro: {ex.Message}", isSuccess: false));
                }
            }
        }
        #endregion
    }
}
