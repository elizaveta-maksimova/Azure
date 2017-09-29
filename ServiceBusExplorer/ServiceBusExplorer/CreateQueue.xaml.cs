using System.Windows;
using System.Windows.Input;
using ServiceBusExplorer.ViewModels;

namespace ServiceBusExplorer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "It is OK")]
    public partial class CreateQueue : Window
    {
        public CreateQueue()
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
            e.CanExecute = e.Parameter != null && MainViewModel.CanCreateQueue(e.Parameter as CreateQueueViewModel);
        }

        private void Create(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.CreateQueue(e.Parameter as CreateQueueViewModel);
            Close();
        }
    }
}
