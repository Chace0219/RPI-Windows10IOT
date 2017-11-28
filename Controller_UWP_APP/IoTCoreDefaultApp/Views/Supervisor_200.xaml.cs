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
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Supervisor_200 : Page
    {
        private DispatcherTimer DateTimetimer;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await UpdateDateTime();
            DateTimetimer = new DispatcherTimer();
            DateTimetimer.Tick += DateTime_Tick;
            DateTimetimer.Interval = TimeSpan.FromSeconds(5);
            DateTimetimer.Start();

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));

                //
                StatusTxt.Text = "Welcome Supervisor ";
                StatusTxt.Text += UserInfo.UserName;
                StatusTxt.Text += Environment.NewLine;
                StatusTxt.Text += "UserID : ";
                StatusTxt.Text += UserInfo.UserID;
                StatusTxt.Text += Environment.NewLine;
                StatusTxt.Text += "You are succesfully logged on.";

                // 
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated]) == true)
                    ResetButton.IsEnabled = true;

                ResetCnt.Text = Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();
            });
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Handling of this event is included for completenes, as it will only fire when navigating between pages and this sample only includes one page
            DateTimetimer.Stop();
            DateTimetimer = null;
        }

        public Supervisor_200()
        {
            this.InitializeComponent();
        }

        private async void DateTime_Tick(object sender, object e)
        {
            await UpdateDateTime();
        }

        private async Task UpdateDateTime()
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SYSTEMTIME localTime;
                NativeTimeMethods.GetLocalTime(out localTime);
                DateTime t = localTime.ToDateTime();
                CurrentTime.Text = t.ToString("t", CultureInfo.CurrentCulture) + Environment.NewLine + t.ToString("d", CultureInfo.CurrentCulture);
            });
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            int nCurrResetCnt = Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]);

            if (nCurrResetCnt >= Constants.MaxResetCount)
            {
                NotifyTxt.Visibility = Visibility.Visible;
            }
            else
            {
                ResetButton.IsEnabled = false;
                bool bRes = await App.ResetAlarm();

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
                strMsg += "Reset By Supervisor! Name:";
                strMsg += UserInfo.UserName;
                strMsg += ", UserID:";
                strMsg += UserInfo.UserID;
                strMsg += Environment.NewLine;
                strMsg += "Current Reset Count is ";
                strMsg += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();

                await App.SaveMessageIntoDB(strDateTime, strMsg);
            }

        }

        private async void LogoffButton_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
            });
        }
    }
}
