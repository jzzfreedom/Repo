using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ProjectAgle
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new MainWindowViewModel(this.CurrentFrame);
        }

        void onClick(object sender, RoutedEventArgs arg)
        {
            //NavigationService ns = NavigationService.GetNavigationService(this);
            //ns.Navigate(typeof(ProjectAgle.NewPage.SelfCheckingPage.SelfCheckingPage));
            //StartFrame.Navigate(new NewPage.SelfCheckingPage.SelfCheckingPage());
            //var newWindow = new ProjectAgle.NewPage.SelfCheckingWindow.SelfCheckingWindow();
            //newWindow.Show();
            //this.Close();
        }

        

        void OnSelfCheckingPageFinished(object sender, object arg)
        {

        }

        void OnWindowClosed(object sender, CancelEventArgs e)
        {
            this.mainWindowViewModel.OnWindowClosedHandler();
        }
    }
}
