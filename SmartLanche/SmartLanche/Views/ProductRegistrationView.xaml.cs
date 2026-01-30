using SmartLanche.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartLanche.Views
{    
    public partial class ProductRegistrationView : UserControl
    {
        public ProductRegistrationView()
        {
            InitializeComponent();            
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TxtSearch.Focus();

            if (DataContext is ProductRegistrationViewModel viewModel)
            {
                viewModel.ResetScreenState();
                await viewModel.LoadProductsCommand.ExecuteAsync(null);
            }
        }
    }
}
