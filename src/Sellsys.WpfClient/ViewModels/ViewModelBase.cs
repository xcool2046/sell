using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sellsys.WpfClient.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Virtual method for loading data when view becomes active
        public virtual async Task LoadDataAsync()
        {
            // Default implementation does nothing
            await Task.CompletedTask;
        }

        // Property to track if data has been loaded
        private bool _isDataLoaded = false;
        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            protected set => SetProperty(ref _isDataLoaded, value);
        }
    }
}