using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartLanche.Models;
using SmartLanche.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace SmartLanche.ViewModels
{
    public partial class ProductRegistrationViewModel : ObservableObject
    {
        private readonly IRepository<Product> _repository;

        public ProductRegistrationViewModel(IRepository<Product> repository)
        {
            _repository = repository;

            Products = new ObservableCollection<Product>();

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync);
            NewProductCommand = new RelayCommand(NewProduct);

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
        
        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand SaveProductCommand { get; }
        public IAsyncRelayCommand DeleteProductCommand { get; }
        public IRelayCommand NewProductCommand { get; }

        private async Task LoadProductsAsync()
        {
            Products.Clear();

            var lista = await _repository.GetAllAsync();

            foreach (var p in lista)
                Products.Add(p);
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

                await _repository.AddAsync(product);
            }
            else
            {
                var product = await _repository.GetByIdAsync(Id);
                if (product == null) return;

                product.Name = Name;
                product.Category = Category;
                product.Price = Price;
                product.Description = Description;

                await _repository.UpdateAsync(product);
            }

            await LoadProductsAsync();
            NewProduct();
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
                return;

            await _repository.DeleteAsync(SelectedProduct.Id);
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
            SelectedProduct = null;
        }
    }
}
