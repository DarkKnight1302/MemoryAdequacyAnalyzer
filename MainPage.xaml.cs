using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MemoryAdequacyAnalyzer.Utils;
using MemoryAdequacyAnalyzer.Models;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.ApplicationModel.Background;
using CommonLib.Services;
using CommonLib.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MemoryAdequacyAnalyzer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string RamUsage = "20Mb";
        public string PageFaultProcess = "Teams";
        public string IsRamUpgradeRequired = "Yes";
        public int AnalysingSince = 100;
        private DataReaderWriter drwObj = DataReaderWriter.Instance;


        public MainPage()
        {
            this.InitializeComponent();
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ShowDashboard().ConfigureAwait(false));
            //CheckBackgroundTaskStarted();
        }

        /// <summary>
        /// Load the chart based on item source.
        /// </summary>
        /// <param name="dataList"></param>
        private void LoadChartContent(List<DataModel> dataList)
        {
            try
            {
                (LineChart.Series[0] as LineSeries).ItemsSource = dataList;
                (LineChart1.Series[0] as LineSeries).ItemsSource = dataList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void CheckBackgroundTaskStarted()
        {
            if (started)
            {
                // If anything to be done.
            }
        }

        public bool started
        {
            get { return BackgroundTaskRegistration.AllTasks.Count > 0; }
        }



        /// <summary>
        /// Show the dashboard in Main Page View.
        /// </summary>
        /// <returns></returns>
        private async Task<object> ShowDashboard()
        {
            var tempList = await drwObj.ReadDataFromBeginning();
            List<DataModel> dataList = tempList;
            LoadChartContent(dataList);
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Start the background task if not started.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartAnalysing_Handler(object sender, RoutedEventArgs e)
        {
            try
            {
                BackgroundService.MemoryMetricPeriodicTask.Register();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Stop the background task if running.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopAnalysing_Handler(object sender, RoutedEventArgs e)
        {
            try
            {
                var task = BackgroundTaskRegistration.AllTasks.Values.First();
                if (task.Name == "MemoryMetricPeriodicTask")
                {
                    task.Unregister(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generate the report after reading the data file.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">e</param>
        private void GenerateReport_Handler(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                AnalysisResponse response = await AnalyzerService.GetInstance().AnalyzeData();
                await ShowToastNotification("Report Generated", "Voila!! Your report is ready to be viewed :-)");
            });
        }

        private async Task<object> ShowToastNotification(string title, string stringContent)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            // call the actual algorithm
            await Task.Delay(3000);
            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(4);
            ToastNotifier.Show(toast);
            return Task.FromResult<object>(null);
        }
    }
}
