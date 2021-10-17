
using CommonLib.Models;
using MemoryAdequacyAnalyzer.Models;
using MemoryAdequacyAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private AnalyzerService()
        {
        }

        public async Task<AnalysisResponse> AnalyzeData()
        {
            DataReaderWriter dataReaderWriter = DataReaderWriter.Instance;
            List<DataModel> dataModelList = await dataReaderWriter.ReadDataFromBeginning().ConfigureAwait(false);
            int highRamCount = 0;
            int highPageFaultCount = 0;
            int highPageFileSizeCount = 0;
            ulong TotalVirtualMemoryInGb = 0;
            ulong bytesInGb = 1073741824;
            ulong virtualMemoryCount = 0;
            DateTime start = dataModelList[0].CurrentTimeStamp;
            DateTime end = dataModelList[dataModelList.Count - 1].CurrentTimeStamp;
            foreach (DataModel dataModel in dataModelList)
            {
                int conditionCheckCount = 0;
                if (dataModel.RamUsage > RamUsagePercentThreshold)
                {
                    highRamCount++;
                    conditionCheckCount++;
                }
                if (dataModel.PageFileSize > pageFileSizeInBytesThreshold)
                {
                    highPageFileSizeCount++;
                    conditionCheckCount++;
                }
                if (dataModel.PageFaultsPerMin >= pageFaultPerMinThreshold)
                {
                    highPageFaultCount++;
                    conditionCheckCount++;
                }
                if (conditionCheckCount == 3)
                {
                    TotalVirtualMemoryInGb += (ulong)(dataModel.VirtualMemorySizeInBytes / bytesInGb);
                    virtualMemoryCount++;
                }
            }
            int size = dataModelList.Count;
            double averageVirtualMemory = (double)TotalVirtualMemoryInGb / (double)virtualMemoryCount;
            double highRamCountPercent = (double)(highRamCount / size) * (double)100;
            double highPageFileSizePercent = (double)(highPageFileSizeCount / size) * (double)100 ;
            double highPageFaultCountPercent = (double)(highPageFaultCount / size) * (double)100 ;

            if (highRamCountPercent > 30 && highPageFileSizePercent > 30 && highPageFaultCountPercent > 30)
            {
                return new AnalysisResponse
                {
                    IsRamUpgradeNeeded = true,
                    RecommendedRamSize = averageVirtualMemory,
                    AnalysisHours = end.Subtract(start).Hours,
                };
            }
            return new AnalysisResponse
            {
                IsRamUpgradeNeeded = false,
                AnalysisHours = end.Subtract(start).Hours,
            };
        }
    }
}
