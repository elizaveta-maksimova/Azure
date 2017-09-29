using GalaSoft.MvvmLight;

namespace ServiceBusExplorer.ViewModels
{
    public class PropertyViewModel : ViewModelBase
    {
        private string _name;
        private string _value;
        
        public string Name
        {
            get { return _name; }

            set { Set(() => Name, ref _name, value); }
        }

        public string Value
        {
            get { return _value; }

            set { Set(() => Value, ref _value, value); }
        }
    }
}
