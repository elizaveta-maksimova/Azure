using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NLog;

namespace ServiceBusExplorer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(Constants.AppLogger);
        private string _connectionString;
        private NamespaceManager _namespaceManager;
        private int _processorsCount;
        private ObservableObject _selectedViewModel;

        public MainViewModel()
        {
            ViewModels = new ObservableCollection<ObservableObject>();

            if (IsInDesignMode)
            {
                ViewModels.Add(new QueueViewModel { Name = "Queue1" });
                ViewModels.Add(new QueueViewModel { Name = "Queue2" });

                var topic = new TopicViewModel { Name = "Topic1" };

                topic.ViewModels.Add(new SubscriptionViewModel { Name = "Subscription1" });
                topic.ViewModels.Add(new SubscriptionViewModel { Name = "Subscription2" });

                ViewModels.Add(topic);

                ViewModels.Add(new TopicViewModel { Name = "Topic2" });

                SelectedViewModel = ViewModels.First();
            }
        }

        public ObservableCollection<ObservableObject> ViewModels { get; private set; }

        public ObservableObject SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { Set(() => SelectedViewModel, ref _selectedViewModel, value); }
        }

        public bool IsProcessing
        {
            get { return _processorsCount > 0; }
        }

        public string Status
        {
            get
            {
                if (_namespaceManager == null)
                {
                    return "Disconnected";
                }

                return string.Format(CultureInfo.InvariantCulture, "Connected: {0}", _namespaceManager.Address.Host);
            }
        }

        public bool CanDisconnect()
        {
            return _namespaceManager != null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is being logged.")]
        public void Disconnect()
        {
            try
            {
                Logger.Trace(CultureInfo.InvariantCulture, "Disconnecting service bus namespace '{0}'...", _namespaceManager.Address.Host);

                var name = _namespaceManager.Address.Host;

                _namespaceManager = null;
                _connectionString = null;

                StopProcessors();

                ViewModels.Clear();

                Logger.Info(CultureInfo.InvariantCulture, "Service bus namespace '{0}' disconnected successfully.", name);

                RaisePropertyChanged(() => Status);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, CultureInfo.InvariantCulture, "Failed to disconnect service bus namespace");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "No need to do this")]
        public bool CanConnectNamespace(string connectionString)
        {
            return !string.IsNullOrEmpty(connectionString);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is being logged.")]
        public void ConnectNamespace(string connectionString)
        {
            try
            {
                _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
                _connectionString = connectionString;

                Logger.Info(CultureInfo.InvariantCulture, "Service bus namespace '{0}' connected successfully.", _namespaceManager.Address.Host);

                RaisePropertyChanged(() => Status);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, CultureInfo.InvariantCulture, "Failed to connect service bus namespace");
            }
        }

        public bool CanCreateQueue(CreateQueueViewModel viewModel)
        {
            return CanCreateQueueCore(viewModel);
        }

        public void CreateQueue(CreateQueueViewModel viewModel)
        {
            CreateQueueAsync(viewModel);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "No need to do this.")]
        public bool CanCreateTopic(CreateTopicViewModel viewModel)
        {
            return CanCreateQueueCore(viewModel);
        }

        public void CreateTopic(CreateTopicViewModel viewModel)
        {
            CreateTopicAsync(viewModel);
        }

        public bool CanCreateSubscription(CreateSubscriptionViewModel viewModel)
        {
            var topicViewModel = SelectedViewModel as TopicViewModel;

            return topicViewModel != null && viewModel != null && !string.IsNullOrEmpty(viewModel.Name);
        }

        public void CreateSubscription(CreateSubscriptionViewModel viewModel)
        {
            var topicViewModel = SelectedViewModel as TopicViewModel;

            CreateSubscriptionAsync(topicViewModel, viewModel);
        }

        public bool CanRefresh()
        {
            return _namespaceManager != null;
        }

        public void Refresh()
        {
            RefreshAsync();
        }

        public bool CanDelete()
        {
            return SelectedViewModel != null;
        }

        public void Delete()
        {
            var processor = SelectedViewModel as IMessageProcessor;

            if (processor != null && processor.IsRunning)
            {
                StopProcessing(processor);
            }

            var queue = SelectedViewModel as QueueViewModel;

            if (queue != null)
            {
                DeleteQueueAsync(queue);
                return;
            }

            var topic = SelectedViewModel as TopicViewModel;

            if (topic != null)
            {
                DeleteTopicAsync(topic);
                return;
            }

            var subscription = SelectedViewModel as SubscriptionViewModel;

            if (subscription != null)
            {
                DeleteSubscriptionAsync(subscription);
            }
        }

        public bool CanSend(MessageViewModel viewModel)
        {
            return SelectedViewModel != null &&
                   (SelectedViewModel is TopicViewModel || SelectedViewModel is QueueViewModel) &&
                   viewModel != null &&
                   !string.IsNullOrEmpty(viewModel.Body);
        }

        public void Send(MessageViewModel viewModel)
        {
            var selectedViewModel = SelectedViewModel;

            var topicViewModel = selectedViewModel as TopicViewModel;

            if (topicViewModel != null)
            {
                SendToTopicAsync(topicViewModel, viewModel);
            }

            var queueViewModel = selectedViewModel as QueueViewModel;

            if (queueViewModel != null)
            {
                SendToQueueAsync(queueViewModel, viewModel);
            }
        }

        public bool CanStartProcessing(ProcessMessageViewModel viewModel)
        {
            var messageProcessor = SelectedViewModel as IMessageProcessor;

            return messageProcessor != null &&
                   !messageProcessor.IsRunning &&
                   viewModel != null &&
                   viewModel.Timeout > TimeSpan.Zero;
        }

        public bool CanStopProcessing()
        {
            var messageProcessor = SelectedViewModel as IMessageProcessor;

            return messageProcessor != null && messageProcessor.IsRunning;
        }

        public void StartProcessing(ProcessMessageViewModel viewModel)
        {
            var selectedViewModel = SelectedViewModel;

            var topicViewModel = selectedViewModel as SubscriptionViewModel;

            if (topicViewModel != null)
            {
                StartSubscriptionProcessing(topicViewModel, viewModel);
            }

            var queueViewModel = selectedViewModel as QueueViewModel;

            if (queueViewModel != null)
            {
                StartQueueProcessing(queueViewModel, viewModel);
            }
        }

        public void StopProcessing()
        {
            StopProcessing(SelectedViewModel as IMessageProcessor);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "No need to do this")]
        public void StopProcessing(IMessageProcessor processor)
        {
            Logger.Trace(CultureInfo.InvariantCulture, "Stopping processing '{0}'...", processor.Name);

            processor.CancellationTokenSource.Cancel();
        }

        private bool CanCreateQueueCore(CreateQueueViewModel viewModel)
        {
            return _namespaceManager != null &&
                    viewModel != null &&
                    !string.IsNullOrEmpty(viewModel.Name) &&
                    viewModel.Size >= 1 &&
                    viewModel.Size <= 5;
        }

        private async Task CreateQueueAsync(CreateQueueViewModel viewModel)
        {
            try
            {
                Logger.Trace("Creating service bus queue '{0}'...", viewModel.Name);

                var description = new QueueDescription(viewModel.Name)
                {
                    EnablePartitioning = viewModel.EnablePartitioning,
                    MaxSizeInMegabytes = viewModel.Size * 1024
                };

                var createdDescription = await _namespaceManager.CreateQueueAsync(description);

                var result = new QueueViewModel { Name = createdDescription.Path };

                ViewModels.Add(result);

                Logger.Info("Service bus queue '{0}' created successfully.", viewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to create service bus queue");
            }
        }

        private async Task CreateTopicAsync(CreateTopicViewModel viewModel)
        {
            try
            {
                Logger.Trace("Creating service bus topic '{0}'...", viewModel.Name);

                var description = new TopicDescription(viewModel.Name)
                {
                    EnablePartitioning = viewModel.EnablePartitioning,
                    MaxSizeInMegabytes = viewModel.Size * 1024
                };

                var createdDescription = await _namespaceManager.CreateTopicAsync(description);

                var result = new TopicViewModel() { Name = createdDescription.Path };

                ViewModels.Add(result);

                Logger.Info("Service bus topic '{0}' created successfully.", viewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to create service bus topic");
            }
        }

        private async Task CreateSubscriptionAsync(TopicViewModel topicViewModel, CreateSubscriptionViewModel viewModel)
        {
            try
            {
                Logger.Trace("Creating service bus subscription '{0}'...", viewModel.Name);

                var description = new SubscriptionDescription(topicViewModel.Name, viewModel.Name);

                SubscriptionDescription createdDescription = null;

                if (!string.IsNullOrEmpty(viewModel.Filter))
                {
                    var filter = new SqlFilter(viewModel.Filter);

                    createdDescription = await _namespaceManager.CreateSubscriptionAsync(description, filter);
                }
                else
                {
                    createdDescription = await _namespaceManager.CreateSubscriptionAsync(description);
                }

                var result = new SubscriptionViewModel { Name = createdDescription.Name };

                topicViewModel.ViewModels.Add(result);

                Logger.Info("Service bus subscription '{0}' created successfully.", viewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to create service bus subscription");
            }
        }

        private async Task DeleteQueueAsync(QueueViewModel viewModel)
        {
            try
            {
                Logger.Trace("Deleting service bus queue '{0}'...", viewModel.Name);

                await _namespaceManager.DeleteQueueAsync(viewModel.Name);

                ViewModels.Remove(viewModel);

                Logger.Info("Service bus queue '{0}' deleted successfully.", viewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to delete service bus queue");
            }
        }

        private async Task DeleteTopicAsync(TopicViewModel viewModel)
        {
            try
            {
                Logger.Trace("Deleting service bus topic '{0}'...", viewModel.Name);

                foreach (var subscriptionViewModel in viewModel.ViewModels.Where(s => s.IsRunning))
                {
                    StopProcessing(subscriptionViewModel);
                }

                await _namespaceManager.DeleteTopicAsync(viewModel.Name);

                ViewModels.Remove(viewModel);

                Logger.Info("Service bus topic '{0}' deleted successfully.", viewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to delete service bus topic");
            }
        }

        private async Task DeleteSubscriptionAsync(SubscriptionViewModel viewModel)
        {
            try
            {
                Logger.Trace("Deleting service bus subscription '{0}'...", viewModel.Name);

                if (viewModel.IsRunning)
                {
                    StopProcessing(viewModel);
                }

                var topic = ViewModels.OfType<TopicViewModel>().First(t => t.ViewModels.Contains(viewModel));

                await _namespaceManager.DeleteSubscriptionAsync(topic.Name, viewModel.Name);

                topic.ViewModels.Remove(viewModel);

                Logger.Info("Service bus subscription '{0}' deleted successfully.", viewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to delete service bus subscription");
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                Logger.Trace("Refreshing service bus namespace '{0}'...", _namespaceManager.Address.Host);

                var queues = await _namespaceManager.GetQueuesAsync();
                var topics = await _namespaceManager.GetTopicsAsync();

                var result = new List<ViewModelBase>();

                foreach (var topic in topics)
                {
                    var topicViewModel = new TopicViewModel { Name = topic.Path };

                    var subscriptions = await _namespaceManager.GetSubscriptionsAsync(topic.Path);

                    foreach (var subscription in subscriptions)
                    {
                        topicViewModel.ViewModels.Add(new SubscriptionViewModel { Name = subscription.Name });
                    }

                    result.Add(topicViewModel);
                }

                result.AddRange(queues.Select(q => new QueueViewModel { Name = q.Path }));

                StopProcessors();

                ViewModels.Clear();

                foreach (var viewModel in result)
                {
                    ViewModels.Add(viewModel);
                }

                Logger.Info("Service bus namespace '{0}' refreshed successfully.", _namespaceManager.Address.Host);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to refresh service bus namespace");
            }
        }

        private async Task SendToQueueAsync(QueueViewModel queueViewModel, MessageViewModel messageViewModel)
        {
            try
            {
                Logger.Trace("Sending message...");

                var queue = QueueClient.CreateFromConnectionString(_connectionString, queueViewModel.Name);

                var message = BuildMessage(messageViewModel);

                await queue.SendAsync(message);

                Logger.Info("Message was sent to the queue '{0}'", queueViewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to send message to the service bus queue");
            }
        }

        private async Task SendToTopicAsync(TopicViewModel topicViewModel, MessageViewModel messageViewModel)
        {
            try
            {
                Logger.Trace("Sending message...");

                var topic = TopicClient.CreateFromConnectionString(_connectionString, topicViewModel.Name);

                var message = BuildMessage(messageViewModel);

                await topic.SendAsync(message);

                Logger.Info("Message was sent to the topic '{0}'", topicViewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to send message to the service bus topic");
            }
        }

        private void StartQueueProcessing(QueueViewModel queueViewModel, ProcessMessageViewModel processMessage)
        {
            Logger.Trace(CultureInfo.InvariantCulture, "Starting queue '{0}' processing...", queueViewModel.Name);

            var cancellationTokenSource = new CancellationTokenSource();

            queueViewModel.CancellationTokenSource = cancellationTokenSource;
            queueViewModel.Task = Task.Factory.StartNew(() => ProcessQueue(processMessage, queueViewModel));

            Logger.Info(CultureInfo.InvariantCulture, "Started queue '{0}' processing", queueViewModel.Name);
        }

        private void StartSubscriptionProcessing(SubscriptionViewModel subscriptionViewModel, ProcessMessageViewModel processMessage)
        {
            Logger.Trace(CultureInfo.InvariantCulture, "Starting subscription '{0}' processing...", subscriptionViewModel.Name);

            var topicViewModel = ViewModels.OfType<TopicViewModel>().First(t => t.ViewModels.Contains(subscriptionViewModel));
            var cancellationTokenSource = new CancellationTokenSource();
            subscriptionViewModel.CancellationTokenSource = cancellationTokenSource;
            subscriptionViewModel.Task = Task.Factory.StartNew(() => ProcessSubscription(processMessage, topicViewModel, subscriptionViewModel));

            Logger.Info(CultureInfo.InvariantCulture, "Started subscription '{0}' processing", subscriptionViewModel.Name);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is being logged.")]
        private void ProcessQueue(ProcessMessageViewModel processMessage, QueueViewModel queueViewModel)
        {
            SetProcessorsCount();

            try
            {
                var queue = QueueClient.CreateFromConnectionString(_connectionString, queueViewModel.Name, processMessage.Mode);

                var token = queueViewModel.CancellationTokenSource.Token;

                queueViewModel.IsRunning = true;

                while (true)
                {
                    var message = queue.Receive(processMessage.Timeout);

                    if (message != null)
                    {
                        using (var reader = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8))
                        {
                            Logger.Info("Recieved message from queue '{0}':{1}{2}", queueViewModel.Name, Environment.NewLine, reader.ReadToEnd());
                        }

                        if (queue.Mode == ReceiveMode.PeekLock)
                        {
                            message.Complete();
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                Logger.Info(CultureInfo.InvariantCulture, "Stopped queue processing '{0}'", queueViewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, CultureInfo.InvariantCulture, "Failed to process service bus queue");
            }
            finally
            {
                SetProcessorsCount(false);
                queueViewModel.IsRunning = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is being logged.")]
        private void ProcessSubscription(ProcessMessageViewModel processMessage, TopicViewModel topicViewModel, SubscriptionViewModel subscriptionViewModel)
        {
            SetProcessorsCount();

            try
            {
                var subscription = SubscriptionClient.CreateFromConnectionString(_connectionString, topicViewModel.Name, subscriptionViewModel.Name, processMessage.Mode);

                var token = subscriptionViewModel.CancellationTokenSource.Token;

                subscriptionViewModel.IsRunning = true;

                while (true)
                {
                    var message = subscription.Receive(processMessage.Timeout);

                    if (message != null)
                    {
                        using (var reader = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8))
                        {
                            Logger.Info("Recieved message from subscription '{0}':{1}{2}", subscriptionViewModel.Name, Environment.NewLine, reader.ReadToEnd());
                        }

                        if (subscription.Mode == ReceiveMode.PeekLock)
                        {
                            message.Complete();
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                Logger.Info(CultureInfo.InvariantCulture, "Stopped queue processing '{0}'", subscriptionViewModel.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, CultureInfo.InvariantCulture, "Failed to process service bus subscription");
            }
            finally
            {
                SetProcessorsCount(false);
                subscriptionViewModel.IsRunning = false;
            }
        }

        private void StopProcessors()
        {
            var processors = ViewModels.OfType<IMessageProcessor>().Where(p => p.IsRunning).ToList();

            foreach (var processor in processors)
            {
                StopProcessing(processor);
            }
        }

        private void SetProcessorsCount(bool increment = true)
        {
            if (increment)
            {
                Interlocked.Increment(ref _processorsCount);
            }
            else
            {
                Interlocked.Decrement(ref _processorsCount);
            }

            RaisePropertyChanged(() => IsProcessing);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "An instance of the BrokeredMessage type controls stream's lifetime.")]
        private static BrokeredMessage BuildMessage(MessageViewModel messageViewModel)
        {
            var byteArray = Encoding.UTF8.GetBytes(messageViewModel.Body);

            var stream = new MemoryStream(byteArray);
            
            var message = new BrokeredMessage(stream, true);

            foreach (var propertyViewModel in messageViewModel.Properties)
            {
                var value = 0;

                if (int.TryParse(propertyViewModel.Value, out value))
                {
                    message.Properties.Add(propertyViewModel.Name, value);
                }
                else
                {
                    message.Properties.Add(propertyViewModel.Name, propertyViewModel.Value);
                }
            }

            return message;
        }
    }
}
