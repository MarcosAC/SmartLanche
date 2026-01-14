using SmartLanche.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartLanche.Views
{
    public partial class ClientRegistrationView : UserControl
    {
        public ClientRegistrationView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClientRegistrationViewModel viewModel)
            {
                await viewModel.LoadClientsCommand.ExecuteAsync(null);
            }
        }
    }
}
