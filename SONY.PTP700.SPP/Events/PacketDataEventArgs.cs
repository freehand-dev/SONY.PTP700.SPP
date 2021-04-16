using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SONY.PTP700.SPP.Events
{ 
    public class PacketReceivedEventArgs : System.EventArgs
    {
        public PacketFactory.BasicPacket Packet { get; set; }
    }
}