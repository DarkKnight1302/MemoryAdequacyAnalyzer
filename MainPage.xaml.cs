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
        private DataReaderWriter drwObj;


        public MainPage()
        {
            this.InitializeComponent();
            this.drwObj = new DataReaderWriter();
        }

        private void StartAnalysing_Handler(object sender, RoutedEventArgs e)
        {
            this.drwObj.WriteData(new DataModel(3, "teams", 4));
        }

        private void StopAnalysing_Handler(object sender, RoutedEventArgs e)
        {

        }
    }
}
