using BackgroundService.Lib;
using MemoryAdequacyAnalyzer.Models;
using MemoryAdequacyAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System;
using Windows.System.Diagnostics;

namespace BackgroundService
{
    public sealed class MemoryMetricPeriodicTask : IBackgroundTask
    {
        private static Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

       private static Windows.Storage.ApplicationDataContainer container =
           localSettings.CreateContainer("settings", Windows.Storage.ApplicationDataCreateDisposition.Always);

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            await BackgroundTaskUtil.RunBackgroundTaskAsync(taskInstance, this.RunCoreAsync).ConfigureAwait(false);
        }

        public static void Register()
        {
            if (BackgroundTaskRegistration.AllTasks.Any(
                r => string.Equals(r.Value.Name, nameof(MemoryMetricPeriodicTask), StringComparison.Ordinal)))
            {
                // Background task is already registered
                return;
            }

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder()
            {
                Name = nameof(MemoryMetricPeriodicTask),
                TaskEntryPoint = typeof(MemoryMetricPeriodicTask).FullName,
            };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            taskBuilder.Register();
        }

        private async Task RunCoreAsync(CancellationToken cancellation)
        {
            await AppDiagnosticInfo.RequestAccessAsync();
            IReadOnlyList<ProcessDiagnosticInfo> diagnosticInfos = ProcessDiagnosticInfo.GetForProcesses();
            SystemDiagnosticInfo systemdiagnosticInfo =  SystemDiagnosticInfo.GetForCurrentSystem();
            SystemMemoryUsageReport usageReport = systemdiagnosticInfo.MemoryUsage.GetReport();
            double ramUsagePercent = (double)100 - (((double)usageReport.AvailableSizeInBytes / (double)usageReport.CommittedSizeInBytes) * 100);
            uint totalPageFault = 0;
            ulong totalPageFileSize = 0;
            foreach (ProcessDiagnosticInfo process in diagnosticInfos)
            {
                if (process != null)
                {
                    ProcessMemoryUsageReport report = process.MemoryUsage.GetReport();
                    totalPageFault += report.PageFaultCount;
                    totalPageFileSize += report.PageFileSizeInBytes;
                    if (report.PageFaultCount > (uint)(container.Values["maxPageFault"] ?? (uint)0))
                    {
                        container.Values["maxPageFault"] = (uint)report.PageFaultCount;
                        container.Values["maxPageFaultProcess"] = process.ExecutableFileName;
                    }
                }
            }
            uint previousPageFault = (uint)(container.Values["previousPageFault"] ?? (uint)0);
            long previousTime = (long)(container.Values["previousTime"] ?? (long)0);
            container.Values["previousPageFault"] = totalPageFault;
            container.Values["previousTime"] = DateTime.Now.ToBinary();
            if (previousTime != 0)
            {
                DateTime previousDateTime = DateTime.FromBinary(previousTime);
                uint pageFaultDiff = totalPageFault > previousPageFault ? (uint)totalPageFault - previousPageFault : 0;
                uint minsDiff = (uint)DateTime.Now.Subtract(previousDateTime).Minutes;
                float pageFaultPerMin = (float)pageFaultDiff / (float)minsDiff;
                DataReaderWriter readerWriter = DataReaderWriter.Instance;
                DataModel dataModel = new DataModel
                {
                    CurrentTimeStamp = DateTime.Now,
                    RamUsage = (int)ramUsagePercent,
                    PageFaultsPerMin = (int)pageFaultPerMin,
                    PageFileSize = totalPageFileSize,
                };
                await readerWriter.WriteData(dataModel).ConfigureAwait(false);
            }
        }
    }
}
