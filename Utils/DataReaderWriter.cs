namespace MemoryAdequacyAnalyzer.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using MemoryAdequacyAnalyzer.Models;
    using System.IO;

    public class DataReaderWriter
    {
        private StorageFile dataFile = null;
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public DataReaderWriter()
        {
            if (this.dataFile == null)
            {
                this.IntializeComponent();
            }
            
        }

        public async void IntializeComponent()
        {
            this.dataFile = await this.localFolder.CreateFileAsync("dataFile", CreationCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// Return the data between timestamp t1 and t2.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public async void ReadData(DateTime t1, DateTime t2)
        {
            // do nothing now..
        }

        /// <summary>
        /// Write data to the file.
        /// </summary>
        /// <param name="dm"> Object of DataModel class. </param>
        public async void WriteData(DataModel dm)
        {
            try
            {
                var currentEntry = dm.CurrentTimeStamp.ToString() + " " + dm.RamUsage.ToString() + " " + dm.PeakProcessName + " " + dm.PeakPageFaults.ToString() + Environment.NewLine;
                await FileIO.AppendTextAsync(this.dataFile, currentEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
