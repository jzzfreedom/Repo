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

namespace ProjectAgle.NewPage.AglePage
{
    /// <summary>
    /// AglePage.xaml 的交互逻辑
    /// </summary>
    public partial class AglePage : Page
    {
        private AglePageViewModel aglePageVM;
        public AglePage()
        {
            InitializeComponent();
        }

        internal void InitializeAglePage(AglePageViewModel aglePageVM)
        {
            this.aglePageVM = aglePageVM;
            this.DataContext = this.aglePageVM;
            this.InfoPanel.DataContext = this.aglePageVM.infoPanelVM;
            this.MainMenuUC.DataContext = this.aglePageVM.mainMenuVM;
        }

    }
}
