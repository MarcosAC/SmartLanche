using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace SmartLanche.ViewModels
{
    public partial class BaseViewModel : ObservableValidator
    {
        protected readonly IMessenger Messenger;

        [ObservableProperty]
        private bool _isBusy;

        public BaseViewModel(IMessenger messenger)
        {
            Messenger = messenger;
        }

        public BaseViewModel() : this(WeakReferenceMessenger.Default) { }
    }
}
