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
using Windows.Media.Capture;

using System.Threading;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using System.Linq;
using Windows.System.Display;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTCoreDefaultApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CameraActivity_335 : Page
    {
        private DispatcherTimer DateTimetimer;

        // 
        private MediaCapture mediaCapture;

        private readonly DisplayRequest _displayRequest = new DisplayRequest();
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
                CameraToggle.IsOn = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey]);
            });
        }
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (DateTimetimer.IsEnabled)
                DateTimetimer.Stop();
            DateTimetimer = null;
        }

        public CameraActivity_335()
        {
            this.InitializeComponent();
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.EnableCamKey))
                ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey] = false;
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
            if (mediaCapture != null)
            {
                try
                {
                    await mediaCapture.StopPreviewAsync();
                    mediaCapture.Dispose();
                    mediaCapture = null;
                    previewElement.Source = null;
                    _displayRequest.RequestRelease();
                }
                catch (Exception ex)
                {

                }
            }
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigationUtils.NavigateToScreen(typeof(AdminMain_310));
            });
        }

        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    /*
                    status.Text = "MediaCaptureFailed: " + currentFailure.Message;
                    */
                }
                catch (Exception)
                {
                }
                finally
                {
                    /*
                    SetInitButtonVisibility(Action.DISABLE);
                    SetVideoButtonVisibility(Action.DISABLE);
                    SetAudioButtonVisibility(Action.DISABLE);
                    status.Text += "\nCheck if camera is diconnected. Try re-launching the app";
                    */
                }
            });
        }
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private async Task UpdateCamPreview()
        {
            try
            {
                if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey]))
                {
                    // Set callbacks for failure and recording limit exceeded
                    if (mediaCapture == null)
                    {
                        var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);

                        if (cameraDevice == null)
                        {
                            return;
                        }

                        mediaCapture = new MediaCapture();

                        mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                        //mediaCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);
                        var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                        await mediaCapture.InitializeAsync(settings);

                        if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey]) == true)
                        {
                            _displayRequest.RequestActive();
                            previewElement.Source = mediaCapture;
                            await mediaCapture.StartPreviewAsync();
                        }
                    }
                }
                else
                {
                    if (mediaCapture != null)
                    {
                        await mediaCapture.StopPreviewAsync();
                        mediaCapture.Dispose();
                        mediaCapture = null;
                        previewElement.Source = null;
                        _displayRequest.RequestRelease();
                    }
                }
            }
            catch (Exception ex)
            {
                //status.Text = "Unable to initialize camera for audio/video mode: " + ex.Message;
                CameraStatus.Text = "I found fault in camera, You have to check whether camera is connected or damage!";
            }
        }

        private async void CameraToggle_Toggled(object sender, RoutedEventArgs e)
        {
            Back.IsEnabled = false;
            CameraToggle.IsEnabled = false;
            ApplicationData.Current.LocalSettings.Values[Constants.EnableCamKey] = CameraToggle.IsOn;
            await UpdateCamPreview();
            Back.IsEnabled = true;
            CameraToggle.IsEnabled = true;
        }
    }
}
