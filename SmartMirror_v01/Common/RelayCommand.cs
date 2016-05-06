using System;
using System.Windows.Input;

namespace SmartMirror.Common
{
    public class RelayCommand : ICommand
    {
        private Action targerExecuteMethod;
        private Func<bool> targetCanExecuteMethod;

        public RelayCommand(Action executeMethod)
        {
            targerExecuteMethod = executeMethod;
        }

        public RelayCommand(Action executeMethode, Func<bool> canExecuteMethod)
        {
            targerExecuteMethod = executeMethode;
            targetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (targetCanExecuteMethod != null)
                return targetCanExecuteMethod();
            else if (targerExecuteMethod != null)
                return true;
            else
                return false;
        }

        public event System.EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            if (targerExecuteMethod != null)
                targerExecuteMethod();
        }
    }
}
