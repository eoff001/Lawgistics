using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LG_Timer {
  class SettableStopwatch : System.Diagnostics.Stopwatch {
    public TimeSpan _offset;
    //public System.Diagnostics.Stopwatch _stopwatch;

    public SettableStopwatch(TimeSpan offsetElapsedTimeSpan) {
      _offset = offsetElapsedTimeSpan;
      
    }

    public new void Reset() {
      base.Reset();
      _offset = TimeSpan.Zero;
    }

    public TimeSpan ElapsedTimeSpan {
      get {
        if (_offset == null) {
          return Elapsed;
        }
        return Elapsed.Add(_offset);
      }
      set {
        _offset = value;
      }
    }
  }
}