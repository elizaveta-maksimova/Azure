using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace ServiceBusExplorer.ViewModels
{
    public class SubscriptionViewModel : ObservableObject, IMessageProcessor
    {
        private string _name;
        private bool _isRunning;

        public string Name
        {
            get { return _name; }

            set { Set(() => Name, ref _name, value); }
        }

        public bool IsRunning
        {
            get { return _isRunning; }

            set { Set(() => IsRunning, ref _isRunning, value); }
        }

        public Task Task { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
