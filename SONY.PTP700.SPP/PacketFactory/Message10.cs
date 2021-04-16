using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message10 : BasicMessage
    {
        static internal readonly (int pos, int size) r_SUB_TYPE = (3, 1);
        static internal readonly (int pos, int size) r_CMD_PAIRS = (4, 0);

        Message10.SPpCommands _commands;

        public enum Message10SubType : byte
        {
            Request = 0x00,
            Response = 0x03
        };

        public Message10SubType SubType
        {
            get
            {
                if (this.Size <= r_SUB_TYPE.pos)
                    throw new ArgumentNullException();
                return (Message10SubType)this.Payload[r_SUB_TYPE.pos];
            }
            set
            {
                if (this.Size < r_SUB_TYPE.pos + r_SUB_TYPE.size)
                    this.Size = (byte)(r_SUB_TYPE.pos + r_SUB_TYPE.size);
                this.Payload[r_SUB_TYPE.pos] = (byte)value;
            }
        }

        public Message10.SPpCommands Commands { 
            get 
            {
                return _commands;
            } 
        }

        public Message10(ushort _id)
            : base(_id)
        {
            this.Type = 0x10;
            _commands = new Message10.SPpCommands(this);
        }

        public Message10(byte[] rawPacket)
            : base(rawPacket)
        {
            _commands = new Message10.SPpCommands(this);
        }


    }
}
