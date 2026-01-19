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
            FilteredProducts = new ObservableCollection<Product>();
        }

        #region Propriedades Observáveis

        [ObservableProperty]
        private ObservableCollection<Product> products = new();        

        [ObservableProperty]
        private ObservableCollection<Product> filteredProducts;

        [ObservableProperty]
        private List<Product> allProducts = new();

        [ObservableProperty]
        private Product? selectedProduct;
        
        [ObservableProperty]
        private int id;

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [ObservableProperty]
        private string name = "";

        [Required(ErrorMessage = "A Categopria é obrigatória.")]
        [ObservableProperty]
        private string category = "";

        [Required(ErrorMessage = "O Preço é obrigatório.")]
        [ObservableProperty]
        private decimal price;

        [Required(ErrorMessage = "A Descrição é obrigatória")]
        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private bool isCombo;

        [ObservableProperty]
        private string? categoryFilter;        

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFormEnabled))]
        [NotifyPropertyChangedFor(nameof(DataGridReadOnly))]
        [NotifyCanExecuteChangedFor(nameof(SaveProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(NewProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelActionCommand))]

        [NotifyCanExecuteChangedFor(nameof(EditProductCommand))]
        private bool isEditing = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFormEnabled))]
        [NotifyPropertyChangedFor(nameof(DataGridReadOnly))]
        [NotifyCanExecuteChangedFor(nameof(EditProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelActionCommand))]
        [NotifyCanExecuteChangedFor(nameof(NewProductCommand))]

        private bool isViewing = false;

        public bool IsFormEnabled => IsEditing;
        public bool DataGridReadOnly => IsEditing;

        #endregion

        #region Comandos
        
        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            try
            {
                var listProducts = await _repositoryProduct.GetAllAsync();
                AllProducts = listProducts.Where(products => products.IsActive).ToList();

                ApplyFilter();

                //Products = new ObservableCollection<Product>(listProducts.Where(product => product.IsActive).ToList());                
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao carregar dados iniciais: {ex.Message}", isSuccess: false));
            }
            
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveProductAsync()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault()?.ErrorMessage;

                Messenger.Send(new StatusMessage(firstError ?? "Verifique os campos obrigatórios.", isSuccess: false));
                return;
            }

            var product = Id == 0 ? new Product() : await _repositoryProduct.GetByIdAsync(Id);
            if (product == null) return;

            product.Name = Name;
            product.Category = Category;
            product.Price = Price;
            product.Description = Description;
            product.IsCombo = IsCombo;

            if (Id == 0) await _repositoryProduct.AddAsync(product);
            else await _repositoryProduct.UpdateAsync(product);

            string successMessage = Id == 0 ? "Produto cadastrado com sucesso!" : "Produto atualizado com sucesso!";
            Messenger.Send(new StatusMessage(successMessage, isSuccess: true));

            await LoadProductsAsync();

            CancelAction();
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            try
            {
                var product = await _repositoryProduct.GetByIdAsync(SelectedProduct.Id);

                if (product != null)
                {
                    product.IsActive = false;
                    await _repositoryProduct.UpdateAsync(product);

                    Messenger.Send(new StatusMessage("Cliente removido da lista com sucesso!", isSuccess: true));
                }

                await LoadProductsAsync();
                CancelAction();
            }
            catch (Exception)
            {
                Messenger.Send(new StatusMessage("Erro ao desativar produto.", isSuccess: false));
            }           
        }

        [RelayCommand(CanExecute = nameof(CanCreateNew))]
        private void NewProduct()
        {
            ClearFields();
            IsEditing = true;
            IsViewing = true;
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private void EditProduct() => IsEditing = true;   

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelAction()
        {
            ClearFields();
            IsEditing = false;
            IsViewing = false;
        }

        private void ApplyFilter()
        {
            IEnumerable<Product> query = AllProducts;

            if (!string.IsNullOrEmpty(CategoryFilter) || CategoryFilter == "Todas")
            {
                query = query.Where(product => product.Category == CategoryFilter);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(product => product.Name != null && 
                                               product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            FilteredProducts = new ObservableCollection<Product>(query.ToList());
        }
        #endregion

        #region Lógica de Apoio (CanExecute)

        private bool CanSave() => IsEditing;
        private bool CanDelete() => IsViewing && !IsEditing;
        private bool CanCreateNew() => !IsEditing && !IsViewing;
        private bool CanEdit() => IsViewing && !IsEditing;
        private bool CanCancel() => IsViewing || IsEditing;

        private void ClearFields()
        {
            Id = 0;
            Name = "";
            Category = "";
            Price = 0;
            Description = "";
            IsCombo = false;
            SelectedProduct = null;

            SearchText = string.Empty;
            CategoryFilter = "Todas";
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            if (value != null)
            {
                Id = value.Id;
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
        }

        partial void OnSearchTextChanged(string? value) => ApplyFilter();
        partial void OnCategoryFilterChanged(string? value) => ApplyFilter();
        #endregion
    }
}
