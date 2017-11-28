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
    public sealed partial class AdminLoadDB_315 : Page
    {
        private DispatcherTimer DateTimetimer;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await UpdateDateTime();
            await RefreshTextBox();

            DateTimetimer = new DispatcherTimer();
            DateTimetimer.Tick += DateTime_Tick;
            DateTimetimer.Interval = TimeSpan.FromSeconds(5);
            DateTimetimer.Start();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (DateTimetimer.IsEnabled)
                DateTimetimer.Stop();
            DateTimetimer = null;
        }

        public AdminLoadDB_315()
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

        private async Task RefreshTextBox()
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                // Server Database Interaction
                LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));

                Content.Text = "Current System Infomation!";
                Content.Text += Environment.NewLine;

                Content.Text += " ";
                Content.Text += Environment.NewLine;

                Content.Text += "- Device Setting";
                Content.Text += Environment.NewLine;
                Content.Text += "   Device ID:";
                Content.Text += Constants.nDeviceNum;
                Content.Text += Environment.NewLine;

                Content.Text += "   Digital CheckList ";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EanbleCheckListKey]))
                    Content.Text += "Enabled";
                else
                    Content.Text += "Disabled";
                Content.Text += Environment.NewLine;

                Content.Text += "   Camera Function ";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey]))
                    Content.Text += "Enabled";
                else
                    Content.Text += "Disabled";
                Content.Text += Environment.NewLine;

                Content.Text += "   Tilt Function ";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EanbleTiltKey]))
                    Content.Text += "Enabled";
                else
                    Content.Text += "Disabled";
                Content.Text += Environment.NewLine;

                Content.Text += "   Tilt Alarm Sensitivity Setting ";
                Content.Text += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey]).ToString() + "%";
                Content.Text += Environment.NewLine;


                Content.Text += "- Working Status in Current Shifting";
                Content.Text += Environment.NewLine;

                Content.Text += "   Last Logon Time";
                Content.Text += UserInfo.LogoffTime;
                Content.Text += Environment.NewLine;

                Content.Text += "   Working Hours ";
                Int32 nSecs = Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.RunningTime]);
                Int32 nHour = nSecs / 3600;
                Int32 nMin = (nSecs % 3600) / 60;
                Int32 nSec = (nSecs % 3600) % 60;
                Content.Text += nHour.ToString("00:");
                Content.Text += nMin.ToString("00:");
                Content.Text += nSec.ToString("00");
                Content.Text += Environment.NewLine;

                Content.Text += "   Alarm Reset Count ";
                Content.Text += Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();
                Content.Text += Environment.NewLine;

            });
        }

        private async void LogOffButton_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
            });

        }

        private async void TiltSensorActivity_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(TitlSenseActivity_330));
            });

        }

        private async void Checklist_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(CheckListActive_325));
            });
        }

        private async void CameraActivity_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(CameraActivity_335));
            });
        }

        private async void TiltFunc_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(SwitchTilt_320));
            });
        }

        private async void LoadDB_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(AdminLoadDB_315));
            });
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(AdminMain_310));
            });
        }

        private async Task UpdateControls(bool bUpdate)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (bUpdate)
                {
                    Upload.IsEnabled = true;
                    Download.IsEnabled = true;
                    Reset.IsEnabled = true;
                    BackButton.IsEnabled = true;
                }
                else
                {
                    Upload.IsEnabled = false;
                    Download.IsEnabled = false;
                    Reset.IsEnabled = false;
                    BackButton.IsEnabled = false;
                }
            });

        }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            await UpdateControls(false);
            bool bResult = await App.ResetAlarm(true);
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

            String strMsg = "Full Reset Event" + Environment.NewLine;
            strMsg += "Reset Count of Alarm By Administrator! Name:";
            strMsg += UserInfo.UserName;
            strMsg += ", UserID:";
            strMsg += UserInfo.UserID;
            strMsg += Environment.NewLine;
            strMsg += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();

            await App.SaveMessageIntoDB(strDateTime, strMsg);

            await RefreshTextBox();
            await UpdateControls(true);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            await UpdateControls(false);
            await App.GetSettingsFromDB();
            await RefreshTextBox();
            await UpdateControls(true);
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            await UpdateControls(false);
            await App.SetSettingsIntoDB();
            await RefreshTextBox();
            await UpdateControls(true);
        }
    }
}
