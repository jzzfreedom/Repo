﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectAgle
{
    class GeneralCommand :ICommand
    {

        readonly Action<object> execute;

        public GeneralCommand(Action<object> execute)
        {
            this.execute = execute;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
