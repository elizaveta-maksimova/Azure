using GalaSoft.MvvmLight;

namespace ServiceBusExplorer.ViewModels
{
    public class CreateSubscriptionViewModel : ViewModelBase
    {
        private string _name;
        private string _filter;

        public string Name
        {
            get { return _name; }

            set { Set(() => Name, ref _name, value); }
        }

        public string Filter
        {
            get { return _filter; }

            set { Set(() => Filter, ref _filter, value); }
        }
    }
}
