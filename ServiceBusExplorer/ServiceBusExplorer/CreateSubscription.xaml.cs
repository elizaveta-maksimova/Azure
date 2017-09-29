using System.Windows;
using System.Windows.Input;
using ServiceBusExplorer.ViewModels;

namespace ServiceBusExplorer
{
    public partial class CreateSubscription : Window
    {
        public CreateSubscription()
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

        private void CanCreate(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null && MainViewModel.CanCreateSubscription(e.Parameter as CreateSubscriptionViewModel);
        }

        private void Create(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.CreateSubscription(e.Parameter as CreateSubscriptionViewModel);
            Close();
        }
    }
}
