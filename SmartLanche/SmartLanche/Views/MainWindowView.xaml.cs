using SmartLanche.ViewModels;
using System.Windows;

namespace SmartLanche.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView(MainWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
