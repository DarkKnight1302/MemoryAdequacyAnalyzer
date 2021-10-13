namespace MemoryAdequacyAnalyzer.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using System.IO;
    using MemoryAdequacyAnalyzer.Models;
    using System.Threading;
    using System.Diagnostics;

    public class DataReaderWriter
    {
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public static DataReaderWriter Instance { get; } = new DataReaderWriter();

        /// <summary>
        /// Return the data between timestamp t1 and t2.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public List<DataModel> ReadData(DateTime t1, DateTime t2)
        {
            return new List<DataModel>();
            // do nothing now..
        }

        // <summary>
        /// Return the data between timestamp t1 and t2.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public List<DataModel> ReadDataFromBeginning()
        {
            DateTime dateTime = DateTime.Now;
            return new List<DataModel>();
            // do nothing now..
        }

        /// <summary>
        /// Write data to the file.
        /// </summary>
        /// <param name="dm"> Object of DataModel class. </param>
        public async Task WriteData(DataModel dm)
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                StorageFile dataFile = await this.localFolder.CreateFileAsync("dataFile", CreationCollisionOption.OpenIfExists);
                var currentEntry = dm.CurrentTimeStamp.ToString() + "|" + dm.RamUsage + "|" + dm.PageFileSize + "|" + dm.PageFaultsPerMin +  "|" + Environment.NewLine;
                await FileIO.AppendTextAsync(dataFile, currentEntry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n" +  ex.StackTrace);
            } finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
