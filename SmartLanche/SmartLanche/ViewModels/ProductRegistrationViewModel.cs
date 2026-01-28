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

            FilteredProducts = new ObservableCollection<Product>();

            CategoryFilter = "Todas";
        }

        #region Propriedades Observáveis              

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

        [NotifyCanExecuteChangedFor(nameof(ClearFiltersCommand))]

        private bool isViewing = false;

        public bool IsFormEnabled => IsEditing;
        public bool DataGridReadOnly => IsEditing;

        #endregion

        #region Comandos
        
        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            IsBusy = true;

            try
            {
                var listProducts = await _repositoryProduct.GetAllAsync();
                AllProducts = listProducts.Where(products => products.IsActive).ToList();

                ApplyFilter();
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

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveProductAsync()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault()?.ErrorMessage;

                Messenger.Send(new StatusMessage(firstError ?? "Verifique os campos obrigatórios.", false));
                return;
            }

            IsBusy = true;

            try
            {
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
                Messenger.Send(new StatusMessage(successMessage, true));
                Messenger.Send(new ProductsChangedMessage());

                await LoadProductsAsync();

                CancelAction();
            }
            catch (Exception ex)
            {
                Messenger.Send(new StatusMessage($"Erro ao salvar: {ex.Message}", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            IsBusy = true;

            try
            {
                var product = await _repositoryProduct.GetByIdAsync(SelectedProduct.Id);

                if (product != null)
                {
                    product.IsActive = false;
                    await _repositoryProduct.UpdateAsync(product);

                    Messenger.Send(new StatusMessage("Item removido da lista com sucesso!", true));
                }

                await LoadProductsAsync();
                CancelAction();
            }
            catch (Exception)
            {
                Messenger.Send(new StatusMessage("Erro ao desativar produto.", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCreateNew))]
        private void NewProduct()
        {
            ClearFields();
            IsViewing = true;
            IsEditing = true;            
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

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            CategoryFilter = "Todas";

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (AllProducts == null) return;

            IEnumerable<Product> query = AllProducts;

            if (!string.IsNullOrWhiteSpace(CategoryFilter) && CategoryFilter != "Todas")
            {
                query = query.Where(product => product.Category == CategoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(product => product.Name != null &&
                                               product.Name.Contains(SearchText!, StringComparison.OrdinalIgnoreCase));
            }

            FilteredProducts = new ObservableCollection<Product>(query.ToList());
        }

        [RelayCommand]
        public void ResetScreenState()
        {
            IsBusy = false;
            ClearFields();
            SearchText = string.Empty;
            IsEditing = false;
            IsViewing = false;
            ApplyFilter();
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
            if (IsEditing) return;

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
                if (!IsViewing && !IsEditing) CancelAction();
            }
        }

        partial void OnSearchTextChanged(string? value) => ApplyFilter();
        partial void OnCategoryFilterChanged(string? value) => ApplyFilter();
        #endregion
    }
}
