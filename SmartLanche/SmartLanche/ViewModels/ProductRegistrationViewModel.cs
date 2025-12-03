using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartLanche.Models;
using SmartLanche.Services;
using System.Collections.ObjectModel;

namespace SmartLanche.ViewModels
{
    public partial class ProductRegistrationViewModel : BaseViewModel
    {
        private readonly IRepository<Product> _repositoryProduct;

        public ProductRegistrationViewModel(IRepository<Product> repository)
        {
            _repositoryProduct = repository;

            Products = new ObservableCollection<Product>();

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, () => IsEditing);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, () => !IsEditing && SelectedProduct != null);
            NewProductCommand = new RelayCommand(NewProduct, () => !IsEditing);
            CancelCommand = new RelayCommand(CancelEditing, () => IsEditing);

            _ = LoadProductsAsync();
        }
        
        [ObservableProperty]
        private ObservableCollection<Product> products;
       
        [ObservableProperty]
        private Product? selectedProduct;
        
        [ObservableProperty] private int id;
        [ObservableProperty] private string name = "";
        [ObservableProperty] private string category = "";
        [ObservableProperty] private decimal price;
        [ObservableProperty] private string? description;

        [ObservableProperty] private bool isEditing = false;

        public bool DataGridReadOnly => !IsEditing;

        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand SaveProductCommand { get; }
        public IAsyncRelayCommand DeleteProductCommand { get; }
        public IRelayCommand NewProductCommand { get; }

        private async Task LoadProductsAsync()
        {
            Products.Clear();

            var listProducts = await _repositoryProduct.GetAllAsync();

            foreach (var product in listProducts)
                Products.Add(product);
        }
        
        partial void OnSelectedProductChanged(Product? value)
        {
            if (value == null) return;

            id = value.Id;
            Name = value.Name;
            Category = value.Category;
            Price = value.Price;
            Description = value.Description;
        }

        partial void OnIsEditingChanged(bool value)
        {
            SaveProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();
            NewProductCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
        }
   
        private void CancelEditing()
        {            
            NewProduct();
            IsEditing = false;
        }

        private async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;

            if (Id == 0)
            {
                var product = new Product
                {
                    Name = Name,
                    Category = Category,
                    Price = Price,
                    Description = Description
                };

                await _repositoryProduct.AddAsync(product);
            }
            else
            {
                var product = await _repositoryProduct.GetByIdAsync(Id);
                if (product == null) return;

                product.Name = Name;
                product.Category = Category;
                product.Price = Price;
                product.Description = Description;

                await _repositoryProduct.UpdateAsync(product);
            }

            await LoadProductsAsync();
            NewProduct();
            IsEditing = false;
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
                return;

            await _repositoryProduct.DeleteAsync(SelectedProduct.Id);
            await LoadProductsAsync();
            NewProduct();
        }

        private void NewProduct()
        {
            Id = 0;
            Name = "";
            Category = "";
            Price = 0;
            Description = "";
            //IsCombo = false;

            SelectedProduct = null;
            IsEditing = true;
        }
    }
}
