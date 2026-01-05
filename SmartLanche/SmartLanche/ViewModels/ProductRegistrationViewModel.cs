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
        }

        #region Propriedades Observáveis

        [ObservableProperty]
        private ObservableCollection<Product> products = new();
       
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

                Products = new ObservableCollection<Product>(listProducts.ToList());
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
            if (SelectedProduct != null)
            {
                await _repositoryProduct.DeleteAsync(SelectedProduct.Id);
                await LoadProductsAsync();

                string sucecessMessage = "Produto excluido com sucesso!";
                Messenger.Send(new StatusMessage(sucecessMessage, isSuccess: true));

                CancelAction();
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
        #endregion    
    }
}
