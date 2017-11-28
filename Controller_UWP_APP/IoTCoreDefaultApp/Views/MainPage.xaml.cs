// Copyright (c) Microsoft. All rights reserved.

using IoTCoreDefaultApp.Utils;
using System;
using System.Globalization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace IoTCoreDefaultApp
{
    //public sealed partial class MainPage : StandardPage
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        private CoreDispatcher MainPageDispatcher;
        private DispatcherTimer DateTimetimer;
        private ConnectedDevicePresenter connectedDevicePresenter;

        private DispatcherTimer countdown;
        private DispatcherTimer delaytimer;

        public CoreDispatcher UIThreadDispatcher
        {
            get
            {
                return MainPageDispatcher;
            }

            set
            {
                MainPageDispatcher = value;
            }
            
        }

        public MainPage()
        {
            this.InitializeComponent();

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            MainPageDispatcher = Window.Current.Dispatcher;

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.DataContext = LanguageManager.GetInstance();

            this.Loaded += async (sender, e) => 
            {
                await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    UpdateBoardInfo();
                    UpdateNetworkInfo();
                    UpdateDateTime();
                    UpdateConnectedDevices();

                    DateTimetimer = new DispatcherTimer();
                    DateTimetimer.Tick += DateTime_Tick;
                    DateTimetimer.Interval = TimeSpan.FromSeconds(5);
                    DateTimetimer.Start();

                    countdown = new DispatcherTimer();
                    countdown.Tick += countdown_Tick;
                    countdown.Interval = TimeSpan.FromMilliseconds(100);

                    delaytimer = new DispatcherTimer();
                    delaytimer.Tick += DelayTimer_Tick;
                    delaytimer.Interval = TimeSpan.FromSeconds(10);
                    delaytimer.Start();

                    LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));

                });
            };
            this.Unloaded += (sender, e) =>
            {
                DateTimetimer.Stop();
                DateTimetimer = null;

                delaytimer.Stop();
                delaytimer = null;
                countdown.Stop();
                countdown = null;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.HasDoneOOBEKey))
            {
                ApplicationData.Current.LocalSettings.Values[Constants.HasDoneOOBEKey] = Constants.HasDoneOOBEValue;
            }

            base.OnNavigatedTo(e);
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                UpdateNetworkInfo();
            });
        }

        private void DateTime_Tick(object sender, object e)
        {
            UpdateDateTime();
        }

        private void DelayTimer_Tick(object sender, object e)
        {
            delaytimer.Stop();
            NavigetNextPage.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            countdown.Start();
        }

        private async void countdown_Tick(object sender, object e)
        {
            var value = NavigetNextPageProgress.Value + NavigetNextPageProgress.SmallChange * 5;
            NavigetNextPageProgress.Value = value;
            if (value >= NavigetNextPageProgress.Maximum)
            {
                countdown.Stop();
                await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, () => 
                {
                    NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
                });
            }
        }

        private void UpdateBoardInfo()
        {
            BoardName.Text = DeviceInfoPresenter.GetBoardName();
            BoardImage.Source = new BitmapImage(DeviceInfoPresenter.GetBoardImageUri());

            ulong version = 0;
            if (!ulong.TryParse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion, out version))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                OSVersion.Text = loader.GetString("OSVersionNotAvailable");
            }
            else
            {
                OSVersion.Text = String.Format(CultureInfo.InvariantCulture,"{0}.{1}.{2}.{3}",
                    (version & 0xFFFF000000000000) >> 48,
                    (version & 0x0000FFFF00000000) >> 32,
                    (version & 0x00000000FFFF0000) >> 16,
                    version & 0x000000000000FFFF);
            }
        }

        private async void UpdateNetworkInfo()
        {
            this.DeviceName.Text = DeviceInfoPresenter.GetDeviceName();
            this.IPAddress1.Text = NetworkPresenter.GetCurrentIpv4Address();
            this.NetworkName1.Text = NetworkPresenter.GetCurrentNetworkName();
            this.NetworkInfo.ItemsSource = await NetworkPresenter.GetNetworkInformation();
        }

        private void UpdateConnectedDevices()
        {
            connectedDevicePresenter = new ConnectedDevicePresenter(MainPageDispatcher);
            this.ConnectedDevices.ItemsSource = connectedDevicePresenter.GetConnectedDevices();
        }

        private async void NextButton_Clicked(object sender, RoutedEventArgs e)
        {
            /*
            var networkPresenter = new NetworkPresenter();
            var wifiAvailable = networkPresenter.WifiIsAvailable();
            SetPreferences();
            Type nextScreen;

            try
            {
                nextScreen = (await wifiAvailable) ? typeof(OOBENetwork) : typeof(MainPage);
            }
            catch (Exception)
            {
                nextScreen = typeof(MainPage);
            }

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(nextScreen);
            });
            */
            Type nextScreen = typeof(LoginScreen_10);
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(nextScreen);
            });

        }
        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
            NavigetNextPage.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void ShutdownButton_Clicked(object sender, RoutedEventArgs e)
        {
            ShutdownDropdown.IsOpen = true;
        }

        private void CommandLineButton_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationUtils.NavigateToScreen(typeof(CommandLinePage));
        }

        private void SettingsButton_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationUtils.NavigateToScreen(typeof(Settings));
        }

        private void ShutdownHelper(ShutdownKind kind)
        {
            new System.Threading.Tasks.Task(() =>
            {
                ShutdownManager.BeginShutdown(kind, TimeSpan.FromSeconds(0));
            }).Start();
        }

        private void UpdateDateTime()
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime t = localTime.ToDateTime();
            CurrentTime.Text = t.ToString("t", CultureInfo.CurrentCulture) + Environment.NewLine + t.ToString("d", CultureInfo.CurrentCulture);
        }

        private void ShutdownListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FrameworkElement;
            if (item == null)
            {
                return;
            }
            switch (item.Name)
            {
                case "ShutdownOption":
                    ShutdownHelper(ShutdownKind.Shutdown);
                    break;
                case "RestartOption":
                    ShutdownHelper(ShutdownKind.Restart);
                    break;
            }
        }

        private void ShutdownDropdown_Opened(object sender, object e)
        {
            var w = ShutdownListView.ActualWidth;
            if (w == 0)
            {
                // trick to recalculate the size of the dropdown
                ShutdownDropdown.IsOpen = false;
                ShutdownDropdown.IsOpen = true;
            }
            var offset = -(ShutdownListView.ActualWidth - ShutdownButton.ActualWidth);
            ShutdownDropdown.HorizontalOffset = offset;
        }

    }
}
