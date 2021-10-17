
namespace CommonLib.Models
{
    public class AnalysisResponse
    {
        private static Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private static Windows.Storage.ApplicationDataContainer container =
          localSettings.CreateContainer("settings", Windows.Storage.ApplicationDataCreateDisposition.Always);

        public bool IsRamUpgradeNeeded {  get; set; }

        public double RecommendedRamSize {  get; set; }

        public int AnalysisHours {  get; set; }

        public uint MaximumPageFaults 
        {
            get { return (uint)container.Values["maxPageFault"]; }
        }

        public string MaximumPageFaultsProcessName
        {
            get { return (string)container.Values["maxPageFaultProcess"]; }
        }

    }
}
