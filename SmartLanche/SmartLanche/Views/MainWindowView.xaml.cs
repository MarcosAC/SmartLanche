using System.Windows;

namespace SmartLanche.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView(ViewModels.MainWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
