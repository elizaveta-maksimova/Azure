using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ServiceBusExplorer.ViewModels;

namespace ServiceBusExplorer
{
    public partial class StartProcessing : Window
    {
        public StartProcessing()
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

        private void CanStart(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null && MainViewModel.CanStartProcessing(e.Parameter as ProcessMessageViewModel);
        }

        private void Start(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.StartProcessing(e.Parameter as ProcessMessageViewModel);
            Close();
        }
    }
}
