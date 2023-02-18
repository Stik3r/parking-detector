using parking_detector.Classes.Parking;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace parking_detector.Controls
{
    /// <summary>
    /// Логика взаимодействия для DataControl.xaml
    /// </summary>
    public partial class DataControl : UserControl
    {
        private DispatcherTimer timer;
        public DataControl()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += UpdateParkingSpace;
            timer.Start();
        }

        private void UpdateParkingSpace(object sender, object e)
        {
            parkingCount.Text = ParkingController.Count().ToString();
            parkingCountTaken.Text = ParkingController.CountTaken().ToString();
        }
    }
}
