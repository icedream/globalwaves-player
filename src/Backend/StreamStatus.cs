using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace globalwaves.Player.Backend
{
    public enum StreamStatus
    {
        Stopped = 0,
        Connecting = 1,
        Buffering = 2,
        Playing = 3
    }
}
