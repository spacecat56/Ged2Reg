using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CommonClassesLib
{
    [DataContract]
    public abstract class AbstractBindableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected static bool SuppressEvents { get; set; } = false;

        // note: with .NET 4.5 we could use [CallerMemberName]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (SuppressEvents) return; // this turns out to be not needed
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
