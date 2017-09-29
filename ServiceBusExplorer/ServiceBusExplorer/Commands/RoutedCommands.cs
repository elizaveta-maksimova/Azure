using System.Windows.Input;

namespace ServiceBusExplorer.Commands
{
    public static class RoutedCommands
    {
        public static readonly RoutedCommand OpenConnectNamespaceView = new RoutedCommand();

        public static readonly RoutedCommand OpenCreateQueueView = new RoutedCommand();

        public static readonly RoutedCommand OpenCreateTopicView = new RoutedCommand();

        public static readonly RoutedCommand OpenCreateSubscriptionView = new RoutedCommand();

        public static readonly RoutedCommand OpenStartProcessingView = new RoutedCommand();
        
        public static readonly RoutedCommand ConnectNamespace = new RoutedCommand();

        public static readonly RoutedCommand RefreshNamespace = new RoutedCommand();

        public static readonly RoutedCommand DisconnectNamespace = new RoutedCommand();

        public static readonly RoutedCommand CreateQueue = new RoutedCommand();

        public static readonly RoutedCommand CreateTopic = new RoutedCommand();

        public static readonly RoutedCommand CreateSubscription = new RoutedCommand();

        public static readonly RoutedCommand DeleteItem = new RoutedCommand();

        public static readonly RoutedCommand SendMessage = new RoutedCommand();

        public static readonly RoutedCommand StartProcessing = new RoutedCommand();

        public static readonly RoutedCommand StopProcessing = new RoutedCommand();
    }
}
