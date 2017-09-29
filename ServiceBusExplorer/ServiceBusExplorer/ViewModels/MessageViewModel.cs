using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ServiceBusExplorer.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private string _body;

        public MessageViewModel()
        {
            Properties = new ObservableCollection<PropertyViewModel>();

            ClearCommand = new RelayCommand(Clear, CanClear);

            if (IsInDesignMode)
            {
                Properties.Add(new PropertyViewModel {Name = "Property1", Value = "Value 1"});
                Properties.Add(new PropertyViewModel {Name = "Property2", Value = "Value 2"});
            }
        }

        public ObservableCollection<PropertyViewModel> Properties { get; private set; }

        public ICommand ClearCommand { get; private set; }

        public string Body
        {
            get { return _body; }

            set { Set(() => Body, ref _body, value); }
        }

        private bool CanClear()
        {
            return !string.IsNullOrEmpty(Body) || Properties.Any();
        }

        private void Clear()
        {
            Properties.Clear();
            Body = string.Empty;
        }
    }
}
