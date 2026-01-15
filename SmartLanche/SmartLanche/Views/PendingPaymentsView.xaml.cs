using SmartLanche.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartLanche.Views
{    
    public partial class PendingPaymentsView : UserControl
    {
        public PendingPaymentsView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PendingPaymentsViewModel viewModel)
            {
                await viewModel.LoadPendingDataCommand.ExecuteAsync(null);
            }
        }
    }
}
