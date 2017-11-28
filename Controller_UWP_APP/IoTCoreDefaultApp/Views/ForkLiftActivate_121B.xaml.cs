using IoTCoreDefaultApp.Utils;
using System;
using System.Globalization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using System.Threading;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

using Windows.Devices.I2c;

using Windows.Devices.Enumeration;
using System.Linq;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    struct Acceleration
    {
        public double X;
        public double Y;
        public double Z;
    };
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ForkLiftActivate_121B : Page
    {
        private const byte ACCEL_I2C_ADDR = 0x53;           /* 7-bit I2C address of the ADXL345 with SDO pulled low */
        private const byte ACCEL_REG_POWER_CONTROL = 0x2D;  /* Address of the Power Control register */
        private const byte ACCEL_REG_DATA_FORMAT = 0x31;    /* Address of the Data Format register   */
        private const byte ACCEL_REG_X = 0x32;              /* Address of the X Axis data register   */
        private const byte ACCEL_REG_Y = 0x34;              /* Address of the Y Axis data register   */
        private const byte ACCEL_REG_Z = 0x36;              /* Address of the Z Axis data register   */

        private I2cDevice I2CAccel;

        private DispatcherTimer DateTimetimer;
        private DispatcherTimer AutoLogTimer;
        private DispatcherTimer RunningCountTimer;
        private DispatcherTimer periodicTimer;

        private GpioPin RunningPin;
        private GpioPin ActiveSignalPin;
        private GpioPin AccelCSPin;

        private MediaCapture mediaCapture;
        private readonly string PHOTO_FILE_NAME = "photo.jpg";

        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Handling of this event is included for completenes, as it will only fire when navigating between pages and this sample only includes one page
            DateTimetimer.Stop();
            DateTimetimer = null;

            if (AutoLogTimer.IsEnabled)
                AutoLogTimer.Stop();
            AutoLogTimer = null;

            if (RunningCountTimer.IsEnabled)
                RunningCountTimer.Stop();
            RunningCountTimer = null;
            if(ActiveSignalPin != null)
            {
                ActiveSignalPin.Write(GpioPinValue.Low);
                ActiveSignalPin.Dispose();
            }

            if(RunningPin != null)
                RunningPin.Dispose();
            if (AccelCSPin != null)
                AccelCSPin.Dispose();
            if (periodicTimer != null)
            {
                if (periodicTimer.IsEnabled)
                    periodicTimer.Stop();
            }

            if (I2CAccel != null)
                I2CAccel.Dispose();

            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);
            DateTime t = localTime.ToDateTime();
            string strDateTime;
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
            UserInfo.LogoffTime = strDateTime;

            String strMsg = "ForkLisft Logout Event, User:";
            strMsg += UserInfo.UserName;
            strMsg += Environment.NewLine;
            bool bRes = false;
            bRes = await App.SaveMessageIntoDB(strDateTime, strMsg);

            strMsg = "Activating Segment Ended - User:" + UserInfo.UserName;
            strMsg += Environment.NewLine;
            strMsg += "#";
            strMsg += UserInfo.LogonTime;
            strMsg += "#";
            strMsg += UserInfo.LogoffTime;
            strMsg += "#";
            strMsg += Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.RunningTime]).ToString();
            ApplicationData.Current.LocalSettings.Values[Constants.RunningTime] = 0;
            UserInfo.LastLogTime = t;
            App.bLogged = false;
            if(bRes)
                bRes = await App.SaveMessageIntoDB(strDateTime, strMsg);
            else
                bRes = await App.InsertIntoSQLite(strDateTime, strMsg);

        }

        public ForkLiftActivate_121B()
        {
            this.InitializeComponent();
        }

        private void RunningCountTick(object sender, object e)
        { // Every Half Minute
            Int32 nRuntimeCount = Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[Constants.RunningTime]);
            nRuntimeCount++;
            ApplicationData.Current.LocalSettings.Values[Constants.RunningTime] = nRuntimeCount;

            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);
            DateTime t = localTime.ToDateTime();
            UserInfo.LastLogTime = t;
        }

        private async Task InitI2CAccel()
        {
            var settings = new I2cConnectionSettings(ACCEL_I2C_ADDR);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            var controller = await I2cController.GetDefaultAsync();
            I2CAccel = controller.GetDevice(settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
            periodicTimer = new DispatcherTimer();
            periodicTimer.Tick += TimerCallback;
            periodicTimer.Interval = TimeSpan.FromMilliseconds(500);

            /* 
             * Initialize the accelerometer:
             *
             * For this device, we create 2-byte write buffers:
             * The first byte is the register address we want to write to.
             * The second byte is the contents that we want to write to the register. 
             */
            byte[] WriteBuf_DataFormat = new byte[] { ACCEL_REG_DATA_FORMAT, 0x01 };        /* 0x01 sets range to +- 4Gs                         */
            byte[] WriteBuf_PowerControl = new byte[] { ACCEL_REG_POWER_CONTROL, 0x08 };    /* 0x08 puts the accelerometer into measurement mode */

            /* Write the register settings */
            try
            {
                I2CAccel.Write(WriteBuf_DataFormat);
                I2CAccel.Write(WriteBuf_PowerControl);
            }
            /* If the write fails display the error and stop running */
            catch (Exception ex)
            {
                CameraStatus.Text = "Failed to communicate with device: " + ex.Message;
                CameraStatus.Visibility = Visibility.Visible;
                return;
            }

            /* Now that everything is initialized, create a timer so we read data every 100mS */
            ActiveSignalPin.SetDriveMode(GpioPinDriveMode.Output);
            ActiveSignalPin.Write(GpioPinValue.High);

            periodicTimer.Start();
        }

        private async void TimerCallback(object sender, object e)
        {
            /* Read and format accelerometer data */
            try
            {
                // Tilt Checking
                //if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EanbleTiltKey]) && RunningCountTimer.IsEnabled)
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EanbleTiltKey]))
                {
                    Acceleration accel = ReadI2CAccel();
                    int nTiltSensityValue = Convert.ToInt16(ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey]);
                    if (Math.Abs(accel.X) >= nTiltSensityValue || Math.Abs(accel.Y) >= nTiltSensityValue)
                    {
                        if (periodicTimer.IsEnabled)
                            periodicTimer.Stop();
                        ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated] = true;
                        await AlarmProc();
                    }
                }
            }
            catch (Exception ex)
            {
                CameraStatus.Text = "Failed to read from Accelerometer: " + ex.Message;
                CameraStatus.Visibility = Visibility.Visible;
            }
        }

        private Acceleration ReadI2CAccel()
        {
            const int ACCEL_RES = 1024;         /* The ADXL345 has 10 bit resolution giving 1024 unique values                     */
            const int ACCEL_DYN_RANGE_G = 8;    /* The ADXL345 had a total dynamic range of 8G, since we're configuring it to +-4G */
            const int UNITS_PER_G = ACCEL_RES / ACCEL_DYN_RANGE_G;  /* Ratio of raw int values to G units                          */

            byte[] RegAddrBuf = new byte[] { ACCEL_REG_X }; /* Register address we want to read from                                         */
            byte[] ReadBuf = new byte[6];                   /* We read 6 bytes sequentially to get all 3 two-byte axes registers in one read */

            /* 
             * Read from the accelerometer 
             * We call WriteRead() so we first write the address of the X-Axis I2C register, then read all 3 axes
             */
            I2CAccel.WriteRead(RegAddrBuf, ReadBuf);

            /* 
             * In order to get the raw 16-bit data values, we need to concatenate two 8-bit bytes from the I2C read for each axis.
             * We accomplish this by using the BitConverter class.
             */
            short AccelerationRawX = BitConverter.ToInt16(ReadBuf, 0);
            short AccelerationRawY = BitConverter.ToInt16(ReadBuf, 2);
            short AccelerationRawZ = BitConverter.ToInt16(ReadBuf, 4);

            /* Convert raw values to G's */
            Acceleration accel;
            accel.X = ((double)AccelerationRawX / UNITS_PER_G) * (double)100;
            accel.Y = ((double)AccelerationRawY / UNITS_PER_G) * (double)100;
            accel.Z = ((double)AccelerationRawZ / UNITS_PER_G) * (double)100;

            return accel;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated]) == true)
            { // Tilt Alarm Check
                ApplicationData.Current.LocalSettings.Values[Constants.TiltAlarmActivated] = true;
                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    NavigationUtils.NavigateToScreen(typeof(TiltAlarmScreen_121C));
                });
            }
            else
            {
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

                UserInfo.LogonTime = strDateTime;

                string strMsg = "ForkLift Activating Event-User:" + UserInfo.UserName;
                strMsg += Environment.NewLine;

                strMsg += FlashSettings.CheckListMsg1;
                strMsg += ":";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList1]))
                    strMsg += "Yes, ";
                else
                    strMsg += "No, ";

                strMsg += FlashSettings.CheckListMsg2;
                strMsg += ":";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList2]))
                    strMsg += "Yes, ";
                else
                    strMsg += "No, ";

                strMsg += FlashSettings.CheckListMsg3;
                strMsg += ":";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList3]))
                    strMsg += "Yes, ";
                else
                    strMsg += "No, ";

                strMsg += FlashSettings.CheckListMsg4;
                strMsg += ":";
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.CheckList4]))
                    strMsg += "Yes";
                else
                    strMsg += "No";

                strMsg += Environment.NewLine;

                bool bRes = false;
                bRes = await App.SaveMessageIntoDB(strDateTime, strMsg);
                App.bLogged = true;

                var GPIO = GpioController.GetDefault();
                //
                if (GPIO == null)
                {
                    RunningPin = null;
                    ActiveSignalPin = null;
                    AccelCSPin = null;
                }
                else
                {
                    await UpdateDateTime();

                    await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));
                        StatusTxt.Text = FlashSettings.StartupMsg;
                        StatusTxt.Text += Environment.NewLine;
                        StatusTxt.Text += " ";
                        StatusTxt.Text += Environment.NewLine;
                        StatusTxt.Text += " - Log on time: ";
                        StatusTxt.Text += UserInfo.LogonTime;
                        StatusTxt.Text += Environment.NewLine;
                        StatusTxt.Text += " - User Name: ";
                        StatusTxt.Text += UserInfo.UserName;
                        StatusTxt.Text += Environment.NewLine;
                        StatusTxt.Text += " - User ID: ";
                        StatusTxt.Text += UserInfo.UserID.ToString();
                    });

                    AutoLogTimer = new DispatcherTimer();
                    AutoLogTimer.Tick += AutoLogOff;
                    AutoLogTimer.Interval = TimeSpan.FromSeconds(300);
                    AutoLogTimer.Start();

                    // Port initializing
                    RunningPin = GPIO.OpenPin(Constants.ForkRunningPIN);
                    ActiveSignalPin = GPIO.OpenPin(Constants.ForkActivePIN);
                    AccelCSPin = GPIO.OpenPin(Constants.ACCELCSPIN);

                    RunningPin.SetDriveMode(GpioPinDriveMode.InputPullDown);
                    RunningPin.DebounceTimeout = TimeSpan.FromMilliseconds(500);
                    RunningPin.ValueChanged += RunningChanged;

                    AccelCSPin.SetDriveMode(GpioPinDriveMode.Output);
                    AccelCSPin.Write(GpioPinValue.High);

                    RunningCountTimer = new DispatcherTimer();
                    RunningCountTimer.Tick += RunningCountTick;
                    RunningCountTimer.Interval = TimeSpan.FromSeconds(1);
                    RunningCountTimer.Stop();

                    DateTimetimer = new DispatcherTimer();
                    DateTimetimer.Tick += DateTime_Tick;
                    DateTimetimer.Interval = TimeSpan.FromSeconds(2);
                    DateTimetimer.Start();

                    await InitI2CAccel();
                }
            }
            LogoffButton.IsEnabled = true;
            BackButton.IsEnabled = true;
        }

        private async Task AlarmProc()
        {
            string strFileContent = "";
            try
            {
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey]) == true)
                {
                    var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);
                    if (cameraDevice == null)
                    {
                    }
                    else
                    {
                        mediaCapture = new MediaCapture();
                        var settings = new MediaCaptureInitializationSettings
                        {
                            StreamingCaptureMode = StreamingCaptureMode.Video,
                            VideoDeviceId = cameraDevice.Id,
                            PhotoCaptureSource = PhotoCaptureSource.VideoPreview,
                            AudioDeviceId = string.Empty
                        };
                        await mediaCapture.InitializeAsync(settings);
                        StorageFile photoFile;
                        photoFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(
                            PHOTO_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                        ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                        await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

                        IBuffer buffer = await FileIO.ReadBufferAsync(photoFile);
                        var bytes = new byte[buffer.Length];
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            dataReader.ReadBytes(bytes);
                        }
                        strFileContent = "0x" + CryptographicBuffer.EncodeToHexString(buffer);
                        mediaCapture.Dispose();
                        mediaCapture = null;
                    }
                }
            }
            catch (Exception ex)
            {
                CameraStatus.Text = "I found fault in camera, You have to check whether camera is connected or damage!";
            }

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

            string strMsg = "Tilt alarm happened-User:" + UserInfo.UserName;
            strMsg += Environment.NewLine;
            bool bRes = false;
            bRes = await App.SaveMessageIntoDB(strDateTime, strMsg, strFileContent);

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(TiltAlarmScreen_121C));
            });
        }

        private void RunningChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if(args.Edge == GpioPinEdge.RisingEdge)
                {
                    AutoLogTimer.Stop();
                    if (!RunningCountTimer.IsEnabled)
                        RunningCountTimer.Start();
                }
                else if(args.Edge == GpioPinEdge.FallingEdge)
                {
                    AutoLogTimer.Start();
                    if (RunningCountTimer.IsEnabled)
                        RunningCountTimer.Stop();
                }
            });
        }
        private async void DateTime_Tick(object sender, object e)
        {
            GpioPinValue Value = RunningPin.Read();
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (Value == GpioPinValue.High)
                    ForkRunning.IsChecked = true;
                else
                    ForkRunning.IsChecked = false;
            });

            await UpdateDateTime();
        }

        private async void AutoLogOff(object sender, object e)
        { // Auto Log off Proces
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
            });
        }

        private async Task UpdateDateTime()
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime t = localTime.ToDateTime();

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
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

        private async void LogoffButton_Click(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(LoginScreen_10));
            });
        }
    }
}
