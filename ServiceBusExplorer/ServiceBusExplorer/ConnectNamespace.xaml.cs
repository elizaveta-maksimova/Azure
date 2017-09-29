using System.Windows;
using System.Windows.Input;
using ServiceBusExplorer.ViewModels;

namespace ServiceBusExplorer
{
    public partial class ConnectNamespace : Window
    {
        public ConnectNamespace()
        {
            InitializeComponent();
        }

        private MainViewModel MainViewModel
        {
            get
            {
                return DataContext as MainViewModel;
            }
        }

        private void CanConnect(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null && MainViewModel.CanConnectNamespace(e.Parameter as string);
        }

        private void Connect(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.ConnectNamespace(e.Parameter as string);
            
            Close();
        }
    }
}
