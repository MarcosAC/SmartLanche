using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SmartLanche.Messages;
using SmartLanche.Models;
using SmartLanche.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace SmartLanche.ViewModels
{
    public partial class ProductRegistrationViewModel : BaseViewModel
    {
        private readonly IRepository<Product> _repositoryProduct;        

        public ProductRegistrationViewModel(IRepository<Product> repository, IMessenger messenger) : base(messenger)
        {
            _repositoryProduct = repository;            

            Products = new ObservableCollection<Product>();

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, () => IsEditing);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, () => IsViewing && !IsEditing);
            NewProductCommand = new RelayCommand(NewProduct, () => !IsEditing && !IsViewing);
            CancelCommand = new RelayCommand(CancelAction, () => IsViewing || IsEditing);
            EditProductCommand = new RelayCommand(EditProduct, () => IsViewing);

            _ = LoadProductsAsync();
        }
        
        [ObservableProperty]
        private ObservableCollection<Product> products;
       
        [ObservableProperty]
        private Product? selectedProduct;
        
        [ObservableProperty] private int id;

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [ObservableProperty] private string name = "";

        [Required(ErrorMessage = "A Categopria é obrigatória.")]
        [ObservableProperty] private string category = "";

        [Required(ErrorMessage = "O Preço é obrigatório.")]
        [ObservableProperty] private decimal price;

        [Required(ErrorMessage = "A Descrição é obrigatória")]
        [ObservableProperty] private string? description;

        [ObservableProperty] private bool isCombo;

        [ObservableProperty] private bool isEditing = false;
        [ObservableProperty] private bool isViewing = false;

        public bool IsFormEnabled => IsEditing;
        public bool DataGridReadOnly => IsEditing;

        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand SaveProductCommand { get; }
        public IAsyncRelayCommand DeleteProductCommand { get; }
        public IRelayCommand NewProductCommand { get; }
        public IRelayCommand EditProductCommand { get; }

        private async Task LoadProductsAsync()
        {
            Products.Clear();

            var listProducts = await _repositoryProduct.GetAllAsync();

            foreach (var product in listProducts)
                Products.Add(product);
        }
        
        partial void OnSelectedProductChanged(Product? value)
        {
            if (value != null)
            {
                id = value.Id;
                Name = value.Name;
                Category = value.Category;
                Price = value.Price;
                Description = value.Description;
                IsCombo = value.IsCombo;

                IsEditing = false;
                IsViewing = true;
            }
            else
            {
                CancelAction();
            }

            NewProductCommand.NotifyCanExecuteChanged();
            SaveProductCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            EditProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
            OnPropertyChanged(nameof(IsFormEnabled));
        }

        partial void OnIsEditingChanged(bool value)
        {
            SaveProductCommand.NotifyCanExecuteChanged();
            NewProductCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
            OnPropertyChanged(nameof(IsFormEnabled));
        }

        private async Task SaveProductAsync()
        {
           ValidateAllProperties();

            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault()?.ErrorMessage;

                Messenger.Send(new StatusMessage(firstError ?? "Verifique os campos obrigatórios.", isSuccess: false));
                return;
            }

            if (Id == 0)
            {
                var product = new Product
                {
                    Name = Name,
                    Category = Category,
                    Price = Price,
                    Description = Description,
                    IsCombo = IsCombo
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
                product.IsCombo = IsCombo;

                await _repositoryProduct.UpdateAsync(product);
            }

            await LoadProductsAsync();

            string successMessage = Id == 0 ? "Produto cadastrado com sucesso!" : "Produto atualizado com sucesso!";
            Messenger.Send(new StatusMessage(successMessage, isSuccess: true));

            CancelAction();          
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
                return;

            await _repositoryProduct.DeleteAsync(SelectedProduct.Id);
            await LoadProductsAsync();
            CancelAction();          
        }

        private void NewProduct()
        {
            Id = 0;
            Name = "";
            Category = "";
            Price = 0;
            Description = "";
            IsCombo = false;

            SelectedProduct = null;
            IsEditing = true;
            IsViewing = true;

            NewProductCommand.NotifyCanExecuteChanged();
            SaveProductCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();          

            OnPropertyChanged(nameof(IsFormEnabled));
            OnPropertyChanged(nameof(DataGridReadOnly));
        }

        private void EditProduct()
        {
            IsEditing = true;

            SaveProductCommand.NotifyCanExecuteChanged();
            NewProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(IsFormEnabled));
            OnPropertyChanged(nameof(DataGridReadOnly));
        }

        private void CancelAction()
        {
            Id = 0;
            Name = "";
            Category = "";
            Price = 0;
            Description = "";
            IsCombo = false;

            SelectedProduct = null;
            IsEditing = false;
            IsViewing = false;

            NewProductCommand.NotifyCanExecuteChanged();
            SaveProductCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            EditProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
            OnPropertyChanged(nameof(IsFormEnabled));
        }
    }
}
