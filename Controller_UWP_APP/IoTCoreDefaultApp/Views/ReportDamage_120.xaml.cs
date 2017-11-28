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

using Windows.Web.Http;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReportDamage_120 : Page
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
                MyStatus.Text = "";
            });
        }
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (DateTimetimer.IsEnabled)
                DateTimetimer.Stop();
            DateTimetimer = null;
        }

        public ReportDamage_120()
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

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(OperatorScreen_110));
            });
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Send Reason to Database 
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

            string strMsg = "Damage Report-User:" + UserInfo.UserName;
            strMsg += Environment.NewLine;
            strMsg += MyStatus.Text;

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                SendButton.IsEnabled = false;
                BackButton.IsEnabled = false;
            });
            bool bRes = false;
            bRes = await App.SaveMessageIntoDB(strDateTime, strMsg);
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (!bRes)
                    Statustxt.Visibility = Visibility.Visible;
                SendButton.IsEnabled = true;
                BackButton.IsEnabled = true;

                MyStatus.Text = "";

            });

        }
    }
}
