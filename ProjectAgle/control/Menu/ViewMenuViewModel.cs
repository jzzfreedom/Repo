using ProjectAgle.Agle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectAgle.control.Menu
{
    class ViewMenuItem
    {
        private string menuText;
        private AgleView agleView;
        private ICommand menuCommand = null;
        public event EventHandler<AgleView> ViewMenuPressed;
        public string MenuText
        {
            get { return this.menuText; }
        }

        public ICommand MenuCommand
        {
            get { return this.menuCommand; }
        }

        public ViewMenuItem(AgleView agleView)
        {
            this.agleView = agleView;
            this.menuText = agleView.ToString();
        }
    }
    class ViewMenuViewModel
    {
        public List<ViewMenuItem> MenuItem
        {
            get { return this.viewMenuList; }
        }

        private List<ViewMenuItem> viewMenuList = null;
        public ViewMenuViewModel()
        {
            this.viewMenuList = new List<ViewMenuItem>();
            ViewMenuItem colorViewMenuItem = new ViewMenuItem(AgleView.KinectColor);
            this.viewMenuList.Add(colorViewMenuItem);
            ViewMenuItem infraredViewMenuItem = new ViewMenuItem(AgleView.KinectInfrared);
            this.viewMenuList.Add(infraredViewMenuItem);
        }
    }
}
