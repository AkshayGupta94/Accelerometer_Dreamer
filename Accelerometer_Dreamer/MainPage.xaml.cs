using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Accelerometer_Dreamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Accelerometer _accelerometer;
        private Gyrometer _gyrometer;
        List<string> AccX = new List<string>();
        List<string> AccY = new List<string>();
        List<string> AccZ = new List<string>();
        List<string> GyrX = new List<string>();
        List<string> GyrY = new List<string>();
        List<string> GyrZ = new List<string>();

        bool check = false;
        public MainPage()
        {
            this.InitializeComponent();
            _accelerometer = Accelerometer.GetDefault();
            _gyrometer = Gyrometer.GetDefault();
            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                // Assign an event handler for the reading-changed event
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }

            if(_gyrometer != null)
            {
                uint minGyro = _gyrometer.MinimumReportInterval;
                uint reportGyro = minGyro > 16 ? minGyro : 16;
                _gyrometer.ReportInterval = reportGyro;

                _gyrometer.ReadingChanged += new TypedEventHandler<Gyrometer, GyrometerReadingChangedEventArgs>(ReadingChanged);

            }
        }

        async private void ReadingChanged(object sender, GyrometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GyrometerReading reading = e.Reading;
                ScenarioOutput_X.Text = "Gyro_X =" + String.Format("{0,5:0.00000}", reading.AngularVelocityX);
                ScenarioOutput_Y.Text = "Gyro_Y =" + String.Format("{0,5:0.00000}", reading.AngularVelocityY);
                ScenarioOutput_Z.Text = "Gyro_Z =" + String.Format("{0,5:0.00000}", reading.AngularVelocityZ);
                if (check)
                {
                    GyrX.Add(String.Format("{0,5:0.00000}", reading.AngularVelocityX));
                    GyrY.Add(String.Format("{0,5:0.00000}", reading.AngularVelocityY));
                    GyrZ.Add(String.Format("{0,5:0.00000}", reading.AngularVelocityZ));
                }
            });
        }


        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;
                txtXAxis.Text = "Acce_X =" + String.Format("{0,5:0.00000}", reading.AccelerationX);
                txtYAxis.Text = "Acce_Y =" + String.Format("{0,5:0.00000}", reading.AccelerationY);
                txtZAxis.Text = "Acce_Z =" + String.Format("{0,5:0.00000}", reading.AccelerationZ);
                if (check)
                {
                    AccX.Add(String.Format("{0,5:0.00000}", reading.AccelerationX));
                    AccY.Add(String.Format("{0,5:0.00000}", reading.AccelerationY));
                    AccZ.Add(String.Format("{0,5:0.00000}", reading.AccelerationZ));
            }
            });
           
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            check = true;
            MessageDialog msgbox = new MessageDialog("Reading started");
            await msgbox.ShowAsync();
            // start recording the readings in the file 
        }

        private async void button_1_Click(object sender, RoutedEventArgs e)
        {

            check = false;
            MessageDialog msgbox = new MessageDialog("Reading stopped");
            await msgbox.ShowAsync();
            string file_save = ",";
            int i = AccX.Count > AccZ.Count ? AccZ.Count : AccX.Count;
            int j = GyrX.Count > GyrZ.Count ? GyrZ.Count : GyrX.Count;
            i = i > j ? j : i;
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            for (j=0;j<i;j++)
            {
                    file_save = file_save + AccX[j] + "," + AccY[j] + "," + AccZ[j] + "," + GyrX[j] + "," + GyrY[j] + "," + GyrZ[j] + ",";
            }

            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await Windows.Storage.FileIO.WriteTextAsync(file, file_save);
                Windows.Storage.Provider.FileUpdateStatus status =
          await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    msgbox = new MessageDialog("File " + file.Name + " was saved.");
                    await msgbox.ShowAsync(); 
                }
                else
                {
                    msgbox = new MessageDialog("File " + file.Name + " was NOT saved.");
                    await msgbox.ShowAsync();
                }

            }
    
            //Stop the reading
        }
    }
}
