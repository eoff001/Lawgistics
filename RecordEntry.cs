using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LG_Timer {
  [Serializable]
  public class RecordEntry {
    //public string clientName { get; set; }
    public string billableTime { get; set; }
    public TimeSpan actualTime { get; set; }
    public string notes { get; set; }
  }
}
