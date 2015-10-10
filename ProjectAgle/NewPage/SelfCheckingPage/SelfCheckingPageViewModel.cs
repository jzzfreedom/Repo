using ProjectAgle.Agle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAgle.NewPage.SelfCheckingPage
{
    class SelfCheckingPageViewModel
    {
        private Agle.Agle MyAgle;
        public event EventHandler<object> AgleSystemStart;

        internal List<CheckingInfo> CheckingInfoList
        {
            get { return this.MyAgle.CheckingInfoList; }
        }
        public SelfCheckingPageViewModel()
        { }

        public void PopulateCheckingList()
        {
            MyAgle = Agle.Agle.GetAgleInstance;

            // Checking Modules needed
            MyAgle.CheckingModule();
        }

        internal void OnSelfCheckingFinished()
        {
            if (this.MyAgle.AgleGoodToGo)
            {
                AgleSystemStart.Invoke(this, null);
            }
        }
    }
}
