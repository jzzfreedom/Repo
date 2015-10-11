using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectAgle.Agle.AgleVisionControl
{
    class AgleColorFrame : IAgleVision
    {
        private KinectSensor kinectSensor = null;
        private WriteableBitmap colorBitmap = null;
        private ColorFrameReader colorFrameReader = null;
        private FrameDescription colorFrameDescription = null;
        private double fps = 0.0;

        // IAgleVision interface implementation
        public event EventHandler<AgleView> OnFrameArrived;
        public double FPS
        {
            get { return this.fps; }
        }

        public ImageSource currentImage
        {
            get { return this.colorBitmap; }
        }

        public bool InitializeFrame()
        {
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            if (this.colorFrameReader != null)
            {
                // wire color frame arrive event
                this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

                this.colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
                return true;

            }
            return false;
        }

        public void TerminateFrame()
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
                this.colorBitmap = null;
            }
        }

        public AgleColorFrame(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;
        }

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
                        //calculate FPS
                        this.fps = 1.0 / colorFrame.ColorCameraSettings.FrameInterval.TotalSeconds;
                        this.OnFrameArrived.Invoke(this, AgleView.KinectColor);
                        this.colorBitmap.Unlock();
                    }
                }
            }
        }
    }
}
