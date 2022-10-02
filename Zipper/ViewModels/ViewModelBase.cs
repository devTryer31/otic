using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zipper.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChaged(string? propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public bool Set<T>(ref T target, T data, [CallerMemberName] string? propName =  null)
        {
            if (object.ReferenceEquals(target, data))
                return false;

            target = data;
            OnPropertyChaged(propName);
            return true;
        }
    }
}
