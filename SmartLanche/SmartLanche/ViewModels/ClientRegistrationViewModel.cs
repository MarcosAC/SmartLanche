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
    public partial class ClientRegistrationViewModel : BaseViewModel
    {
        private readonly IRepository<Client> _repositoryClient;

        public ClientRegistrationViewModel(IRepository<Client> repository, IMessenger messenger) : base(messenger)
        {
            _repositoryClient = repository;
            
            FilteredClients = new ObservableCollection<Client>();
        }

        #region Propriedades Observáveis

        [ObservableProperty]
        private ObservableCollection<Client> filteredClients;

        [ObservableProperty]
        private List<Client> allClients = new();

        [ObservableProperty]
        private Client? selectedClient;

        [ObservableProperty]
        private int id;

        [Required(ErrorMessage = "O nome é obrigatório.")]        
        [ObservableProperty]
        private string name = "";

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [ObservableProperty]
        private string? phone;

        [ObservableProperty] 
        private string? observations;

        [ObservableProperty] 
        private decimal outstandingBalance;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFormEnabled))]
        [NotifyPropertyChangedFor(nameof(DataGridReadOnly))]
        [NotifyCanExecuteChangedFor(nameof(SaveClientCommand))]
        [NotifyCanExecuteChangedFor(nameof(NewClientCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteClientCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelActionCommand))]

        [NotifyCanExecuteChangedFor(nameof(EditClientCommand))]
        private bool isEditing = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFormEnabled))]
        [NotifyPropertyChangedFor(nameof(DataGridReadOnly))]
        [NotifyCanExecuteChangedFor(nameof(EditClientCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteClientCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelActionCommand))]
        [NotifyCanExecuteChangedFor(nameof(NewClientCommand))]

        private bool isViewing = false;

        public bool IsFormEnabled => IsEditing;
        public bool DataGridReadOnly => IsEditing;

        #endregion      

        #region Comandos

        [RelayCommand]
        private async Task LoadClientsAsync()
        {
            try
            {
                IsBusy = true;

                var listClients = await _repositoryClient.GetAllAsync();

                // mover essa lógica de "IsActive" para dentro do repositório no futuro
                AllClients = listClients.Where(clients => clients.IsActive).ToList();

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
        private async Task SaveClientAsync()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault()?.ErrorMessage;

                Messenger.Send(new StatusMessage(firstError ?? "Verifique os campos obrigatórios.", false));

                return;
            }

            try
            {
                IsBusy = true;

                Client? client;

                if (Id == 0)
                {
                    client = new Client { IsActive = true };
                }
                else
                {
                    client = await _repositoryClient.GetByIdAsync(Id);
                }

                if (client == null) return;

                client.Name = Name;
                client.Phone = Phone;
                client.Observations = Observations;
                client.OutstandingBalance = OutstandingBalance;

                if (Id == 0)
                    await _repositoryClient.AddAsync(client);
                else
                    await _repositoryClient.UpdateAsync(client);

                Messenger.Send(new StatusMessage("Dados salvos com sucesso!", true));
                Messenger.Send(new ClientsChangedMessage());

                await LoadClientsAsync();
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
        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null) return;

            try
            {
                IsBusy = true;

                var client = await _repositoryClient.GetByIdAsync(SelectedClient.Id);

                if (client != null)
                {
                    client.IsActive = false;
                    await _repositoryClient.UpdateAsync(client);

                    Messenger.Send(new StatusMessage("Cliente removido da lista com sucesso!", true));
                    Messenger.Send(new ClientsChangedMessage());
                }

                await LoadClientsAsync();
                CancelAction();
            }
            catch (Exception)
            {
                Messenger.Send(new StatusMessage("Erro ao desativar cliente.", false));
            }
            finally 
            { 
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCreateNew))]
        private void NewClient()
        {
            ClearFields();
            IsEditing = true;
            IsViewing = true;
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private void EditClient() => IsEditing = true;

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelAction()
        {
            ClearFields();
            IsEditing = false;
            IsViewing = false;
        }

        private void ApplyFilter()
        {
            if (AllClients == null) return;

            IEnumerable<Client> query = AllClients;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(client => client.Name != null && client.Name.Contains(SearchText!, StringComparison.OrdinalIgnoreCase));
            }

            FilteredClients = new ObservableCollection<Client>(query.ToList());
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
            Phone = "";
            Observations = "";
            OutstandingBalance = 0;
            SelectedClient = null;
        }

        partial void OnSelectedClientChanged(Client? value)
        {
            if (IsEditing) return;

            if (value != null)
            {
                Id = value.Id;
                Name = value.Name;
                Phone = value.Phone;
                Observations = value.Observations;
                OutstandingBalance = value.OutstandingBalance;
                
                IsEditing = false;
                IsViewing = true;
            }
            else
            {
                if (!IsViewing && !IsEditing) CancelAction();
            }
        }

        partial void OnSearchTextChanged(string? value) => ApplyFilter();

        #endregion
    }
}
