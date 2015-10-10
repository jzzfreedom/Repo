using ProjectAgle.control.CheckingResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ProjectAgle.NewPage.SelfCheckingPage
{
    /// <summary>
    /// SelfCheckingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SelfCheckingPage : Page
    {
        internal SelfCheckingPageViewModel selfCheckingPageVM;
        internal CheckingResultViewModel checkingResultVM;
        private int scrollTimes = 0;
        private System.Windows.Threading.DispatcherTimer Timer;
        private System.Windows.Threading.DispatcherTimer Tr;
        private double offsetDelta;
        public SelfCheckingPage()
        {
            InitializeComponent();
            //this.On// += this.OnNavigatedToCurrentPage;
        }

        internal void InitializeSelfChekingPage(SelfCheckingPageViewModel SelfCheckingPageVM)
        {
            this.selfCheckingPageVM = SelfCheckingPageVM;
            this.CheckingPageBackground.Source = new BitmapImage(new Uri(@"\Asset\asurada .jpg", UriKind.Relative));
            //MyAgle = Agle.Agle.GetAgleInstance;

            // Checking Modules needed
            //MyAgle.CheckingModule();
            this.selfCheckingPageVM.PopulateCheckingList();
            this.checkingResultVM = new CheckingResultViewModel(this.selfCheckingPageVM.CheckingInfoList);
            this.AgleCheckingList.DataContext = this.checkingResultVM;
            this.FinalDecision.DataContext = this.checkingResultVM;

        }

        private void OnLoaded(object sender, EventArgs e)
        {
            var titleHeight = this.SelfCheckingPageTitle.ActualHeight;
            var firstRowHeight = this.FirstRow.ActualHeight;
            var decisionHeight = this.FinalDecision.ActualHeight;
            this.CheckingListScrollView.Height = System.Windows.SystemParameters.PrimaryScreenHeight - titleHeight - firstRowHeight - decisionHeight - 250;
            this.offsetDelta = CheckingListScrollView.ActualHeight - CheckingListScrollView.Height;

            // Setting timer
            Tr = new System.Windows.Threading.DispatcherTimer();
            Tr.Tick += new EventHandler(AutoScroll);
            Tr.Interval = new TimeSpan(0, 0, 1);
            Tr.Start();
        }

        private void AutoScroll(object sender, EventArgs e)
        {
            Tr.Stop();
            
            // Has invisible checking result. Do auto scroll
            if (this.offsetDelta > 0)
            {
                scrollTimes = (int)offsetDelta / 50 + 1;
                Timer = new System.Windows.Threading.DispatcherTimer();
                Timer.Tick += new EventHandler(Timer_Tick);
                Timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                Timer.Start();
            }
            else
            {
                CheckingFinished();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (scrollTimes == 0)
            {               
                this.Timer.Stop();
                CheckingFinished();
            }
            else
            {
                this.CheckingListScrollView.ScrollToVerticalOffset(CheckingListScrollView.VerticalOffset+50);
                scrollTimes--;
            }
        }

        private async void CheckingFinished()
        {
            this.FinalDecision.Opacity = 1;
            SystemSounds.Asterisk.Play();
            await Task.Delay(3000);
            this.selfCheckingPageVM.OnSelfCheckingFinished();
        }
    }
}
