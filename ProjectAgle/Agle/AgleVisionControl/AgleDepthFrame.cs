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
    class AgleDepthFrame : IAgleVision
    {
        private KinectSensor kinectSensor = null;
        private DepthFrameReader depthFrameReader = null;
        private FrameDescription depthFrameDescription = null;
        private const int MapDepthToByte = 8000 / 256;
        private byte[] depthPixels = null;

        private Stopwatch stopwatch = new Stopwatch();
        private WriteableBitmap depthBitmap = null;
        private double fps = 26.0;
        // IAgleVision interface implementation
        public event EventHandler<AgleView> OnFrameArrived;
        public double FPS
        {
            get { return this.fps; }
        }

        public ImageSource currentImage
        {
            get { return this.depthBitmap; }
        }

        public bool InitializeFrame()
        {
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
            if (this.depthFrameReader != null)
            {
                this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;
                this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
                this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
                this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);
                this.stopwatch.Start();
                return true;
            }
            return false;
        }

        public void TerminateFrame()
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }
        }

        public AgleDepthFrame(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;
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
            this.stopwatch.Stop();
            this.fps = 1000 / stopwatch.ElapsedMilliseconds;
            this.stopwatch.Reset();
            this.OnFrameArrived(this, AgleView.KinectDepth);
            this.stopwatch.Start();         
        }
    }
}
