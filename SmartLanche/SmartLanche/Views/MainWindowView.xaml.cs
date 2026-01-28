using CommunityToolkit.Mvvm.Messaging;
using SmartLanche.Messages;
using SmartLanche.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                var destination = selectedTab.Tag?.ToString();
                if (!string.IsNullOrEmpty(destination) && DataContext is MainWindowViewModel vm)
                {
                    vm.NavigateCommand.Execute(destination);
                }
            }
        }
    }
}
