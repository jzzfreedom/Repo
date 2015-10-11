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

namespace ProjectAgle.Agle.AgleVisionControl
{
    class AgleInfraredFrame : IAgleVision
    {
        private const float InfraredSourceScale = 0.75f;
        private const float InfraredOutputValueMinimum = 0.01f;
        private const float InfraredOutputValueMaximum = 1.0f;
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        private KinectSensor kinectSensor = null;
        private InfraredFrameReader infraredFrameReader = null;
        private FrameDescription infraredFrameDescription = null;
        private WriteableBitmap infraredBitmap = null;

        private Stopwatch stopwatch = new Stopwatch();

        private double fps = 48.0;

        // IAgleVision interface implementation
        public event EventHandler<AgleView> OnFrameArrived;
        public double FPS
        {
            get { return this.fps; }
        }

        public ImageSource currentImage
        {
            get { return this.infraredBitmap; }
        }

        public bool InitializeFrame()
        {
            this.infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();
            if (this.infraredFrameReader != null)
            {
                this.infraredFrameReader.FrameArrived += this.Reader_InfraredFrameArrived;
                this.infraredFrameDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;
                this.infraredBitmap = new WriteableBitmap(this.infraredFrameDescription.Width, this.infraredFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);
                this.stopwatch.Start();
                return true;
            }
            return false;
        }

        public void TerminateFrame()
        {
            if (this.infraredFrameReader != null)
            {
                this.infraredFrameReader.Dispose();
                this.infraredFrameReader = null;
                this.infraredBitmap = null;
            }
        }

        public AgleInfraredFrame(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;
        }

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
                        var t = this.infraredFrameDescription;
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

            this.stopwatch.Stop();
            this.fps = 1000/stopwatch.ElapsedMilliseconds;
            this.stopwatch.Reset();
            this.OnFrameArrived(this, AgleView.KinectInfrared);
            this.stopwatch.Start();
            // unlock the bitmap
            this.infraredBitmap.Unlock();
        }

    }
}
