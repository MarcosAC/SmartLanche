using SmartLanche.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartLanche.Views
{   
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
        }

        //private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is SalesViewModel viewModel)
        //    {
        //        await viewModel.LoadDataCommand.ExecuteAsync(null);
        //    }
        //}
    }
}