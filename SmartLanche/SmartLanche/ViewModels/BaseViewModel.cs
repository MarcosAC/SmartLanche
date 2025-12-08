using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace SmartLanche.ViewModels
{
    public class BaseViewModel : ObservableValidator
    {
        protected readonly IMessenger Messenger;

        public BaseViewModel(IMessenger messenger)
        {
            Messenger = messenger;
        }

        public BaseViewModel() : this(WeakReferenceMessenger.Default) { }
    }
}
