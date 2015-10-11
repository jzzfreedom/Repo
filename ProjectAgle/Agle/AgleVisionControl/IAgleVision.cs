using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProjectAgle.Agle.AgleVisionControl
{
    interface IAgleVision
    {
        double FPS
        {
            get;
        }

        ImageSource currentImage
        {
            get;
        }
        bool InitializeFrame();
        void TerminateFrame();

        event EventHandler<AgleView> OnFrameArrived;

    }
}
