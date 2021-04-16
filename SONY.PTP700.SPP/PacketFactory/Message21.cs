using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message21 : BasicMessage
    {
        public Message21(ushort _id)
            : base(_id)
        {
            this.Type = 0x21;
        }

        public Message21(byte[] rawPacket)
            : base(rawPacket)
        {

        }


    }
}
