using Microsoft.Kinect;
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
        private const float InfraredSourceScale = 0.75f;
        private const float InfraredOutputValueMinimum = 0.01f;
        private const float InfraredOutputValueMaximum = 1.0f;
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;
        private const int MapDepthToByte = 8000 / 256;

        private static volatile Agle agleInstance;
        private static object syncRoot = new Object();
        private static object agleViewStateLock = new Object();

        public event EventHandler<object> ImageSourceUpdate;
        public event EventHandler<int> FPSUpdate;
        public event EventHandler<AgleView> AgleViewUpdate;

        //Kinect related
        private KinectSensor kinectSensor = null;
        private ColorFrameReader colorFrameReader = null;
        private FrameDescription colorFrameDescription = null;
        private InfraredFrameReader infraredFrameReader = null;
        private FrameDescription infraredFrameDescription = null;
        private DepthFrameReader depthFrameReader = null;
        private FrameDescription depthFrameDescription = null;

        //Kinect status related
        bool isKinectAvailable = true;
        bool isKinectColorAvailable = true;
        bool isKinectInfraredAvailable = true;
        bool isKinectDepthAvailable = true;

        AgleView currentViewState = AgleView.KinectColor;
         
        // Checking status list
        private List<CheckingInfo> checkingInfoList;
        internal bool isCheckingFinished = false;
        private bool isAgleGoodToGo = true;

        private WriteableBitmap colorBitmap = null;
        private WriteableBitmap infraredBitmap = null;
        private WriteableBitmap depthBitmap = null;
           
        private byte[] depthPixels = null;

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
                            return this.colorBitmap;
                        case AgleView.KinectInfrared:
                            return this.infraredBitmap;
                        case AgleView.KinectDepth:
                            return this.depthBitmap;
                        default:
                            return null;
                    }
                }
            }
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
            //this.currentBitmap = new WriteableBitmap()
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
            InitializeKinectInfraredFrame();
            CheckingInfo checkingInfoKinectInfrared = new CheckingInfo("Kinect Infrared", this.isKinectColorAvailable, false);
            checkingInfoList.Add(checkingInfoKinectInfrared);
            
            //Checking color frame
            InitializeKinectColorFrame();
            CheckingInfo checkingInfoKinectColor = new CheckingInfo("Kinect Color", this.isKinectColorAvailable, false);
            checkingInfoList.Add(checkingInfoKinectColor);

            //Checking depth frame
            InitializeKinectDepthFrame();
            CheckingInfo checkingInfoKinectDepth = new CheckingInfo("Kinect Depth", this.isKinectDepthAvailable, false);
            checkingInfoList.Add(checkingInfoKinectDepth);



            //Open Kinect
            this.kinectSensor.IsAvailableChanged += this.OnKinectStatusChanged;
            this.kinectSensor.Open();

            isCheckingFinished = true;
            //this.currentViewState = AgleView.KinectInfrared;
            this.currentViewState = AgleView.KinectInfrared;
        }

        private void InitializeKinectColorFrame()
        {
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.isKinectColorAvailable = this.colorFrameReader == null ? false : true;
            if (this.colorFrameReader!=null)
            {
                // wire color frame arrive event
                this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

                this.colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            }
        }

        private void TerminateKinectColorFrame()
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
                this.colorBitmap = null;
            }
        }

        private void InitializeKinectDepthFrame()
        {
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
            this.isKinectDepthAvailable = this.depthFrameReader == null ? false : true;
            if (this.depthFrameReader != null)
            {
                this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;
                this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
                this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
                this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);
            }
        }

        private void TerminateKinectDepthFrame()
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }
        }
        private void InitializeKinectInfraredFrame()
        {
            this.infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();
            this.isKinectInfraredAvailable = infraredFrameReader == null ? false : true;
            if (this.infraredFrameReader != null)
            {
                this.infraredFrameReader.FrameArrived += this.Reader_InfraredFrameArrived;
                this.infraredFrameDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;
                this.infraredBitmap = new WriteableBitmap(this.infraredFrameDescription.Width, this.infraredFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);
            }

        }

        private void TerminateKinectInfraredFrame()
        {
            if (this.infraredFrameReader != null)
            {
                this.infraredFrameReader.Dispose();
                this.infraredFrameReader = null;
                this.infraredBitmap = null;
            }
        }

        private void OnKinectStatusChanged(object sender, IsAvailableChangedEventArgs e)
        {

        }

        // Event Handler when a new color frame arrives
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }
                        this.ImageSourceUpdate.Invoke(this, null);

                        //calculate FPS
                        double fps = 1.0/colorFrame.ColorCameraSettings.FrameInterval.TotalSeconds;
                        //this.FPSUpdate.Invoke(this, (int)fps);
                        //this.currentBitmap = this.colorBitmap;
                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        // Event Handler when a new infrared frame arrives
        private void Reader_InfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    // the fastest way to process the infrared frame data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer infraredBuffer = infraredFrame.LockImageBuffer())
                    {
                        // verify data and write the new infrared frame data to the display bitmap
                        var t= this.infraredFrameDescription;
                        var tt = this.infraredBitmap;
                        if (((this.infraredFrameDescription.Width * this.infraredFrameDescription.Height) == (infraredBuffer.Size / this.infraredFrameDescription.BytesPerPixel)) &&
                            (this.infraredFrameDescription.Width == this.infraredBitmap.PixelWidth) && (this.infraredFrameDescription.Height == this.infraredBitmap.PixelHeight))
                        {
                            this.ProcessInfraredFrameData(infraredBuffer.UnderlyingBuffer, infraredBuffer.Size);
                        }
                    }
                }
            }
        }

        private unsafe void ProcessInfraredFrameData(IntPtr infraredFrameData, uint infraredFrameDataSize)
        {
            // infrared frame data is a 16 bit value
            ushort* frameData = (ushort*)infraredFrameData;

            // lock the target bitmap
            this.infraredBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*)this.infraredBitmap.BackBuffer;

            // process the infrared data
            for (int i = 0; i < (int)(infraredFrameDataSize / this.infraredFrameDescription.BytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                backBuffer[i] = Math.Min(InfraredOutputValueMaximum, (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            this.infraredBitmap.AddDirtyRect(new Int32Rect(0, 0, this.infraredBitmap.PixelWidth, this.infraredBitmap.PixelHeight));

            //this.currentBitmap = this.infraredBitmap;
            // unlock the bitmap
            this.infraredBitmap.Unlock();


        }

        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance

                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }

        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
            this.stopWatch.Stop();
            var mSecond = this.stopWatch.ElapsedMilliseconds;
            double fps = 1000000.0 / (mSecond + 1);
            this.FPSUpdate.Invoke(this, (int)fps);
            this.stopWatch.Start();
        }
        internal void AgleChangeView(AgleView nextViewState)
        {
            lock (agleViewStateLock)
            {
                this.currentViewState = nextViewState;
                AgleViewUpdate.Invoke(this, nextViewState);
            }
        }
    }
}
