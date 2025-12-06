using CommunityToolkit.Mvvm.Messaging;
using SmartLanche.Messages;
using SmartLanche.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace SmartLanche.Views
{
    public partial class MainWindowView : Window, IRecipient<StatusMessage>
    {
        private readonly IMessenger _messenger;

        public MainWindowView(MainWindowViewModel vm, IMessenger messenger)
        {
            InitializeComponent();
            DataContext = vm;

            _messenger = messenger;

            _messenger.Register<StatusMessage>(this);
        }

        public void Receive(StatusMessage message)
        {
            MessageBoxImage icon = message.IsSuccess
                ? MessageBoxImage.Information
                : MessageBoxImage.Error;

            string title = message.IsSuccess ? "Sucesso" : "Erro de Validação";

            MessageBox.Show(
                message.Content,
                title,
                MessageBoxButton.OK,
                icon
            );
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _messenger.Unregister<StatusMessage>(this);
            base.OnClosing(e);
        }
    }
}
