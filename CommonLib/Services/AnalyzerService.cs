
using MemoryAdequacyAnalyzer.Models;
using System.Collections.Generic;

namespace CommonLib.Services
{
    public class AnalyzerService
    {
        private static AnalyzerService Instance;
        private static object instanceLock = new object();
        private const int RamUsagePercentThreshold = 70;
        private const long pageFaultPerMinThreshold = 18000; // 300 pages per sec.
        private const long pageFileSizeInBytesThreshold = 5368709120; // 5 gb.

        public static AnalyzerService GetInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }
            lock (instanceLock)
            {
                if (Instance == null)
                {
                    Instance = new AnalyzerService();
                }
                return Instance;
            }
        }

        public void AnalyzeData(List<DataModel> dataModelList)
        {
            int highRamCount = 0;
            int highPageFaultCount = 0;
            int highPageFileSizeCount = 0;
            foreach (DataModel dataModel in dataModelList)
            {
                if (dataModel.RamUsage > RamUsagePercentThreshold)
                {
                    highRamCount++;
                }
                if (dataModel.PageFileSize > pageFileSizeInBytesThreshold)
                {
                    highPageFileSizeCount++;
                }
                if (dataModel.PageFaultsPerMin >= pageFaultPerMinThreshold)
                {
                    highPageFaultCount++;
                }
            }
            int size = dataModelList.Count;
            double highRamCountPercent = (double)(highRamCount / size) * (double)100;
            double highPageFileSizePercent = (double)(highPageFileSizeCount / size) * (double)100 ;
            double highPageFaultCountPercent = (double)(highPageFaultCount / size) * (double)100 ;

            if (highRamCountPercent > 40 && highPageFileSizePercent > 50 && highPageFaultCountPercent > 50)
            {
                // recommend RAM increase.
            }
            // Not recommended to increase RAM.
        }
    }
}
