// Copyright (c) Microsoft. All rights reserved.


using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;

using Windows.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

using Windows.System.Threading;
using IoTCoreDefaultApp.Utils;

using SQLite.Net.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

namespace IoTCoreDefaultApp
{
    public class EventList
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int devNum { get; set; }
        public int userid { get; set; }
        public string eventTime { get; set; }
        public string message { get; set; }
        public string Photo { get; set; }

    }

    public class InboundPairingEventArgs
    {
        public InboundPairingEventArgs(DeviceInformation di)
        {
            DeviceInfo = di;
        }
        public DeviceInformation DeviceInfo
        {
            get;
            private set;
        }
    }
    // Callback handler delegate type for Inbound pairing requests
    public delegate void InboundPairingRequestedHandler(object sender, InboundPairingEventArgs inboundArgs);

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        // Handler for Inbound pairing requests
        public static event InboundPairingRequestedHandler InboundPairingRequested;

        private ThreadPoolTimer timer;

        public static bool bLogged = false;

        public static async Task<bool> InsertIntoSQLite(string strDateTime, string strMsg, string strContent = "")
        {

            try
            {
                string path;
                SQLite.Net.SQLiteConnection conn;
                path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "TempDB.sqlite");

                conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

                conn.CreateTable<EventList>();

                var s = conn.Insert(new EventList()
                {
                    devNum = Constants.nDeviceNum,
                    userid = Convert.ToInt32(UserInfo.UserID),
                    eventTime = strDateTime,
                    message = strMsg,
                    Photo = strContent
                });
                conn.Dispose();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> CopyFromSQLite()
        {

            try
            {
                string path;
                SQLite.Net.SQLiteConnection conn;
                path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "TempDB.sqlite");
                conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
                conn.CreateTable<EventList>();

                var query = conn.Table<EventList>();
                if(query.Count() > 0)
                {
                    string strDateTime = "";
                    string strMsg = "";
                    string strContent = "";

                    int devNum = 0;
                    foreach (var message in query)
                    {
                        strMsg = message.message;
                        strDateTime = message.eventTime;
                        strContent = message.Photo;
                        devNum = message.devNum;

                        string tempID = UserInfo.UserID;
                        UserInfo.UserID = message.userid.ToString();
                        bool bRes = await App.InsertRowIntoDB(strDateTime, strMsg, strContent);
                        UserInfo.UserID = tempID;
                        if (!bRes)
                        {
                            conn.Dispose();
                            return false;
                        }
                    }
                }
                conn.DeleteAll<EventList>();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static async Task<string> GetUserSettingStr()
        {
            string strUserSetting = null;

            HttpClient httpClient;
            CancellationTokenSource cts;

            httpClient = new HttpClient();
            cts = new CancellationTokenSource();

            try
            {
                String UrlString = Constants.ServerURL;
                String ContentStr = "query=UPDATE rpi_list SET digitalCheckList=";
                ContentStr = "query=SELECT * FROM user_info, setting_list where user_info.userid = setting_list.userid AND devNum = ";
                ContentStr += Constants.nDeviceNum.ToString();

                Uri ResAddress;
                Uri.TryCreate(UrlString.Trim(), UriKind.Absolute, out ResAddress);

                HttpResponseMessage response = await httpClient.PostAsync(ResAddress,
                    new HttpStringContent(ContentStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")).AsTask(cts.Token);

                string responseBodyAsText;
                responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                cts.Token.ThrowIfCancellationRequested();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);
                strUserSetting = responseBodyAsText;
            }
            catch (Exception ex)
            { // Error
                cts.Token.ThrowIfCancellationRequested();
            }

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
            return strUserSetting;
        }

        public static async Task<bool> WriteSettingIntoFile(string strContent)
        {
            try
            {
                StorageFile SettingFile;
                SettingFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync("Setting.txt", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(SettingFile, strContent);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static async Task<string> ReadSettingFromFile()
        {
            string strContent = null;
            try
            {
                StorageFile SettingFile;
                SettingFile = await Windows.Storage.KnownFolders.PicturesLibrary.GetFileAsync("Setting.txt");
                strContent = await Windows.Storage.FileIO.ReadTextAsync(SettingFile);

            }
            catch (Exception ex)
            {
                return strContent;
            }
            return strContent;
        }

        public static async Task<bool> SetSettingsIntoDB()
        {
            HttpClient httpClient;
            CancellationTokenSource cts;
            bool bResult = true;
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();

            try
            {
                String UrlString = Constants.ServerURL;
                String ContentStr = "query=UPDATE rpi_list SET digitalCheckList='";
                ContentStr += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.EanbleCheckListKey]).ToString();
                ContentStr += "', CameraFunc='";
                ContentStr += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey]).ToString();
                ContentStr += "', TiltFunc='";
                ContentStr += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.EanbleTiltKey]).ToString();
                ContentStr += "', TiltSensitivity='";
                ContentStr += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey]).ToString();
                ContentStr += "' WHERE devNum=";
                ContentStr += Constants.nDeviceNum.ToString();
                ContentStr += "";

                Uri ResAddress;
                Uri.TryCreate(UrlString.Trim(), UriKind.Absolute, out ResAddress);

                HttpResponseMessage response = await httpClient.PostAsync(ResAddress,
                    new HttpStringContent(ContentStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")).AsTask(cts.Token);

                string responseBodyAsText;
                responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                cts.Token.ThrowIfCancellationRequested();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

            }
            catch (Exception ex)
            { // Error
                cts.Token.ThrowIfCancellationRequested();
                bResult = false;
            }

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
            return bResult;
        }

        public static async Task<bool> InsertRowIntoDB(string strDateTime, string strMsg, string strContent = "")
        {

            HttpClient httpClient;
            CancellationTokenSource cts;
            bool bResult = true;
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();

            try
            {

                String UrlString = Constants.ServerURL;
                String ContentStr = "query=INSERT INTO report_list (`devNum`, `userid`, `eventTime`, `message`, `Photo`) VALUES('";
                ContentStr += Constants.nDeviceNum.ToString();
                ContentStr += "', '";
                ContentStr += UserInfo.UserID;
                ContentStr += "', '";
                ContentStr += strDateTime;
                ContentStr += "', N'";
                ContentStr += strMsg;
                ContentStr += "', ";
                if (strContent.CompareTo("") == 0)
                    ContentStr += "''";
                else
                    ContentStr += strContent;
                ContentStr += ")";

                Uri ResAddress;
                Uri.TryCreate(UrlString.Trim(), UriKind.Absolute, out ResAddress);

                HttpResponseMessage response = await httpClient.PostAsync(ResAddress,
                    new HttpStringContent(ContentStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")).AsTask(cts.Token);

                string responseBodyAsText;
                responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                cts.Token.ThrowIfCancellationRequested();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

            }
            catch (Exception ex)
            { // Error
                cts.Token.ThrowIfCancellationRequested();
                bResult = false;
            }

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
            return bResult;
        }

        public static async Task<bool> SaveMessageIntoDB(string strDateTime, string strMsg, string strContent = "")
        {
            await App.InsertIntoSQLite(strDateTime, strMsg, strContent);
            bool bResult = await App.CopyFromSQLite();
            return bResult;
        }

        public static async Task<bool> GetSettingsFromDB()
        {
            bool bResult = true;
            HttpClient httpClient;
            CancellationTokenSource cts;
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();

            try
            {
                String UrlString = Constants.ServerURL;
                String ContentStr = "query=SELECT * FROM rpi_list WHERE devNum=";
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
                    ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt] = Convert.ToInt16(obj.GetNamedString("ResetCnt"));
                    ApplicationData.Current.LocalSettings.Values[Constants.EanbleTiltKey] = Convert.ToInt16(obj.GetNamedString("TiltFunc"));
                    ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey] = Convert.ToInt16(obj.GetNamedString("CameraFunc"));
                    ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey] = Convert.ToInt16(obj.GetNamedString("TiltSensitivity"));
                    ApplicationData.Current.LocalSettings.Values[Constants.EanbleCheckListKey] = Convert.ToInt16(obj.GetNamedString("digitalChecklist"));
                }
                else
                {
                    bResult = false;
                }
            }
            catch (Exception ex)
            { // Error
                cts.Token.ThrowIfCancellationRequested();
                bResult = false;
            }

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
            return bResult;
        }

        public static async Task<bool> ResetAlarm(bool bFull = false)
        {
            bool bResult = true;
            HttpClient httpClient;
            CancellationTokenSource cts;
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();

            if(bFull)
            {
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated] = false;
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt] = 0;

            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated] = false;
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt] = Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]) + 1;
            }

            try
            {
                String UrlString = Constants.ServerURL;
                String ContentStr = "query=UPDATE rpi_list SET ResetCnt=";
                ContentStr += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();
                ContentStr += " WHERE devNum=";
                ContentStr += Constants.nDeviceNum.ToString();

                Uri ResAddress;
                Uri.TryCreate(UrlString.Trim(), UriKind.Absolute, out ResAddress);

                HttpResponseMessage response = await httpClient.PostAsync(ResAddress,
                    new HttpStringContent(ContentStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")).AsTask(cts.Token);

                string responseBodyAsText;
                responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                cts.Token.ThrowIfCancellationRequested();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

            }
            catch (Exception ex)
            { // Error
                cts.Token.ThrowIfCancellationRequested();
                bResult = false;
            }

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
            return bResult;
        }

        // Don't try and make discoverable if this has already been done
        private static bool isDiscoverable = false;

        public static bool IsBluetoothDiscoverable
        {
            get
            {
                return isDiscoverable;
            }

            set
            {
                isDiscoverable = value;
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // System.Diagnostics.Debugger.Break();
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

            /*#if DEBUG
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            this.DebugSettings.EnableFrameRateCounter = true;
                        }
            #endif*/

            Frame rootFrame = Window.Current.Content as Frame;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.EanbleTiltKey))
                ApplicationData.Current.LocalSettings.Values[Constants.EanbleTiltKey] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.EanbleCheckListKey))
                ApplicationData.Current.LocalSettings.Values[Constants.EanbleCheckListKey] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.TiltSensitiKey))
                ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey] = 20;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.EnableCamKey))
                ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.WebCamPreviewKey))
                ApplicationData.Current.LocalSettings.Values[Constants.WebCamPreviewKey] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.TiltAlarmActivated))
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckList1))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckList1] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckList2))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckList2] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckList3))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckList3] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckList4))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckList4] = false;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.TiltAlarmCnt))
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt] = 0;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.RunningTime))
                ApplicationData.Current.LocalSettings.Values[Constants.RunningTime] = 0;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckMsg1))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg1] = "CheckMsg1";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckMsg2))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg2] = "CheckMsg2";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckMsg3))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg3] = "CheckMsg3";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.CheckMsg4))
                ApplicationData.Current.LocalSettings.Values[Constants.CheckMsg3] = "CheckMsg4";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.StartupMsg))
                ApplicationData.Current.LocalSettings.Values[Constants.StartupMsg] = "StartupMsg";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.LastDBTime_Year))
            {
                SYSTEMTIME localTime;
                NativeTimeMethods.GetLocalTime(out localTime);
                DateTime t = localTime.ToDateTime();
                ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Year] = t.Year;
                ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Month] = t.Month;
                ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Day] = t.Day;
                ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Hour] = t.Hour;
                ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Min] = t.Minute;
                ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Sec] = t.Second;
            }

            // Forklift Activate Signal off
            GpioPin ActiveSignalPin = GpioController.GetDefault().OpenPin(Constants.ForkActivePIN);
            ActiveSignalPin.SetDriveMode(GpioPinDriveMode.Output);
            ActiveSignalPin.Write(GpioPinValue.Low);

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                rootFrame.Navigate(typeof(MainPage), e.Arguments);

                /*
                //#if !FORCE_OOBE_WELCOME_SCREEN
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.HasDoneOOBEKey))
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                else
                //#endif
                {
                    rootFrame.Navigate(typeof(OOBEWelcome), e.Arguments);
                }
                */
            }
            // Ensure the current window is active
            Window.Current.Activate();

            Screensaver.InitializeScreensaver();

            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromSeconds(60));

        }

        private async void Timer_Tick(ThreadPoolTimer timer)
        {
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime LastSavedTime = new DateTime(Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Year]), Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Month]),
                Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Day]), Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Hour]),
                Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Min]), Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Sec]));

            DateTime t = localTime.ToDateTime();

            if (((t.Hour == 6 && t.Minute == 0) || (t.Hour == 14 && t.Minute == 0)) || (t.Hour == 22 && t.Minute == 0))
            {
                TimeSpan interval = new TimeSpan(0, 5, 0); // 5 minutes interval
                if ((t - LastSavedTime) >= interval)
                {
                    String strDateTime;
                    strDateTime = t.Year.ToString("00");
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

                    ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Year] = t.Year;
                    ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Month] = t.Month;
                    ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Day] = t.Day;
                    ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Hour] = t.Hour;
                    ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Min] = t.Minute;
                    ApplicationData.Current.LocalSettings.Values[Constants.LastDBTime_Sec] = t.Second;

                    string strMsg = "Shifting Event" + Environment.NewLine;

                    strMsg += "Alarm Reset Count:";
                    strMsg += Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmCnt]).ToString();
                    strMsg += ".";
                    strMsg += Environment.NewLine;

                    bool bRes = await ResetAlarm(true);
                    await SaveMessageIntoDB(strDateTime, strMsg);
                    if(bLogged)
                    {
                        strMsg = "Activating Segment Ended - User:" + UserInfo.UserName;
                        strMsg += Environment.NewLine;
                        strMsg += "#";
                        strMsg += UserInfo.LogonTime;
                        strMsg += "#";
                        strMsg += strDateTime;
                        strMsg += "#";
                        strMsg += Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.RunningTime]).ToString();
                        UserInfo.LastLogTime = t;
                        UserInfo.LogonTime = strDateTime;
                        await App.SaveMessageIntoDB(strDateTime, strMsg);
                    }
                    ApplicationData.Current.LocalSettings.Values[Constants.RunningTime] = 0;
                }
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            // Spot if we are being activated due to inbound pairing request
            if (args.Kind == ActivationKind.DevicePairing)
            {
                // Ensure the main app loads first
                OnLaunched(null);

                // Get the arguments, which give information about the device which wants to pair with this app
                var devicePairingArgs = (DevicePairingActivatedEventArgs)args;
                var di = devicePairingArgs.DeviceInformation;

                // Automatically switch to Bluetooth Settings page
                NavigationUtils.NavigateToScreen(typeof(Settings));

                int bluetoothSettingsIndex = 2;
                Frame rootFrame = Window.Current.Content as Frame;
                ListView settingsListView = null;
                settingsListView = (rootFrame.Content as FrameworkElement).FindName("SettingsChoice") as ListView;
                settingsListView.Focus(FocusState.Programmatic);
                bluetoothSettingsIndex = Math.Min(bluetoothSettingsIndex, settingsListView.Items.Count - 1);
                settingsListView.SelectedIndex = bluetoothSettingsIndex;
                // Appropriate Bluetooth Listview grid content is forced by App_InboundPairingRequested call to SwitchToSelectedSettings

                // Fire the event letting subscribers know there's a new inbound request.
                // In this case Scenario should be subscribed.
                if (InboundPairingRequested != null)
                {
                    InboundPairingEventArgs inboundEventArgs = new InboundPairingEventArgs(di);
                    InboundPairingRequested(this, inboundEventArgs);
                }
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
