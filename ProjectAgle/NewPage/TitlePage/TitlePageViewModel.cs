using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAgle.NewPage.TitlePage
{
    class TitlePageViewModel
    {
        public event EventHandler<object> VideoFinishedEvent;
        public TitlePageViewModel()
        { }

        internal void VideoFinished()
        {
            this.VideoFinishedEvent.Invoke(this, null);
        }
    }
}
