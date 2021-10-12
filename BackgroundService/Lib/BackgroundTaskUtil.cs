using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BackgroundService.Lib
{
    internal class BackgroundTaskUtil
    {
        /// <summary>
        /// Run the background task and then run common post background task functions (telemetry flush and SurfaceApp update)
        /// </summary>
        /// <param name="taskInstance">The background task instance data.</param>
        /// <param name="taskFunc">The background task code.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        internal static async Task RunBackgroundTaskAsync(
            IBackgroundTaskInstance taskInstance,
            Func<CancellationToken, Task> taskFunc)
        {
            int startTicks = Environment.TickCount;

            using (CancellationTokenSource cancellationSource = new CancellationTokenSource())
            {
                void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
                {
                    cancellationSource.Cancel();
                }

                BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
                try
                {
                    taskInstance.Canceled += TaskInstanceCanceled;
                    await taskFunc(cancellationSource.Token).ConfigureAwait(false);
                }
                finally
                {
                    taskInstance.Canceled -= TaskInstanceCanceled;
                    deferral.Complete();
                }
            }
        }

    }
}
