using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using ProjectAgle.NewPage.TitlePage;

namespace ProjectAgle.NewPage.TitlePage
{
    /// <summary>
    /// TitlePage.xaml 的交互逻辑
    /// </summary>
    public partial class TitlePage : Page
    {
        private TitlePageViewModel titlePageVM;
        public TitlePage()
        {
            InitializeComponent();
            //this.titlePageVM = TitlePageVM;
        }

        internal void InitializeTitlePage(TitlePageViewModel TitlePageVM)
        {
            this.titlePageVM = TitlePageVM;
        }

        void TitleVideoFinished(object sender, RoutedEventArgs arg)
        {
            //VideoFinished.Invoke(this, null);
            this.titlePageVM.VideoFinished();
        }
        
    }
}
