using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    public class Notify : BasicPacket
    {

        public Notify()
            : base(null)
        {
            this.Header = PacketHeader.Notify;
        }

        public Notify(byte[] rawPacket)
            : base(rawPacket)
        {

        }

    }
}
