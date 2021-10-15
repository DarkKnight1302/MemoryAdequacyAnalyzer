using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MemoryAdequacyAnalyzer.Utils;
using MemoryAdequacyAnalyzer.Models;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using System.Threading;

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
        }

        private async void StartAnalysing_Handler(object sender, RoutedEventArgs e)
        {
          //await this.drwObj.WriteData(new DataModel(DateTime.Now, 3, 5, 4));
        }

        private void StopAnalysing_Handler(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateReport_Handler(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => await ShowToastNotification("Report Generated", "Voila!! Your report is ready to be viewed :-)"));
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
