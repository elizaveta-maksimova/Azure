using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace ServiceBusExplorer.ViewModels
{
    public class TopicViewModel : ViewModelBase
    {
        private string _name;

        public TopicViewModel()
        {
            ViewModels = new ObservableCollection<SubscriptionViewModel>();
        }

        public string Name
        {
            get { return _name; }

            set { Set(() => Name, ref _name, value); }
        }

        public ObservableCollection<SubscriptionViewModel> ViewModels { get; private set; }
    }
}
