using MyClock.Properties;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyClock
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Using a DependencyProperty as the backing store for Hours. This
        // enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register("Hours", typeof(int), typeof(MainWindow), new PropertyMetadata(12));

        // Using a DependencyProperty as the backing store for Milliseconds.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register("Milliseconds", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for Minutes. This
        // enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for Seconds. This
        // enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register("Seconds", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for Time. This
        // enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));

        private Timer _timer;

        public MainWindow()
        {
            InitializeComponent();
            this.DateMenuItem.IsChecked = Settings.Default.DateVisibility;
            this.DigitalClockMenuItem.IsChecked = Settings.Default.DigitalClockVisibility;
            this.AnalogClockMenuItem.IsChecked = Settings.Default.AnalogClockVisibility;
            this.analogClock.Visibility = Settings.Default.AnalogClockVisibility ? Visibility.Visible : Visibility.Collapsed;
            this.digitalClock.Visibility = Settings.Default.DigitalClockVisibility ? Visibility.Visible : Visibility.Collapsed;
            ClockEnablement();
            this.date.Visibility = Settings.Default.DateVisibility ? Visibility.Visible : Visibility.Collapsed;
            this.Topmost = Settings.Default.Topmost;
            ontop1.IsChecked = ontop2.IsChecked = this.Topmost;
            this.Left = Settings.Default.Left;
            this.Top = Settings.Default.Top;
            TimeUpdate(DateTime.Now);
            _timer = new Timer(Ticker, null, 100, 100);
        }

        public int Hours
        {
            get { return (int)GetValue(HoursProperty); }
            set { SetValue(HoursProperty, value); }
        }

        public int Milliseconds
        {
            get { return (int)GetValue(MillisecondsProperty); }
            set { SetValue(MillisecondsProperty, value); }
        }

        public int Minutes
        {
            get { return (int)GetValue(MinutesProperty); }
            set { SetValue(MinutesProperty, value); }
        }

        public int Seconds
        {
            get { return (int)GetValue(SecondsProperty); }
            set { SetValue(SecondsProperty, value); }
        }

        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Left = this.Left;
            Settings.Default.Top = this.Top;
            Settings.Default.Topmost = this.Topmost;
            Settings.Default.AnalogClockVisibility = analogClock.Visibility == Visibility.Visible;
            Settings.Default.DigitalClockVisibility = digitalClock.Visibility == Visibility.Visible;
            Settings.Default.DateVisibility = date.Visibility == Visibility.Visible;
            Settings.Default.Save();

            base.OnClosing(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            Rectangle hand;

            switch (e.Property.Name)
            {
                case "Minutes":
                    hand = this.HoursHand;
                    MoveHand(hand, this.Hours % 12 + this.Minutes / 60.0, 12);
                    break;

                case "Seconds":
                    hand = this.MinutesHand;
                    MoveHand(hand, this.Minutes + this.Seconds / 60.0, 60);
                    break;

                case "Milliseconds":
                    hand = this.SecondsHand;
                    MoveHand(hand, this.Seconds + this.Milliseconds / 1000.0, 60);
                    break;
            }

            base.OnPropertyChanged(e);
        }

        private void AnalogClockMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            this.analogClock.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ClockEnablement();
        }

        private void ClockEnablement()
        {
            this.AnalogClockMenuItem.IsEnabled = this.DigitalClockMenuItem.IsChecked;
            this.DigitalClockMenuItem.IsEnabled = this.AnalogClockMenuItem.IsChecked;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DateMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            this.date.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DigitalClockMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            this.digitalClock.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ClockEnablement();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MoveHand(Rectangle hand, double time, int maxtime)
        {
            var angle = 360.0 / maxtime * time;
            var rotate = new RotateTransform(angle, hand.RenderTransformOrigin.X, hand.RenderTransformOrigin.Y);
            hand.RenderTransform = rotate;
        }

        private void OnTopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            ontop1.IsChecked = ontop2.IsChecked = this.Topmost;
        }

        private void Ticker(object state)
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() => { TimeUpdate(DateTime.Now); }));
        }

        private void TimeUpdate(DateTime now)
        {
            var hour = now.Hour % 12;
            hour = hour == 0 ? 12 : hour;

            this.SetCurrentValue(TimeProperty, now);
            this.SetCurrentValue(HoursProperty, hour);
            this.SetCurrentValue(MinutesProperty, now.Minute);
            this.SetCurrentValue(SecondsProperty, now.Second);
            this.SetCurrentValue(MillisecondsProperty, now.Millisecond);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
