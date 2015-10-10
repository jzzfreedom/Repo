using ProjectAgle.Agle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAgle.control.InfoPanel
{
    class InfoPanelViewModel : INotifyPropertyChanged
    {
        public string MODE
        {
            get { return this.mode; }
        }

        public string STATUS
        {
            get { return this.status; }
        }

        public string FPS
        {
            get { return this.fps; }
        }

        public string INFO
        {
            get { return this.info; }
        }

        public string VIEW
        {
            get { return this.view; }
        }
        private string mode = "Freeway";
        private string status = "OK";
        private string fps = "0";
        private string info = "OK";
        private string view = "Color";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void UpdateMode(AgleMode agleMode)
        {
            this.mode = agleMode.ToString();
            OnPropertyChanged("MODE");
        }

        internal void UpdateView(AgleView agleView)
        {
            this.view = agleView.ToString();
            OnPropertyChanged("VIEW");
        }

        internal void UpdateFps(int fps)
        {
            this.fps = fps.ToString();
            OnPropertyChanged("FPS");
        }

        internal void UpdateInfo(string info)
        {
            this.info = info;
            OnPropertyChanged("INFO");
        }

        internal void UpdateStatus(string status)
        {
            this.status = status;
            OnPropertyChanged("STATUS");
        }
    }
}
