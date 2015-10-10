using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectAgle.control.ControlPanel
{
    class ControlPanelViewModel
    {
        public event EventHandler<object> ViewMenuStatus;
        public event EventHandler<object> ModeMenuStatus;
        public event EventHandler<object> ControlMenuStatus;

        private double menuWidth = 300;
        private ICommand controlPressed;
        private ICommand modePressed;
        private ICommand viewPressed;

        public ControlPanelViewModel()
        {
            this.controlPressed = new GeneralCommand(param => this.OnControlMenuPressed());
            this.modePressed = new GeneralCommand(param => this.OnModeMenuPressed());
            this.viewPressed = new GeneralCommand(param => this.OnViewMenuPressed());
        }
        public double MenuWidth
        {
            get { return this.menuWidth; }
        }

        public ICommand ControlPressed
        {
            get { return this.controlPressed; }
        }

        public ICommand ModePressed
        {
            get { return this.modePressed; }
        }

        public ICommand ViewPressed
        {
            get { return this.viewPressed; }
        }

        private void OnControlMenuPressed()
        {
            //ControlMenuStatus.Invoke(this, null);
        }

        private void OnModeMenuPressed()
        {
            //ModeMenuStatus.Invoke(this, null);
        }

        private void OnViewMenuPressed()
        {
            ViewMenuStatus.Invoke(this, null);
        }
    }
}
