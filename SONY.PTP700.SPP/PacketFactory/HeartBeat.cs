/**

    Packet send every 1 seconds

    [>] 0x0800      (Header: 0x08, Size: 0x00)
    [<] 0x0900      (Header: 0x09, Size: 0x00)

*/


using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    public class HeartBeat : BasicPacket
    {

        public HeartBeat()
            : base(null)
        {
            this.Header = PacketHeader.HeartBeat;
        }

        public HeartBeat(byte[] rawPacket)
            : base(rawPacket)
        {

        }

    }
}
