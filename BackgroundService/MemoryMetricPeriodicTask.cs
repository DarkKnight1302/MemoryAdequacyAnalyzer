using BackgroundService.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
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
        }

        private async Task RunCoreAsync(CancellationToken cancellation)
        {
            Process[] allProcess = Process.GetProcesses();
            uint totalPageFault = 0;
            ulong totalPageFileSize = 0;
            foreach (Process process in allProcess)
            {
                ProcessDiagnosticInfo diagnosticInfo = ProcessDiagnosticInfo.TryGetForProcessId((uint)process.Id);
                if (diagnosticInfo != null)
                {
                    ProcessMemoryUsageReport report = diagnosticInfo.MemoryUsage.GetReport();
                    totalPageFault += report.PageFaultCount;
                    totalPageFileSize += report.PageFileSizeInBytes;
                }
            }
            uint previousPageFault = (uint)container.Values["previousPageFault"];
            long previousTime = (long)container.Values["previousTime"];
            DateTime previousDateTime = DateTime.FromBinary(previousTime);
            container.Values["previousPageFault"] = totalPageFault;
            container.Values["previousTime"] = DateTime.Now.ToBinary();
            uint pageFaultDiff = totalPageFault > previousPageFault ? (uint)totalPageFault - previousPageFault : 0;
            uint minsDiff = (uint)DateTime.Now.Subtract(previousDateTime).Minutes;
            float pageFaultPerMin = (float)pageFaultDiff / (float)minsDiff; 
        }
    }
}
