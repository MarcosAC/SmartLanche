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
            try
            {
                var listProducts = (await _repositoryProduct.GetAllAsync()).ToList();
                Products = new ObservableCollection<Product>(listProducts.Where(product => product.IsActive).ToList());

                var listClients = (await _repositoryClient.GetAllAsync()).ToList();
                Clients = new ObservableCollection<Client>(listClients.Where(client => client.IsActive).ToList());
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
                Messenger.Send(new StatusMessage("Selecione um cliente para pedidos no Fiado.", isSuccess: false));
                return;
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {                
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
               
                _dbContext.Orders.Add(newOrder);
                await _dbContext.SaveChangesAsync();
                
                if (SelectedPaymentMethod == PaymentMethod.Credit && SelectedClient != null)
                {
                    SelectedClient.OutstandingBalance += TotalOrderAmount;
                    _dbContext.Clients.Update(SelectedClient);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
               
                CartItems.Clear();
                SelectedClient = null;
                SelectedPaymentMethod = PaymentMethod.Cash;
                
                OnPropertyChanged(nameof(TotalOrderAmount));
                OnPropertyChanged(nameof(TotalQuantity));
                FinalizeOrderCommand.NotifyCanExecuteChanged();

                Messenger.Send(new StatusMessage($"Pedido Nº{newOrder.Id} finalizado com sucesso!", isSuccess: true));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Messenger.Send(new StatusMessage($"Erro ao gravar pedido: {ex.Message}", isSuccess: false));
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
