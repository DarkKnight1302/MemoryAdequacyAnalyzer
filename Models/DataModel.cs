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
        public string PeakProcessName { get; set; }
        public int PeakPageFaults { get; set; }

        public DataModel(int ramUsage, string peakProcessName, int peakPageFaults)
        {
            this.RamUsage = ramUsage;
            this.PeakPageFaults = peakPageFaults;
            this.PeakProcessName = peakProcessName;
            this.CurrentTimeStamp = new DateTime();

        }
    }
}
