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
    public sealed partial class CheckList_121A : Page
    {
        private DispatcherTimer DateTimetimer;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));

                CheckList1Toggle.IsOn = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList1]);
                CheckList2Toggle.IsOn = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList2]);
                CheckList3Toggle.IsOn = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList3]);
                CheckList4Toggle.IsOn = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList4]);

                CheckList1Txt.Text = FlashSettings.CheckListMsg1;
                CheckList2Txt.Text = FlashSettings.CheckListMsg2;
                CheckList3Txt.Text = FlashSettings.CheckListMsg3;
                CheckList4Txt.Text = FlashSettings.CheckListMsg4;

            });

            await UpdateDateTime();
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
        public CheckList_121A()
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

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[Constants.CheckList1] = CheckList1Toggle.IsOn;
            ApplicationData.Current.LocalSettings.Values[Constants.CheckList2] = CheckList2Toggle.IsOn;
            ApplicationData.Current.LocalSettings.Values[Constants.CheckList3] = CheckList3Toggle.IsOn;
            ApplicationData.Current.LocalSettings.Values[Constants.CheckList4] = CheckList4Toggle.IsOn;
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(ForkLiftActivate_121B));
            });
        }
    }
}
