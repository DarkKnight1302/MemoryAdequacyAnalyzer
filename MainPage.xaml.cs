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
using System.ComponentModel;
using System.Runtime.CompilerServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MemoryAdequacyAnalyzer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string RecommendedRamSize { get { return recommendedRamSize; } set { recommendedRamSize = value; OnPropertyChanged(); } }
        public string PageFaultProcess { get { return pageFaultProcess; } set { pageFaultProcess = value; OnPropertyChanged(); } }
        public string IsRamUpgradeRequired { get { return isRamUpgradeRequired; } set { isRamUpgradeRequired = value; OnPropertyChanged(); } }
        public int AnalysingSince { get { return analysingSince;  } set { analysingSince = value; OnPropertyChanged(); } }
        public string ReportVisibility { get { return reportVisibility; }  set { reportVisibility = value; OnPropertyChanged(); } }
        public string CurrentStatus { get { return currentStatus; } set { currentStatus = value; OnPropertyChanged(); } }
        public string ProgressMsg {  get { return progressMsg; }  set { progressMsg = value; OnPropertyChanged(); } }
        public string ProgressRingVisibility { get { return progressRingVisibility; } set { progressRingVisibility = value; OnPropertyChanged(); } }

        private DataReaderWriter drwObj = DataReaderWriter.Instance;
        private string reportVisibility="Collapsed";
        private string currentStatus = "";
        private string progressMsg = "";
        private string progressRingVisibility = "Collapsed";
        private string isRamUpgradeRequired = "Yes";
        private string recommendedRamSize = "";
        private int analysingSince = 0;
        private string pageFaultProcess = "";

        public MainPage()
        {
            this.InitializeComponent();
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ShowDashboard().ConfigureAwait(false));
            CheckBackgroundTaskStarted();
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
                CurrentStatus = "Background Task is Running ...";
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
                CurrentStatus = "Background Task is Running ...";
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
                this.CurrentStatus = "Background Process is Stopped ...";
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
                try
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>ShowProgressRing(true));
                    AnalysisResponse response = await AnalyzerService.GetInstance().AnalyzeData();
                    if ( response != null )
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PopulatePerformanceReport(response));
                    }
                    await Task.Delay(5000);
                    await ShowToastNotification("Report Generated", "Voila!! Your report is ready to be viewed :-)");
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ShowProgressRing(false));
                } 
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                
            });
            
        }

        private void PopulatePerformanceReport(AnalysisResponse response)
        {
            IsRamUpgradeRequired = response.IsRamUpgradeNeeded ? "Yes" : "No";
            RecommendedRamSize = response.RecommendedRamSize.ToString();
            PageFaultProcess = response.MaximumPageFaultsProcessName;
            AnalysingSince = response.AnalysisHours;
        }
        private void ShowProgressRing(bool needtoshow)
        {
            if(needtoshow)
            {
                ReportVisibility = "Collapsed";
                ProgressMsg = "Hold Tight.. !! We are working on generating Report";
                ProgressRingVisibility = "Visible";
            }
            else
            {
                ProgressRingVisibility = "Collapsed";
                ReportVisibility = "Visible";
            }
        }

        private Task<object> ShowToastNotification(string title, string stringContent)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");
            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(4);
            ToastNotifier.Show(toast);
            return Task.FromResult<object>(null);
        }
    }
}
