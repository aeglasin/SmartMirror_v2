using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartMirror.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void SetProperty<T>(ref T member, T value,[CallerMemberName] String propertyName = null)
        {
            if (object.Equals(member, value))
                return;
            member = value;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
