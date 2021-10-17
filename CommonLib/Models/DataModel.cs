using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryAdequacyAnalyzer.Models
{
    public class DataModel
    {
        public DateTime CurrentTimeStamp {  get; set; }

        public int RamUsage { get; set; }

        public int PageFaultsPerMin { get; set; }

        public ulong PageFileSize {  get; set; }

        public ulong VirtualMemorySizeInBytes {  get; set; }
    }
}
