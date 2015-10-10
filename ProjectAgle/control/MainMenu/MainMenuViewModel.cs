using ProjectAgle.Agle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectAgle.control.MainMenu
{
    class MainMenuViewModel
    {
        private ICommand kinectColorSelected;
        private ICommand kinectInfraredSelected;
        private ICommand kinectDepthSelected;
        internal event EventHandler<AgleView> ChangeView;
        public MainMenuViewModel()
        {
            this.kinectColorSelected = new GeneralCommand(param => this.OnKinectColorSelected());
            this.kinectInfraredSelected = new GeneralCommand(param => this.OnKinectInfraredSelected());
            this.kinectDepthSelected = new GeneralCommand(param => this.OnKinectDepthSelected());
        }
        public ICommand KinectColorSelected
        {
            get { return this.kinectColorSelected; }
        }

        public ICommand KinectInfraredSelected
        {
            get { return this.kinectInfraredSelected; }
        }

        public ICommand KinectDepthSelected
        {
            get { return this.kinectDepthSelected; }
        }
        private void OnKinectColorSelected()
        {
            this.ChangeView.Invoke(this, AgleView.KinectColor);
        }

        private void OnKinectInfraredSelected()
        {
            this.ChangeView.Invoke(this, AgleView.KinectInfrared);
        }

        private void OnKinectDepthSelected()
        {
            this.ChangeView.Invoke(this, AgleView.KinectDepth);
        }
    }
}
