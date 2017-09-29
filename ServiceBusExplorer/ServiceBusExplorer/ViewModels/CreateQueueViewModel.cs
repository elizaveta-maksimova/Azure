using GalaSoft.MvvmLight;

namespace ServiceBusExplorer.ViewModels
{
    public class CreateQueueViewModel : ViewModelBase
    {
        private string _name;
        private int _size;
        private bool _enablePartitioning;

        public CreateQueueViewModel()
        {
            Size = 1;
        }

        public string Name
        {
            get { return _name; }

            set { Set(() => Name, ref _name, value); }
        }

        public int Size
        {
            get { return _size; }

            set { Set(() => Size, ref _size, value); }
        }

        public bool EnablePartitioning
        {
            get { return _enablePartitioning; }

            set { Set(() => EnablePartitioning, ref _enablePartitioning, value); }
        }
    }
}
