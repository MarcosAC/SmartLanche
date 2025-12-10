using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

            Clients = new ObservableCollection<Client>();

            LoadClientsCommand = new AsyncRelayCommand(LoadClientsAsync);
            SaveClientCommand = new AsyncRelayCommand(SaveClientAsync, () => IsEditing);
            DeleteClientCommand = new AsyncRelayCommand(DeleteClientAsync, () => IsViewing && !IsEditing);
            NewClientCommand = new RelayCommand(NewClient, () => !IsEditing && !IsViewing);
            CancelCommand = new RelayCommand(CancelAction, () => IsViewing || IsEditing);
            EditClientCommand = new RelayCommand(EditClient, () => IsViewing && !IsEditing);

            _ = LoadClientsAsync();
        }       

        [ObservableProperty]
        private ObservableCollection<Client> clients;

        [ObservableProperty]
        private Client? selectedClient;

        [ObservableProperty] private int id;

        [Required(ErrorMessage = "O nome é obrigatório.")]        
        [ObservableProperty] private string name = "";

        [ObservableProperty] private string? phone;

        [ObservableProperty] private string? observations;

        [ObservableProperty] private decimal outstandingBalance;

        [ObservableProperty] private bool isEditing = false;        
        [ObservableProperty] private bool isViewing = false;

        public bool IsFormEnabled => IsEditing;
        public bool DataGridReadOnly => IsEditing;      

        public IAsyncRelayCommand LoadClientsCommand { get; }
        public IAsyncRelayCommand SaveClientCommand { get; }
        public IAsyncRelayCommand DeleteClientCommand { get; }
        public IRelayCommand NewClientCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IRelayCommand EditClientCommand { get; }

        private async Task LoadClientsAsync()
        {
            Clients.Clear();

            var listClients = await _repositoryClient.GetAllAsync();

            foreach (var client in listClients)
                Clients.Add(client);
        }

        partial void OnSelectedClientChanged(Client? value)
        {
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
                CancelAction();
            }

            NewClientCommand.NotifyCanExecuteChanged();
            SaveClientCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            EditClientCommand.NotifyCanExecuteChanged();
            DeleteClientCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
            OnPropertyChanged(nameof(IsFormEnabled));
        }

        partial void OnIsEditingChanged(bool value)
        {
            NewClientCommand.NotifyCanExecuteChanged();
            SaveClientCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
            OnPropertyChanged(nameof(IsFormEnabled));
        }

        private async Task SaveClientAsync()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                var errors = string.Join("\n", GetErrors().Select(e => e.ErrorMessage));

                Messenger.Send(new Messages.StatusMessage($"Erro ao salvar cliente:\n{errors}", isSuccess: false));

                return;
            }

            if (Id == 0)
            {
                var client = new Client
                {
                    Name = Name,
                    Phone = Phone,
                    Observations = Observations,
                    OutstandingBalance = OutstandingBalance,
                };

                await _repositoryClient.AddAsync(client);

                Messenger.Send(new Messages.StatusMessage($"Cliente '{client.Name}' cadastro com sucesso!", isSuccess: true));
            }
            else
            {
                var client = await _repositoryClient.GetByIdAsync(Id);

                if (client == null) return;

                client.Name = Name;
                client.Phone = Phone;
                client.Observations = Observations;

                await _repositoryClient.UpdateAsync(client);

                Messenger.Send(new Messages.StatusMessage($"Cliente '{client.Name}' atualizado com sucesso!", isSuccess: true));
            }
        }

        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null) return;

            if (SelectedClient.OutstandingBalance > 0)
            {
                Messenger.Send(new Messages.StatusMessage("Não é possível excluir o cliente: Saldo devedor pendente.", isSuccess: false));
                return;
            }

            string clientName = SelectedClient.Name;

            await _repositoryClient.DeleteAsync(SelectedClient.Id);
            await LoadClientsAsync();

            Messenger.Send(new Messages.StatusMessage($"Cliente '{clientName}' excluido com sucesso!", isSuccess: true));

            CancelAction();
        }

        private void NewClient()
        {
            Id = 0;
            Name = "";
            Phone = "";
            Observations = "";
            OutstandingBalance = 0.00m;
            
            SelectedClient = null;
            IsEditing = true;
            IsViewing = true;

            NewClientCommand.NotifyCanExecuteChanged();
            SaveClientCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            //DeleteClientCommand.NotifyCanExecuteChanged();
            //EditClientCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(IsFormEnabled));
            OnPropertyChanged(nameof(DataGridReadOnly));
        }

        private void EditClient()
        {
            IsEditing = true;

            NewClientCommand.NotifyCanExecuteChanged();
            SaveClientCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            DeleteClientCommand.NotifyCanExecuteChanged();
            EditClientCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(IsFormEnabled));
            OnPropertyChanged(nameof(DataGridReadOnly));
        }

        private void CancelAction()
        {
            Id = 0;
            Name = "";
            Phone = "";
            Observations = "";
            OutstandingBalance = 0.00m;
            SelectedClient = null;

            SelectedClient = null;
            IsEditing = false;
            IsViewing = false;

            NewClientCommand.NotifyCanExecuteChanged();
            SaveClientCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            EditClientCommand.NotifyCanExecuteChanged();
            DeleteClientCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(DataGridReadOnly));
            OnPropertyChanged(nameof(IsFormEnabled));
        }
    }
}
