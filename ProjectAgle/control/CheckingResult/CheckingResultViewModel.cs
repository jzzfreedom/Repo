using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectAgle.Agle;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectAgle.control.CheckingResult
{
    class CheckingItem
    {
        
        private string name;
        private bool isSuccess;
        private bool isNecessary;
        private ImageSource iconUrl;

        public string Name
        {
            get { return this.name; }
        }
        public string IsSuccess
        {
            get { if (this.isSuccess) return "Success"; else return "Fail"; }
        }
        public string IsNecessary
        {
            get { if (this.isNecessary) return "Yes"; else return "No"; }
        }

        public ImageSource ImageIcon
        {
            get { return this.iconUrl; }
        }
        internal CheckingItem(string name, bool isSuccess, bool isNecessary)
        {
            this.name = name;
            this.isSuccess = isSuccess;
            this.isNecessary = isNecessary;

            if (isNecessary && !isSuccess)
            {
                this.iconUrl = new BitmapImage(new Uri(@"\Asset\wrong.png", UriKind.Relative));
            }
            else
            {
                this.iconUrl = new BitmapImage(new Uri(@"\Asset\right.png", UriKind.Relative));
            }
        } 
    }
    class CheckingResultViewModel
    {
        private bool isSystemGoodToGo;
        private List<CheckingItem> item;

        public bool SystemGoodToGo
        {
            get { return this.isSystemGoodToGo; }
        }

        public string DecisionString
        {
            get { return isSystemGoodToGo ? "Agle All System Green!" : "Agle Self Checking Failed!"; }
        }

        public List<CheckingItem> Item
        {
            get { return this.item; }
        }

        public Brush DecisionColor
        {
            get { return isSystemGoodToGo? new SolidColorBrush(Colors.Green): new SolidColorBrush(Colors.Red); }
        }

        internal CheckingResultViewModel(List<CheckingInfo> InfoList)
        {
            isSystemGoodToGo = true;
            item = new List<CheckingItem>();

            foreach (CheckingInfo info in InfoList)
            {
                if (info.isNecessary == true && info.success == false)
                {
                    isSystemGoodToGo = false;
                }
                CheckingItem checkingItem = new CheckingItem(info.infoName, info.success, info.isNecessary);
                this.item.Add(checkingItem);
            }
        }
    }
}
