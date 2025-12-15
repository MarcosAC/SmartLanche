using SmartLanche.Models;
using SmartLanche.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartLanche.Views
{   
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                if (item.Content is Product product)
                {
                    if (DataContext is SalesViewModel viewModel)
                    {
                        if (viewModel.AddProductToCartCommand.CanExecute(product))
                        {
                            viewModel.AddProductToCartCommand.Execute(product);
                        }
                    }
                }
            }
        }
    }
}