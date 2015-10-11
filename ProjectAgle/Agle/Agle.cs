using Microsoft.Kinect;
using ProjectAgle.Agle.AgleVisionControl;
using ProjectAgle.Agle.AgleVoiceControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectAgle.Agle
{
    enum AgleMode
    {
        Freeway,
        City,
        Parking,
        Reverse,
        Left,
        Right
    };

    enum AgleView
    {
        KinectColor,
        KinectInfrared,
        KinectDepth,
        MainFront,
        Forward,
        Rear,
        Left,
        Right
    };

    internal struct CheckingInfo
    {
        internal string infoName;
        internal bool success;
        internal bool isNecessary;

        internal CheckingInfo(string Name, bool Success, bool Necessary)
        {
            infoName = Name;
            success = Success;
            isNecessary = Necessary;
        }
    };
    class Agle
    {
        private static volatile Agle agleInstance;
        private static object syncRoot = new Object();
        private static object agleViewStateLock = new Object();

        public event EventHandler<object> ImageSourceUpdate;
        public event EventHandler<int> FPSUpdate;
        public event EventHandler<AgleView> AgleViewUpdate;
        public event EventHandler<string> AgleInfoUpdate;

        //Kinect related
        private KinectSensor kinectSensor = null;
        private AgleVoice agleVoice = null;
        private AgleColorFrame agleColorFrame= null;
        private AgleInfraredFrame agleInfraredFrame = null;
        private AgleDepthFrame agleDepthFrame = null;
        private double fps;

        //Kinect status related
        bool isKinectAvailable = true;
        bool isKinectColorAvailable = true;
        bool isKinectInfraredAvailable = true;
        bool isKinectDepthAvailable = true;
        bool isKinectVoiceAvailable = true;

        AgleView currentViewState = AgleView.KinectColor;
         
        // Checking status list
        private List<CheckingInfo> checkingInfoList;
        internal bool isCheckingFinished = false;
        private bool isAgleGoodToGo = true;

        private Stopwatch stopWatch = new Stopwatch();
        internal ImageSource CurrentImage
        {
            get
            {
                lock (agleViewStateLock)
                {
                    switch (this.currentViewState)
                    {
                        case AgleView.KinectColor:
                            return this.agleColorFrame.currentImage;
                        case AgleView.KinectInfrared:
                            return this.agleInfraredFrame.currentImage;
                        case AgleView.KinectDepth:
                            return this.agleDepthFrame.currentImage;
                        default:
                            return null;
                    }
                }
            }
        }

        internal AgleView CurrentViewState
        {
            get { return this.currentViewState; }
        }

        internal List<CheckingInfo> CheckingInfoList
        {
            get { return this.checkingInfoList; } 
        }

        internal bool AgleGoodToGo
        {
            get { return this.isAgleGoodToGo; }
        }
        private Agle()
        { }

        // Agle is a singleton 
        public static Agle GetAgleInstance
        {
            get
            {
                if (agleInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (agleInstance == null)
                        {
                            agleInstance = new Agle();
                        }
                    }
                }

                return agleInstance;
            }
        }

        internal void InitializeAgle()
        {

        }

        internal void CheckingModule()
        {
            this.checkingInfoList = new List<CheckingInfo>();
            this.stopWatch.Start();

            /*------------------Initialize Kinect related sensors------------------------*/
            //initialize kinect
            this.kinectSensor = KinectSensor.GetDefault();
            this.isKinectAvailable = this.kinectSensor == null ? false : true;
            CheckingInfo checkingInfoKinect = new CheckingInfo("Kinect", this.isKinectAvailable, true);
            checkingInfoList.Add(checkingInfoKinect);
            if (kinectSensor == null)
            {
                this.isAgleGoodToGo = false;
                this.isCheckingFinished = true;
                return;
            }

            //Checking infrared frame
            this.agleInfraredFrame = new AgleInfraredFrame(this.kinectSensor);
            this.isKinectInfraredAvailable = this.agleInfraredFrame.InitializeFrame();
            this.agleInfraredFrame.OnFrameArrived += this.AgleVisionFrameArrived;
            CheckingInfo checkingInfoKinectInfrared = new CheckingInfo("Kinect Infrared", this.isKinectColorAvailable, false);
            checkingInfoList.Add(checkingInfoKinectInfrared);

            //Checking color frame
            this.agleColorFrame = new AgleColorFrame(this.kinectSensor);
            this.isKinectColorAvailable = this.agleColorFrame.InitializeFrame();
            this.agleColorFrame.OnFrameArrived += this.AgleVisionFrameArrived;
            CheckingInfo checkingInfoKinectColor = new CheckingInfo("Kinect Color", this.isKinectColorAvailable, false);
            checkingInfoList.Add(checkingInfoKinectColor);

            //Checking depth frame
            this.agleDepthFrame = new AgleDepthFrame(this.kinectSensor);
            this.isKinectDepthAvailable = this.agleDepthFrame.InitializeFrame();
            this.agleDepthFrame.OnFrameArrived += this.AgleVisionFrameArrived;
            CheckingInfo checkingInfoKinectDepth = new CheckingInfo("Kinect Depth", this.isKinectDepthAvailable, false);
            checkingInfoList.Add(checkingInfoKinectDepth);



            //Open Kinect
            this.kinectSensor.IsAvailableChanged += this.OnKinectStatusChanged;
            this.kinectSensor.Open();

            // Kinect voice control
            agleVoice = new AgleVoice();
            isKinectVoiceAvailable = this.agleVoice.TryInitializeAgleVoice(kinectSensor);
            this.agleVoice.UpdateVoiceCommand += this.AgleChangeInfo;
            this.agleVoice.UpdateAgleViewByVoice += this.ChangeViewByVoice;
            CheckingInfo checkingInfoKinectVoice = new CheckingInfo("Kinect Voice", this.isKinectVoiceAvailable, false);
            checkingInfoList.Add(checkingInfoKinectVoice);


            isCheckingFinished = true;
            this.currentViewState = AgleView.KinectColor;
        }
        
        private void OnKinectStatusChanged(object sender, IsAvailableChangedEventArgs e)
        {

        }
      
        internal void AgleChangeView(AgleView nextViewState)
        {
            lock (agleViewStateLock)
            {
                this.currentViewState = nextViewState;
                AgleViewUpdate.Invoke(this, nextViewState);
            }
        }

        internal void AgleVisionFrameArrived(object sender, AgleView viewArrived)
        {
            if (viewArrived == currentViewState)
            {
                this.ImageSourceUpdate.Invoke(this, null);
                IAgleVision inComingFrame = sender as IAgleVision;
                this.fps = inComingFrame.FPS;
                FPSUpdate.Invoke(this, (int)this.fps);
            }
        }

        internal void AgleChangeInfo(object sender, string newInfo)
        {
            this.AgleInfoUpdate.Invoke(this, newInfo);
        }

        internal void ChangeViewByVoice(object sender, AgleView nextViewState)
        {
            this.AgleChangeView(nextViewState);
        }

        internal void OnWindowClosed()
        {
            this.agleInfraredFrame.TerminateFrame();
            this.agleColorFrame.TerminateFrame();
            this.agleDepthFrame.TerminateFrame();
            this.agleVoice.TerminateVoiceControl();
        }
    }
}
