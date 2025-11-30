using Microsoft.Extensions.DependencyInjection;
using SmartLanche.Models;
using SmartLanche.Services;
using System.Windows;
using System.Windows.Controls;

namespace SmartLanche.Views
{    
    public partial class ProductRegistratitionView : UserControl
    {
        private readonly IRepository<Product> _repositoryProduct;

        public ProductRegistratitionView()
        {
            InitializeComponent();
            _repositoryProduct = App.ServiceProvider!.GetRequiredService<IRepository<Product>>();
            Loaded += ProductRegistratitionView_Loaded;
        }

        private async void ProductRegistratitionView_Loaded(object sender, RoutedEventArgs e)
        {
            var listProducts = await _repositoryProduct.GetAllAsync();
            GridProducts.ItemsSource = listProducts;
        }

        private async void New_Click(object sender, RoutedEventArgs e)
        {
            var product = new Product { Name = "Novo Produto", Price = 0.0m };

            var listProducts = (GridProducts.ItemsSource as IList<Product>) ?? new List<Product>();

            listProducts.Add(product);

            GridProducts.ItemsSource = null;
            GridProducts.ItemsSource = listProducts;
            GridProducts.SelectedItem = product;
            GridProducts.ScrollIntoView(product);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (GridProducts.SelectedItem is Product product && product.Id == 0)
            {
                await _repositoryProduct.AddAsync(product);
            }
            else if (GridProducts.SelectedItem is Product produto2)
            {
                await _repositoryProduct.UpdateAsync(produto2);
            }

            var listProducts = await _repositoryProduct.GetAllAsync();
            GridProducts.ItemsSource = listProducts;
            MessageBox.Show("Salvo com sucesso");
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (GridProducts.SelectedItems is Product product && product.Id != 0)
            {
                await _repositoryProduct.DeleteAsync(product.Id);
                GridProducts.ItemsSource = await _repositoryProduct.GetAllAsync();
            }
        }
    }
}
