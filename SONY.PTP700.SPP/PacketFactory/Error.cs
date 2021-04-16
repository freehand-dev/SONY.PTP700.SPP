using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    public class Error : BasicPacket
    {
        public Error()
            : base(null)
        {
            this.Header = PacketHeader.Error;
        }

        public Error(byte[] rawPacket)
            : base(rawPacket)
        {

        }

    }
}
