using ProjectAgle.Agle;
using ProjectAgle.control.ControlPanel;
using ProjectAgle.control.InfoPanel;
using ProjectAgle.control.MainMenu;
using ProjectAgle.control.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProjectAgle.NewPage.AglePage
{
    class AglePageViewModel : INotifyPropertyChanged
    {
        public ImageSource ImageSource
        {
            get { return this.MyAgle.CurrentImage; }
        }

        internal InfoPanelViewModel infoPanelVM;
        internal MainMenuViewModel mainMenuVM;
        //internal ControlPanelViewModel controlPanelVM;
        //internal ViewMenuViewModel viewMenuVM;


        private Agle.Agle MyAgle;
        public AglePageViewModel()
        {
            this.MyAgle = Agle.Agle.GetAgleInstance;
            /*------------------------wire Agle related event------------------------*/
            this.MyAgle.ImageSourceUpdate += this.OnImageSourceUpdated;
            this.MyAgle.FPSUpdate += this.OnFPSUpdated;
            this.MyAgle.AgleViewUpdate += this.OnAgleViewUpdated;

            /*-------------------------create all kinds of view model--------------------*/
            this.infoPanelVM = new InfoPanelViewModel();
            this.mainMenuVM = new MainMenuViewModel();
            this.mainMenuVM.ChangeView += this.OnViewChanged;
            //this.controlPanelVM = new ControlPanelViewModel();
            //this.viewMenuVM = new ViewMenuViewModel();
        }


        /*-------------------------Image Source related-----------------------------*/
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnImageSourceUpdated(object sender, object e)
        {
            OnPropertyChanged("ImageSource");
        }


        /*------------------------------InfoPanel related------------------------------------*/
        private void OnFPSUpdated(object sender, int fps)
        {
            this.infoPanelVM.UpdateFps(fps);
        }

        private void OnAgleViewUpdated(object sender, AgleView nextViewState)
        {
            this.infoPanelVM.UpdateView(nextViewState);
        }
        /*------------------------------ControlPanel related------------------------------------*/
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnViewChanged(object sender, AgleView newViewState)
        {
            this.MyAgle.AgleChangeView(newViewState);
        }

    }
}
