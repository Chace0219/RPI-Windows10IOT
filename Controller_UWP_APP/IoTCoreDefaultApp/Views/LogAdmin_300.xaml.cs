using IoTCoreDefaultApp.Utils;
using System;
using System.Globalization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LogAdmin_300 : Page
    {
        private DispatcherTimer DateTimetimer;

        public LogAdmin_300()
        {
            this.InitializeComponent();
            this.Loaded += async (sender, e) =>
            {
                UpdateDateTime();

                if(Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated]))
                {
                    TiltAlramResetButton.IsEnabled = true;
                }

                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));
                    
                    WelcomeText.Text = "Welcome Administrator (" + UserInfo.UserName;
                    WelcomeText.Text += "),";
                    WelcomeText.Text += Environment.NewLine;
                    WelcomeText.Text += "(";
                    WelcomeText.Text += UserInfo.rfid;
                    WelcomeText.Text += ")";
                    WelcomeText.Text += Environment.NewLine;
                    WelcomeText.Text += "You are succesfully logged on";
                });

                DateTimetimer = new DispatcherTimer();
                DateTimetimer.Tick += DateTime_Tick;
                DateTimetimer.Interval = TimeSpan.FromSeconds(5);
                DateTimetimer.Start();

            };
            this.Unloaded += (sender, e) =>
            {
                DateTimetimer.Stop();
                DateTimetimer = null;
            };
        }

        private void DateTime_Tick(object sender, object e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime t = localTime.ToDateTime();
            CurrentTime.Text = t.ToString("t", CultureInfo.CurrentCulture) + Environment.NewLine + t.ToString("d", CultureInfo.CurrentCulture);
        }

        private async void LogoffButt_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
            });
        }

        private async void TiltAlramResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Tilt Alarm Reset
            TiltAlramResetButton.IsEnabled = false;
            bool bResult = await App.ResetAlarm();
            TiltAlramResetButton.IsEnabled = true;

            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);
            DateTime t = localTime.ToDateTime();
            String strDateTime;
            strDateTime = t.Year.ToString();
            strDateTime += "-";
            strDateTime += t.Month.ToString("00");
            strDateTime += "-";
            strDateTime += t.Day.ToString("00");
            strDateTime += " ";
            strDateTime += t.Hour.ToString("00");
            strDateTime += ":";
            strDateTime += t.Minute.ToString("00");
            strDateTime += ":";
            strDateTime += t.Second.ToString("00");

            String strMsg = "Reset Event" + Environment.NewLine;
            strMsg += "Reset By Administrator! Name:";
            strMsg += UserInfo.UserName;
            strMsg += ", UserID:";
            strMsg += UserInfo.UserID;
            strMsg += Environment.NewLine;
            strMsg += "Current Reset Count is ";
            strMsg += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();
            strMsg += Environment.NewLine;

            bResult = await App.SaveMessageIntoDB(strDateTime, strMsg);
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
            });
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(AdminMain_310));
            });
        }
    }
}
