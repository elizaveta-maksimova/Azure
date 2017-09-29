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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using ServiceBusExplorer.ViewModels;

namespace ServiceBusExplorer
{
    public partial class MainWindow : Window
    {
        private const string ConsoleTargetName = "WpfConsole";
        private AsyncTargetWrapper _wrapper;

        public MainWindow()
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It is OK.")]
        private void MainWindowInitialized(object sender, EventArgs e)
        {
            DispatcherHelper.Initialize();

            var target = new WpfRichTextBoxTarget
            {
                Name = ConsoleTargetName,
                Layout = "${pad:padding=-5:inner=${level:upperCase=true}} ${date} ${message}${newline}${onexception:Exception: ${exception:format=message}${newline}}",
                ControlName = OutputTextBox.Name,
                FormName = Name,
                AutoScroll = true,
                MaxLines = 10,
                UseDefaultRowColoringRules = false,
            };

            target.RowColoringRules.Add(new WpfRichTextBoxRowColoringRule("level == LogLevel.Fatal", "Red", "Empty"));
            target.RowColoringRules.Add(new WpfRichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty"));
            target.RowColoringRules.Add(new WpfRichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty"));
            target.RowColoringRules.Add(new WpfRichTextBoxRowColoringRule("level == LogLevel.Info", "Green", "Empty"));
            target.RowColoringRules.Add(new WpfRichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"));
            target.RowColoringRules.Add(new WpfRichTextBoxRowColoringRule("level == LogLevel.Trace", "Gray", "Empty"));

            _wrapper = new AsyncTargetWrapper
            {
                Name = ConsoleTargetName,
                WrappedTarget = target
            };

            SimpleConfigurator.ConfigureForTargetLogging(_wrapper, LogLevel.Trace);
        }

        private void CanOpenConnectNamespace(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !MainViewModel.CanDisconnect();
            e.ContinueRouting = false;
        }

        private void OpenConnectNamespace(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new ConnectNamespace() { Owner = this };
            window.ShowDialog();
        }

        private void CanOpenCreateQueue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.CanDisconnect();
            e.ContinueRouting = false;
        }

        private void OpenCreateQueue(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new CreateQueue() { Owner = this };
            window.ShowDialog();
        }

        private void CanOpenCreateTopic(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.CanDisconnect();
            e.ContinueRouting = false;
        }

        private void OpenCreateTopic(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new CreateTopic() { Owner = this };
            window.ShowDialog();
        }

        private void CanOpenCreateSubscription(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.SelectedViewModel is TopicViewModel;
            e.ContinueRouting = false;
        }

        private void OpenCreateSubscription(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new CreateSubscription() { Owner = this };
            window.ShowDialog();
        }

        private void CanDisconnectNamespace(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.CanDisconnect();
            e.ContinueRouting = false;
        }

        private void DisconnectNamespace(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.Disconnect();
        }

        private void CanRefreshNamespace(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.CanRefresh();
        }

        private void RefreshNamespace(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.Refresh();
        }

        private void CanSendMessage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null && MainViewModel.CanSend(e.Parameter as MessageViewModel);
        }

        private void SendMessage(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.Send(e.Parameter as MessageViewModel);
        }

        private void CanOpenStartProcessingView(object sender, CanExecuteRoutedEventArgs e)
        {
            var processor = MainViewModel.SelectedViewModel as IMessageProcessor;
            e.CanExecute = processor != null && !processor.IsRunning;
            e.ContinueRouting = false;
        }

        private void OpenStartProcessingView(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new StartProcessing() { Owner = this };
            window.ShowDialog();
        }

        private void CanStopProcessing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.CanStopProcessing();
        }

        private void StopProcessing(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.StopProcessing();
        }

        private void CanDeleteItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewModel.CanDelete();
        }

        private void DeleteItem(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.Delete();
        }
    }
}
