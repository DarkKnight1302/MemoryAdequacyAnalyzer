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
        private static Semaphore semaphore = new Semaphore(1, 1, "dataRead");

        public static DataReaderWriter Instance { get; } = new DataReaderWriter();

        private DataReaderWriter()
        {
        }

        /// <summary>
        /// Return the data between timestamp t1 and t2.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public async Task<List<DataModel>> ReadData(DateTime t1, DateTime t2)
        {
            try
            {
                semaphore.WaitOne();
                List<DataModel> dataEntry = new List<DataModel>();
                DataModel dm;
                StorageFile sampleFile = await localFolder.GetFileAsync("dataFile");
                String stringContent = await FileIO.ReadTextAsync(sampleFile);
                string[] enteries = stringContent.Split(Environment.NewLine);
                foreach (string entry in enteries)
                {
                    if (entry == "")
                    {
                        continue;
                    }
                    if(DateTime.Compare(t1,DateTime.Parse(entry.Split('|')[0])) < 0 && DateTime.Compare(t2, DateTime.Parse(entry.Split('|')[0])) > 0)
                    {
                        dm = new DataModel
                        {
                            CurrentTimeStamp = DateTime.Parse(entry.Split('|')[0]),
                            RamUsage = int.Parse(entry.Split('|')[1]),
                            PageFileSize = ulong.Parse(entry.Split('|')[2]),
                            PageFaultsPerMin = int.Parse(entry.Split('|')[3]),
                            VirtualMemorySizeInBytes = ulong.Parse(entry.Split('|')[4]),
                        };
                        dataEntry.Add(dm);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
            } finally
            {
                semaphore.Release();
            }
            return new List<DataModel>();
        }

        // <summary>
        /// Return the data between timestamp t1 and t2.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public async Task<List<DataModel>> ReadDataFromBeginning()
        {
            DateTime dateTime = DateTime.Now;
            try
            {
                semaphore.WaitOne();
                List<DataModel> dataEntry = new List<DataModel>();
                DataModel dm;
                StorageFile sampleFile = await localFolder.GetFileAsync("dataFile");
                String stringContent = await FileIO.ReadTextAsync(sampleFile);
                string[] enteries = stringContent.Split(Environment.NewLine);
                foreach(string entry in enteries)
                {
                    if(entry == "")
                    {
                        continue;
                    }
                    dm = new DataModel
                    {
                        CurrentTimeStamp = DateTime.Parse(entry.Split('|')[0]),
                        RamUsage = int.Parse(entry.Split('|')[1]),
                        PageFileSize = ulong.Parse(entry.Split('|')[2]),
                        PageFaultsPerMin = int.Parse(entry.Split('|')[3]),
                        VirtualMemorySizeInBytes = ulong.Parse(entry.Split('|')[4]),
                    };
                    dataEntry.Add(dm);
                }
                return dataEntry;
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
            } finally
            {
                semaphore.Release();
            }
            return null;
        }

        /// <summary>
        /// Write data to the file.
        /// </summary>
        /// <param name="dm"> Object of DataModel class. </param>
        public async Task WriteData(DataModel dm)
        {
            try
            {
                semaphore.WaitOne();
                StorageFile dataFile = await this.localFolder.CreateFileAsync("dataFile", CreationCollisionOption.OpenIfExists);
                var currentEntry = dm.CurrentTimeStamp.ToString() + "|" + dm.RamUsage + "|" + dm.PageFileSize + "|" + dm.PageFaultsPerMin +  "|" + dm.VirtualMemorySizeInBytes + "|" + Environment.NewLine;
                await FileIO.AppendTextAsync(dataFile, currentEntry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n" +  ex.StackTrace);
            } finally
            {
                semaphore.Release();
            }
        }
    }
}
