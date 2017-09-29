using System;
using GalaSoft.MvvmLight;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBusExplorer.ViewModels
{
    public class ProcessMessageViewModel : ViewModelBase
    {
        private ReceiveMode _mode;
        private TimeSpan _timeout;

        public ProcessMessageViewModel()
        {
            Timeout = TimeSpan.FromSeconds(1);
            Mode = ReceiveMode.ReceiveAndDelete;
        }

        public ReceiveMode Mode
        {
            get { return _mode; }

            set { Set(() => Mode, ref _mode, value); }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }

            set { Set(() => Timeout, ref _timeout, value); }
        }
    }
}
