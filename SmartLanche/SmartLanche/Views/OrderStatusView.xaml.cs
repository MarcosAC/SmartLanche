using SmartLanche.Models;
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

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            var comboBox = sender as ComboBox;
          
            if (comboBox != null && comboBox.IsLoaded && comboBox.IsFocused)
            {                
                if (comboBox.DataContext is Order order)
                {
                    if (DataContext is OrderStatusViewModel viewModel)
                    {
                        viewModel.ChangeStatusCommand.Execute(order);
                    }
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrderStatusViewModel viewModel)
            {
                await viewModel.LoadOrdersCommand.ExecuteAsync(null);
            }
        }
    }
}
