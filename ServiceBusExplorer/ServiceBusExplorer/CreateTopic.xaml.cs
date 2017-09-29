using System.Windows;
using System.Windows.Input;
using ServiceBusExplorer.ViewModels;

namespace ServiceBusExplorer
{
    public partial class CreateTopic : Window
    {
        public CreateTopic()
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
            e.CanExecute = e.Parameter != null && MainViewModel.CanCreateTopic(e.Parameter as CreateTopicViewModel);
        }

        private void Create(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.CreateTopic(e.Parameter as CreateTopicViewModel);
            Close();
        }
    }
}
