using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message01 : BasicMessage
    {
        Message01.SPpCommands _commands;

        public Message01.SPpCommands Commands
        {
            get
            {
                return _commands;
            }
        }

        public Message01(ushort _id)
            : base(_id)
        {
            this.Type = 0x01;
            _commands = new Message01.SPpCommands(this);
        }

        public Message01(byte[] rawPacket)
            : base(rawPacket)
        {
            _commands = new Message01.SPpCommands(this);
        }


    }
}
