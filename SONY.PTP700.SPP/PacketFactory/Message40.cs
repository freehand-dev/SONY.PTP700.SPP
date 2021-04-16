using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message40 : BasicMessage
    {

        static internal readonly (int pos, int size) r_CMD_SIZE = (11, 2);
        static internal readonly (int pos, int size) r_CMD_PAIRS = (13, 0);

        Message40.SPpCommands _commands;

        public Message40.SPpCommands Commands
        {
            get
            {
                return _commands;
            }
        }

        public Message40(ushort _id)
            : base(_id)
        {
            this.Type = 0x40;

            this._commands = new Message40.SPpCommands(this);
            this._commands.Size = 0;
        }

        public Message40(byte[] rawPacket)
            : base(rawPacket)
        {
            this._commands = new Message40.SPpCommands(this);
        }


    }
}
