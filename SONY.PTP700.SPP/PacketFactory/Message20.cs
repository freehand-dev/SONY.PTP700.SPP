using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message20 : BasicMessage
    {

        Message20.SPpCommands _commands;

        public Message20.SPpCommands Commands
        {
            get
            {
                return _commands;
            }
        }

        public Message20(ushort _id)
            : base(_id)
        {
            this.Type = 0x20;
            _commands = new Message20.SPpCommands(this);

        }

        public Message20(byte[] rawPacket)
            : base(rawPacket)
        {
            _commands = new Message20.SPpCommands(this);
        }


    }
}
