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
    public partial class InventoryViewModel : BaseViewModel
    {
        private readonly IRepository<Product> _repositoryProduct;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public InventoryViewModel(
            IRepository<Product> repository,
            IMessenger messenger,
            IDbContextFactory<AppDbContext> contextFactory) : base(messenger)
        {
            _repositoryProduct = repository;
            _contextFactory = contextFactory;

            FilteredProducts = new ObservableCollection<Product>();
            _ = LoadInventoryAsync();
        }

        #region Propriedades Observáveis

        [ObservableProperty]
        private ObservableCollection<Product> filteredProducts;

        [ObservableProperty]
        private List<Product> allProducts = new();

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private double movementAmount;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLowStockWarningVisible))]
        private bool hasLowStockItems;

        public bool IsLowStockWarningVisible => AllProducts?.Any(product => product.StockQuantity <= 5 && product.IsActive) ?? false;

        #endregion

        #region Comandos

        [RelayCommand]
        private async Task LoadInventoryAsync()
        {
            IsBusy = true;
            try
            {
                var list = await _repositoryProduct.GetAllAsync();
                AllProducts = list.Where(product => product.IsActive).ToList();
                ApplyFilter();
                OnPropertyChanged(nameof(IsLowStockWarningVisible));
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar estoque: {ex.Message}", false));
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ProcessManualEntryAsync() => await ChangeStock(MovementType.Input, "Entrada Manual");

        [RelayCommand]
        private async Task ProcessManualExitAsync() => await ChangeStock(MovementType.Output, "Saída Manual");

        #endregion

        #region Lógica de Negócio

        private async Task ChangeStock(MovementType type, string reason)
        {
            if (SelectedProduct == null || MovementAmount <= 0)
            {
                Messenger.Send(new StatusMessage("Selecione um produto e informe uma quantidade válida.", false));
                return;
            }

            IsBusy = true;
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var product = await context.Products.FindAsync(SelectedProduct.Id);
                if (product == null) return;

                double adjustment = type == MovementType.Input ? MovementAmount : -MovementAmount;
                
                product.StockQuantity += adjustment;
                
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    Type = type,
                    Quantity = MovementAmount,
                    Date = DateTime.Now,
                    Reason = reason
                };

                context.StockMovements.Add(movement);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                Messenger.Send(new StatusMessage($"Estoque de {product.Name} atualizado!", true));

                MovementAmount = 0;
                await LoadInventoryAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Messenger.Send(new StatusMessage($"Erro na movimentação: {ex.Message}", false));
            }
            finally { IsBusy = false; }
        }

        private void ApplyFilter()
        {
            if (AllProducts == null) return;

            var query = AllProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            FilteredProducts = new ObservableCollection<Product>(query);
        }

        partial void OnSearchTextChanged(string? value) => ApplyFilter();

        #endregion
    }
}