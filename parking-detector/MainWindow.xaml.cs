using parking_detector.Controls;
using System.Windows;

namespace parking_detector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ButtonsControl bs = (ButtonsControl)FindName("buttons");
            bs.videoControl = (VideoControl)FindName("videoPlayer");
        }

    }
}
