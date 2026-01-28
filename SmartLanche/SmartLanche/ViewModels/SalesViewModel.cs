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
    public partial class SalesViewModel : BaseViewModel
    {
        private readonly IRepository<Product> _repositoryProduct;
        private readonly IRepository<Client> _repositoryClient;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SalesViewModel(
            IRepository<Product> repostoryProduct,
            IRepository<Client> repositoryClient,
            IDbContextFactory<AppDbContext> contextFactory,
            IMessenger messenger) : base(messenger)
        {
            _repositoryProduct = repostoryProduct;
            _repositoryClient = repositoryClient;
            _contextFactory = contextFactory;

            Messenger.Register<SalesViewModel, ProductsChangedMessage>(this, async (r, m) =>
            {
                await r.LoadDataAsync();
            });

            Messenger.Register<SalesViewModel, ClientsChangedMessage>(this, async (r, m) =>
            {
                await r.LoadDataAsync();
            });

            _ = LoadDataAsync();
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

        [ObservableProperty]
        private Product? selectedProduct;

        public decimal TotalOrderAmount => CartItems.Sum(item => item.Subtotal);
        public int TotalQuantity => CartItems.Sum(item => item.Quantity);
        public bool CanBeCredit => SelectedPaymentMethod == PaymentMethod.Credit;

        #endregion

        #region Comandos

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var listProducts = (await _repositoryProduct.GetAllAsync()).ToList();
                Products = new ObservableCollection<Product>(listProducts.Where(product => product.IsActive).ToList());

                var listClients = (await _repositoryClient.GetAllAsync()).ToList();
                Clients = new ObservableCollection<Client>(listClients.Where(client => client.IsActive).ToList());
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar dados iniciais: {ex.Message}", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void AddProductToCart(Product? product)
        {
            if (product == null) return;
            
            if (CartItems.Any(item => item.ProductId == product.Id))
                return;
           
                CartItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                });

            UpdateTotals();
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
                Messenger.Send(new StatusMessage("Selecione um cliente para pedidos no Fiado.", false));
                return;
            }
            
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                IsBusy = true;

                var newOrder = new Order
                {
                    TotalAmount = TotalOrderAmount,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    PaymentMethod = SelectedPaymentMethod,
                    ClientId = SelectedClient?.Id,

                    OrderItems = CartItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                context.Orders.Add(newOrder);
                await context.SaveChangesAsync();
                
                if (SelectedPaymentMethod == PaymentMethod.Credit && SelectedClient != null)
                {                    
                    var clientDb = await context.Clients.FindAsync(SelectedClient.Id);
                    if (clientDb != null)
                    {
                        clientDb.OutstandingBalance += TotalOrderAmount;
                        context.Clients.Update(clientDb);
                        await context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                
                Messenger.Send(new OrderCreatedMessage(newOrder));
                
                CartItems.Clear();
                SelectedClient = null;
                SelectedPaymentMethod = PaymentMethod.Cash;
                UpdateTotals();

                Messenger.Send(new StatusMessage($"Pedido Nº{newOrder.Id} finalizado com sucesso!", true));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Messenger.Send(new StatusMessage($"Erro ao gravar pedido: {ex.Message}", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void IncreaseQuantity(OrderItem item)
        {
            item.Quantity++;
            UpdateTotals();
        }

        [RelayCommand]
        private void DecreaseQuantity(OrderItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {                
                CartItems.Remove(item);
            }

            UpdateTotals();
        }        
        
        #endregion

        #region Lógica de Apoio (CanExecute)
        partial void OnSelectedProductChanged(Product? value)
        {
            if (value == null) return;
            
            AddProductToCart(value);

            SelectedProduct = null;          
        }

        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(TotalOrderAmount));
            OnPropertyChanged(nameof(TotalQuantity));
            FinalizeOrderCommand.NotifyCanExecuteChanged();
        }
        #endregion
    }
}
