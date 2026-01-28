using SmartLanche.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartLanche.Views
{    
    public partial class OrderStatusView : UserControl
    {
        public OrderStatusView()
        {
            InitializeComponent();
        }        

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrderStatusViewModel viewModel)
            {
                viewModel.InitializeFilters();
                await viewModel.LoadOrdersCommand.ExecuteAsync(null);
            }
        }
    }
}
