using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.Events
{
    public class DataReceivedEventArgs : System.EventArgs
    {
        public byte[] Data { get; set; }
        public long BytesRead { get; set; }
    }
}
