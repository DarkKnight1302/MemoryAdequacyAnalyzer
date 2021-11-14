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
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using Hangfire.Annotations;

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
                if (Dispatcher == null) // For console App
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    if (Dispatcher.HasThreadAccess)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                    else
                    {
                        _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
                    }
                }
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
        public string ChartVisibility {  get { return chartVisibility;  } set { chartVisibility = value; OnPropertyChanged(); } }

        private DataReaderWriter drwObj = DataReaderWriter.Instance;
        private string reportVisibility="Collapsed";
        private string currentStatus = "";
        private string progressMsg = "";
        private string progressRingVisibility = "Collapsed";
        private string isRamUpgradeRequired = "Yes";
        private string recommendedRamSize = "";
        private int analysingSince = 0;
        private string pageFaultProcess = "";
        private string chartVisibility = "Visible";

        public MainPage()
        {
            this.InitializeComponent();
            _ = ShowDashboard().ConfigureAwait(false);
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
                if (dataList != null)
                {
                    (LineChart.Series[0] as LineSeries).ItemsSource = dataList;
                    // Todo: Uncomment it if we want to be in Gb.
                    // List<DataModel> tempList = ConvertToGb(dataList);
                    (LineChart1.Series[0] as LineSeries).ItemsSource = dataList;
                    ChartVisibility = "Visible";
                } 
                else
                {
                    ChartVisibility = "Collapsed";
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private List<DataModel> ConvertToGb(List<DataModel> dataList)
        {
            ulong bytesInGb = 1073741824;
            List<DataModel> tempList = new List<DataModel>();
            foreach (DataModel currEntry in dataList)
            {
                currEntry.PagedMemorySizeInBytes = currEntry.PagedMemorySizeInBytes / bytesInGb;
                tempList.Add(currEntry);
            }

            return tempList;
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
        /// Mouse Pointer styling when enter the button section.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        /// <summary>
        /// Mouse pointer styling when exit the button section.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
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
                    ShowProgressRing(true);
                    AnalysisResponse response = await AnalyzerService.GetInstance().AnalyzeData();
                    if ( response != null )
                    {
                        PopulatePerformanceReport(response);
                    }
                    await Task.Delay(5000);
                    await ShowToastNotification("Report Generated", "Voila!! Your report is ready to be viewed :-)");
                    ShowProgressRing(false);
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

        private async void ShowDashBoardBetweenDates_Handler(object sender, RoutedEventArgs e)
        {
            DateTime startDate = DateTime.Parse(StartDate.Date.ToString());
            DateTime endDate = DateTime.Parse(EndDate.Date.ToString());

            // no selction of any date.
            if (startDate.Year == 1601 || endDate.Year == 1601)
            {
                var msg = new MessageDialog("Please select valid dates");
                await msg.ShowAsync();
                return;
            }

            if (DateTime.Compare(startDate, endDate) > 0)
            {
                var msg = new MessageDialog("Start Date can't be greater than End Date");
                await msg.ShowAsync();
                return;
            }

            var tempList = await drwObj.ReadData(startDate, endDate);
            if (tempList.Count == 0)
            {
                var msg = new MessageDialog("No any data between selected date. Hence defaulting to till latest date");
                _ = await ShowDashboard();
                await msg.ShowAsync();
                return;
            }
            else
            {
                List<DataModel> dataList = tempList;
                LoadChartContent(dataList);
                return;
            }
        }
    }
}
