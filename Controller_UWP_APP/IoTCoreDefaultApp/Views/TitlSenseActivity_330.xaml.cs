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

using Windows.Devices.I2c;

using Windows.Devices.Gpio;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TitlSenseActivity_330 : Page
    {
        private const byte ACCEL_I2C_ADDR = 0x53;           /* 7-bit I2C address of the ADXL345 with SDO pulled low */
        private const byte ACCEL_REG_POWER_CONTROL = 0x2D;  /* Address of the Power Control register */
        private const byte ACCEL_REG_DATA_FORMAT = 0x31;    /* Address of the Data Format register   */
        private const byte ACCEL_REG_X = 0x32;              /* Address of the X Axis data register   */
        private const byte ACCEL_REG_Y = 0x34;              /* Address of the Y Axis data register   */
        private const byte ACCEL_REG_Z = 0x36;              /* Address of the Z Axis data register   */

        private I2cDevice I2CAccel;
        private DispatcherTimer periodicTimer;

        private GpioPin AccelCSPin;

        private DispatcherTimer DateTimetimer;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await UpdateDateTime();
            DateTimetimer = new DispatcherTimer();
            DateTimetimer.Tick += DateTime_Tick;
            DateTimetimer.Interval = TimeSpan.FromSeconds(5);
            DateTimetimer.Start();
            Object value = ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey];

            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                LogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.png"));
                TiltSenseSlide.Value = (double)Convert.ToInt16(value);
            });

            var GPIO = GpioController.GetDefault();
            //
            if (GPIO == null)
                AccelCSPin = null;
            else
            {
                AccelCSPin = GPIO.OpenPin(Constants.ACCELCSPIN);
                AccelCSPin.SetDriveMode(GpioPinDriveMode.Output);
                AccelCSPin.Write(GpioPinValue.High);
                InitI2CAccel();
            }
        }
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

            if(periodicTimer != null)
            {
                if (periodicTimer.IsEnabled)
                    periodicTimer.Stop();
            }
            if (DateTimetimer.IsEnabled)
                DateTimetimer.Stop();
            DateTimetimer = null;
            if (AccelCSPin != null)
                AccelCSPin.Dispose();
            if (I2CAccel != null)
                I2CAccel.Dispose();
        }

        public TitlSenseActivity_330()
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

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[Constants.TiltSensitiKey] = (int)TiltSenseSlide.Value;
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(AdminMain_310));
            });
        }

        private async void TiltSenseSlide_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                TiltSensePerc.Text = TiltSenseSlide.Value.ToString() + "%";
            });
        }

        private async void InitI2CAccel()
        {
            var settings = new I2cConnectionSettings(ACCEL_I2C_ADDR);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            var controller = await I2cController.GetDefaultAsync();
            I2CAccel = controller.GetDevice(settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */

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
                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    xTilt.Text = "X Axis Tilt Value : Device Communication Error!";
                    yTilt.Text = "Y Axis Tilt Value : Device Communication Error!";
                });
                return;
            }

            /* Now that everything is initialized, create a timer so we read data every 100mS */
            periodicTimer = new DispatcherTimer();
            periodicTimer.Tick += TimerCallback;
            periodicTimer.Interval = TimeSpan.FromMilliseconds(500);
            periodicTimer.Start();
        }

        private void TimerCallback(object sender, object e)
        {

            /* Read and format accelerometer data */
            try
            {
                // Tilt Checking
                Acceleration accel = ReadI2CAccel();
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    xTilt.Text = String.Format("X Axis: {0:F1}%", accel.X);
                    yTilt.Text = String.Format("Y Axis: {0:F1}%", accel.Y);
                });
            }
            catch (Exception ex)
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    xTilt.Text = "X Axis Tilt Value : Failed To Read Value!";
                    yTilt.Text = "Y Axis Tilt Value : Failed To Read Value!";
                });
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
    }
}
