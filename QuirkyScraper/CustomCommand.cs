using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuirkyScraper
{
    public class CustomCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Func<object, bool> CanExecuteAction { get; set; }
        public Action<object> ExecuteAction { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteAction != null)
                return CanExecuteAction(parameter);
            return false;
        }

        public void Execute(object parameter)
        {
            if (ExecuteAction != null)
                ExecuteAction(parameter);
        }
    }
}
