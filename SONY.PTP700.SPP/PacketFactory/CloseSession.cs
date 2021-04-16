using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    public class CloseSession : BasicPacket
    {
        public CloseSession()
            : base(null)
        {
            this.Header = PacketHeader.Close;
        }

        public CloseSession(byte[] rawPacket)
            : base(rawPacket)
        {

        }

    }

}
