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

using Windows.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.WiFi;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Security.Credentials;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginScreen_10 : Page
    {
        private DispatcherTimer DateTimetimer;
        private DispatcherTimer RFIDCheck;

        private DispatcherTimer LoadingTimer;

        private static Mfrc522 mfrc;

        private static string strUserSettingJSON;
        private NetworkPresenter networkPresenter = new NetworkPresenter();

        private BitmapImage wifiOnImage, wifiOffImage;
        private bool bWifiConnected = false;
        /*
        public async void WebServerFoundError()
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Statustxt.Text = "We can't connect Web Server! You can check network status.";
                Statustxt.Visibility = Visibility.Visible;
            });
        }
        */

        private async Task<bool> LoadRPISetting()
        {
            // Load User Settings
            bool bServerConnected = true;
            strUserSettingJSON = await App.GetUserSettingStr();
            if(strUserSettingJSON == null)
            {
                strUserSettingJSON = await App.ReadSettingFromFile();
                FlashSettings.StartupMsg = "";
                FlashSettings.CheckListMsg1 = "CheckList1";
                FlashSettings.CheckListMsg2 = "CheckList2";
                FlashSettings.CheckListMsg3 = "CheckList3";
                FlashSettings.CheckListMsg4 = "CheckList4";
                bServerConnected = false;
            }
            else
            {
                JsonArray jsonArr = JsonArray.Parse(strUserSettingJSON);
                if (jsonArr.Count == 0)
                    strUserSettingJSON = await App.ReadSettingFromFile();
                else
                    await App.WriteSettingIntoFile(strUserSettingJSON);
            }

            // Load Message Settings
            FlashSettings.StartupMsg = Convert.ToString(ApplicationData.Current.LocalSettings.Values[Constants.StartupMsg]);
            FlashSettings.CheckListMsg1 = Convert.ToString(ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg1]);
            FlashSettings.CheckListMsg2 = Convert.ToString(ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg2]);
            FlashSettings.CheckListMsg3 = Convert.ToString(ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg3]);
            FlashSettings.CheckListMsg4 = Convert.ToString(ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg4]);

            if (bServerConnected)
            {
                HttpClient httpClient = new HttpClient();
                CancellationTokenSource cts = new CancellationTokenSource();
                try
                {
                    string UrlString = Constants.ServerURL;
                    string ContentStr = "query=SELECT * FROM rpi_list where devNum = ";
                    ContentStr += Constants.nDeviceNum.ToString();

                    Uri ResAddress;
                    Uri.TryCreate(UrlString.Trim(), UriKind.Absolute, out ResAddress);
                    HttpResponseMessage response = await httpClient.PostAsync(ResAddress,
                        new HttpStringContent(ContentStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")).AsTask(cts.Token);
                    string responseBodyAsText;
                    responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                    cts.Token.ThrowIfCancellationRequested();
                    responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

                    JsonArray jsonArr = JsonArray.Parse(responseBodyAsText);
                    if (jsonArr.Count > 0)
                    {
                        JsonObject obj = jsonArr.GetObjectAt(0);
                        FlashSettings.StartupMsg = obj.GetNamedString("StartupMsg");
                        FlashSettings.CheckListMsg1 = obj.GetNamedString("CheckListMsg1");
                        FlashSettings.CheckListMsg2 = obj.GetNamedString("CheckListMsg2");
                        FlashSettings.CheckListMsg3 = obj.GetNamedString("CheckListMsg3");
                        FlashSettings.CheckListMsg4 = obj.GetNamedString("CheckListMsg4");

                        // Save to file 
                        ApplicationData.Current.LocalSettings.Values[Constants.StartupMsg] = FlashSettings.StartupMsg;
                        ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg1] = FlashSettings.CheckListMsg1;
                        ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg2] = FlashSettings.CheckListMsg2;
                        ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg3] = FlashSettings.CheckListMsg3;
                        ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg4] = FlashSettings.CheckListMsg4;
                    }
                    httpClient.Dispose();
                    httpClient = null;
                    cts.Dispose();
                    cts = null;
                }
                catch (Exception ex)
                { // Erroe
                    cts.Token.ThrowIfCancellationRequested();
                    //WebServerFoundError();

                    if (httpClient != null)
                    {
                        httpClient.Dispose();
                        httpClient = null;
                    }
                    if (cts != null)
                    {
                        cts.Dispose();
                        cts = null;
                    }
                    bServerConnected = false;
                }
            }
            else
            {
                //WebServerFoundError();
            }
            return bServerConnected;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            strUserSettingJSON = "";
            await UpdateDateTime(true);
            DateTimetimer = new DispatcherTimer();
            DateTimetimer.Tick += DateTime_Tick;
            DateTimetimer.Interval = TimeSpan.FromSeconds(20);
            DateTimetimer.Start();

            LoadingPage.Visibility = Visibility.Visible;
            Arrow.Visibility = Visibility.Collapsed;
            LoadingTimer = new DispatcherTimer();
            LoadingTimer.Tick += Loading_Tick;
            LoadingTimer.Interval = TimeSpan.FromMilliseconds(100);
            LoadingTimer.Start();

            bool bServerConnected = await LoadRPISetting();
            if(bServerConnected)
                await App.CopyFromSQLite();

            LoadingPage.Visibility = Visibility.Collapsed;
            Arrow.Visibility = Visibility.Visible;

            if (mfrc == null)
            {
                mfrc = new Mfrc522();
                await mfrc.InitIO();
            }

            RFIDCheck = new DispatcherTimer();
            RFIDCheck.Tick += RFIDCheck_Tick;
            RFIDCheck.Interval = TimeSpan.FromMilliseconds(500);
            RFIDCheck.Start();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (DateTimetimer.IsEnabled)
                DateTimetimer.Stop();
            DateTimetimer = null;
            if (RFIDCheck.IsEnabled)
                RFIDCheck.Stop();
            RFIDCheck = null;
        }

        public LoginScreen_10()
        {
            wifiOffImage = new BitmapImage(new Uri("ms-appx:///Assets/WIFI_OFF.png"));
            wifiOnImage = new BitmapImage(new Uri("ms-appx:///Assets/WIFI_ON.png"));
            this.InitializeComponent();
        }

        private async void RFIDCheck_Tick(object sender, object e)
        {
            if (mfrc.IsTagPresent())
            {
                var uid = mfrc.ReadUid();
                String StrUUID = "";
                short idx = 0;
                for (idx = 0; idx < 5; idx++)
                    StrUUID += uid.FullUid[idx].ToString("X02");
                mfrc.HaltTag();

                if (strUserSettingJSON == null)
                    return;

                RFIDCheck.Stop();
                SYSTEMTIME localTime;
                NativeTimeMethods.GetLocalTime(out localTime);
                DateTime t = localTime.ToDateTime();

                /* User Login Process */
                try
                {
                    JsonArray jsonArr = JsonArray.Parse(strUserSettingJSON);
                    if (jsonArr.Count > 0)
                    {
                        bool bFind = false;
                        for (uint index = 0; index < jsonArr.Count; index++)
                        {
                            JsonObject obj = jsonArr.GetObjectAt(index);
                            if (StrUUID.CompareTo(obj.GetNamedString("rfid")) == 0)
                            {
                                UserInfo.accountType = Convert.ToInt16(obj.GetNamedString("accountType"));
                                UserInfo.UserName = obj.GetNamedString("userName");
                                UserInfo.UserID = obj.GetNamedString("userid");
                                UserInfo.rfid = obj.GetNamedString("rfid");
                                UserInfo.textMsg = obj.GetNamedString("textMsg");

                                bFind = true;
                                break;
                            }
                        }

                        if (bFind)
                        {
                            // If correct user login.
                            Type nextScreen = null;
                            switch (UserInfo.accountType)
                            {
                                case (int)UserTypes.ADMINISTRATOR:
                                    {
                                        nextScreen = typeof(LogAdmin_300);
                                        UserInfo.LastUser = UserInfo.UserName;
                                    }
                                    break;

                                case (int)UserTypes.GENERALUSER:
                                    {
                                        if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated]))
                                            nextScreen = typeof(TiltAlarmScreen_121C);
                                        else
                                            nextScreen = typeof(LogScreen_100);
                                    }
                                    break;

                                case (int)UserTypes.SUPERVISOR:
                                    {
                                        nextScreen = typeof(Supervisor_200);
                                        UserInfo.LastUser = UserInfo.UserName;
                                    }
                                    break;
                            }

                            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                NavigationUtils.NavigateToScreen(nextScreen);
                            });
                        }
                        else
                        {
                            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                            {
                                Statustxt.Text = "This Card or User is not registered on this Machine.";
                                Statustxt.Visibility = Visibility.Visible;
                            });

                            // Send Reason to Database 
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

                            string strMsg = "Unregistered RFID" + Environment.NewLine;
                            strMsg += StrUUID;

                            bool bRes = false;
                            bRes = await App.SaveMessageIntoDB(strDateTime, strMsg);
                            RFIDCheck.Start();
                        }
                    }
                    else
                    {
                        await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            Statustxt.Text = "There is no User Setting on this Machine.";
                            Statustxt.Visibility = Visibility.Visible;
                        });
                        RFIDCheck.Start();
                    }
                }
                catch (Exception ex)
                {
                    await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        Statustxt.Text = "There is no User Setting on this Machine.";
                        Statustxt.Visibility = Visibility.Visible;
                    });
                    RFIDCheck.Start();
                }
            }
        }

        private async void DateTime_Tick(object sender, object e)
        {
            await UpdateDateTime();
        }

        private async void Loading_Tick(object sender, object e)
        {
            var value = LoadingProgress.Value + LoadingProgress.SmallChange * 5;
            LoadingProgress.Value = value;
            if (value >= LoadingProgress.Maximum)
                LoadingTimer.Stop();
        }


        private async Task UpdateDateTime(bool bOption = false)
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SYSTEMTIME localTime;
                NativeTimeMethods.GetLocalTime(out localTime);
                DateTime t = localTime.ToDateTime();
                CurrentTime.Text = t.ToString("t", CultureInfo.CurrentCulture) + Environment.NewLine + t.ToString("d", CultureInfo.CurrentCulture);
            });

            bool bEthernet = false;
            bool bWifi = false;

            var ethernetProfile = NetworkPresenter.GetDirectConnectionName();

            if (ethernetProfile != null)
                bEthernet = true;

            if (await networkPresenter.WifiIsAvailable())
            {
                var networks = await networkPresenter.GetAvailableNetworks();
                if (networks.Count > 0)
                {
                    var connectedNetwork = networkPresenter.GetCurrentWifiNetwork();
                    if (connectedNetwork != null)
                    {
                        WifiImage.Source = null;
                        //WifiImage.Source = wifiOnImage;
                        bWifi = true;
                    }
                    else
                    {
                        bWifi = false;
                        WifiImage.Source = wifiOffImage;
                        if (bWifiConnected && ethernetProfile == null)
                            bWifiConnected = false;
                    }
                }
                else
                {
                    WifiImage.Source = wifiOffImage;
                    bWifi = false;
                }
            }
            if(!bOption)
            {
                if (bWifiConnected)
                {
                    if (bWifi == false && bEthernet == false)
                        bWifiConnected = false;
                }
                else
                {
                    if (bWifi || bEthernet)
                    {
                        bool bRes = await LoadRPISetting();
                        if (bRes)
                        {
                            bWifiConnected = true;
                            await App.CopyFromSQLite();
                        }
                    }
                }
            }

        }
    }
}
