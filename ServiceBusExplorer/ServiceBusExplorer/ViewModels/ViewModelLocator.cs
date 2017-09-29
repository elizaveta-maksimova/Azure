using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using NLog;

namespace ServiceBusExplorer.ViewModels
{
    public class ViewModelLocator
    {
        private static readonly ILogger Logger = LogManager.GetLogger(Constants.AppLogger);

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MessageViewModel>();
            Messenger.Default.Register<NotificationMessage>(this, NotifyUserMethod);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It is being used in XAML code")]
        public MainViewModel MainViewModel
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It is being used in XAML code")]
        public CreateQueueViewModel CreateQueueViewModel
        {
            get { return new CreateQueueViewModel(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It is being used in XAML code")]
        public CreateTopicViewModel CreateTopicViewModel
        {
            get { return new CreateTopicViewModel(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It is being used in XAML code")]
        public CreateSubscriptionViewModel CreateSubscriptionViewModel
        {
            get { return new CreateSubscriptionViewModel(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It is being used in XAML code")]
        public MessageViewModel MessageViewModel
        {
            get { return ServiceLocator.Current.GetInstance<MessageViewModel>(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It is being used in XAML code")]
        public ProcessMessageViewModel ProcessMessageViewModel
        {
            get { return new ProcessMessageViewModel(); }
        }

        private void NotifyUserMethod(NotificationMessage message)
        {
            Logger.Info(message.Notification);

            MessageBox.Show(message.Notification);
        }
    }
}
