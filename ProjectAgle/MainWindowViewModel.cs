using ProjectAgle.NewPage.AglePage;
using ProjectAgle.NewPage.SelfCheckingPage;
using ProjectAgle.NewPage.TitlePage;
using ProjectAgle.Agle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;

namespace ProjectAgle
{
    class MainWindowViewModel
    {
        public Frame CurrentFrame
        {
            get { return currentFrame; }
        }

        private Agle.Agle MyAgle;
        private Frame currentFrame;
        private AglePageViewModel aglePageVM;
        private SelfCheckingPageViewModel selfCheckingPageVM;
        private TitlePageViewModel titlePageVM;
        private bool titlePageFinished = false;
        public MainWindowViewModel(Frame CurrentFrame)
        {
            this.currentFrame = CurrentFrame;

            // Initialize Agle as soon as possible
            MyAgle = Agle.Agle.GetAgleInstance;
            MyAgle.InitializeAgle();


            // Creating ViewModel for all related pages
            aglePageVM = new AglePageViewModel();

            selfCheckingPageVM = new SelfCheckingPageViewModel();

            titlePageVM = new TitlePageViewModel();
            titlePageVM.VideoFinishedEvent += this.OnTitlePageFinished;

            selfCheckingPageVM.AgleSystemStart += this.OnAgleSystemStart;

            // Ready navigating to title page, show starting movie
            TitlePage titlePage = new TitlePage();
            titlePage.InitializeTitlePage(this.titlePageVM);
            this.currentFrame.NavigationService.Navigate(titlePage);


           
        }
        void OnTitlePageFinished(object sender, object arg)
        {
            titlePageFinished = true;
            // Title Page Video Finished
            //Debug.Assert(MyAgle.isCheckingFinished == true);
            SelfCheckingPage selfCheckingPage = new SelfCheckingPage();
            selfCheckingPage.InitializeSelfChekingPage(this.selfCheckingPageVM);
            this.currentFrame.NavigationService.Navigate(selfCheckingPage);
        }

        void OnAgleSystemStart(object sender, object arg)
        {
            Debug.Assert(this.titlePageFinished == true);
            AglePage aglePage = new AglePage();
            this.aglePageVM = new AglePageViewModel();
            aglePage.InitializeAglePage(this.aglePageVM);
            this.currentFrame.NavigationService.Navigate(aglePage);
        }

    }
}
