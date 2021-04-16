using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message50 : BasicMessage
    {
        static internal readonly (int pos, int size, byte value) r_UNKNOWN = (3, 1, 0x18);
        static internal readonly (int pos, int size) r_SUB_TYPE = (4, 1);
        static internal readonly (int pos, int size) r_CCU_NO = (5, 2);
        static internal readonly (int pos, int size, byte value) r_UNKNOWN1 = (7, 1, 0x18);      
        static internal readonly (int pos, int size) r_SENDER_SRCID = (8, 1);
        static internal readonly (int pos, int size) r_CCU_NO1 = (9, 2);
        static internal readonly (int pos, int size) r_CMD_SIZE = (11, 2);
        static internal readonly (int pos, int size) r_CMD_PAIRS = (13, 0);

        Message50.SPpCommands _commands;

        public enum Message50SubType : byte
        {
            Request = 0x02,
            Response = 0x01,
            Unknown_0x04 = 0x04
        };

        public int CcuID
        {
            set 
            {
                this.CCU_NO = this.CCU_NO1 = Utils.ByteUtils.ReverseBytes((ushort)value);
            }
        }

        public byte Unknown
        {
            get
            {
                if (this.Size <= r_UNKNOWN.pos)
                    throw new ArgumentNullException();
                return this.Payload[r_UNKNOWN.pos];
            }
            set
            {
                if (this.Size < r_UNKNOWN.pos + r_UNKNOWN.size)
                    this.Size = (byte)(r_UNKNOWN.pos + r_UNKNOWN.size);
                this.Payload[r_UNKNOWN.pos] = value;
            }
        }

        public Message50SubType SubType
        {
            get
            {
                if (this.Size <= r_SUB_TYPE.pos)
                    throw new ArgumentNullException();
                return (Message50SubType)this.Payload[r_SUB_TYPE.pos];
            }
            set
            {
                if (this.Size < r_SUB_TYPE.pos + r_SUB_TYPE.size)
                    this.Size = (byte)(r_SUB_TYPE.pos + r_SUB_TYPE.size);
                this.Payload[r_SUB_TYPE.pos] = (byte)value;
            }
        }

        public ushort CCU_NO
        {
            get
            {
                ushort _id = 0;
                if (this.Size > r_CCU_NO.pos)
                    _id = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(this.Payload, r_CCU_NO.pos));
                return _id;
            }
            set
            {
                if (this.Size < r_CCU_NO.pos + r_CCU_NO.size)
                    this.Size = (byte)(r_CCU_NO.pos + r_CCU_NO.size);

                byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(value));
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_CCU_NO.pos, _buffer.Length);
            }
        }

        public byte Unknown1
        {
            get
            {
                if (this.Size <= r_UNKNOWN1.pos)
                    throw new ArgumentNullException();
                return this.Payload[r_UNKNOWN1.pos];
            }
            set
            {
                if (this.Size < r_UNKNOWN1.pos + r_UNKNOWN1.size)
                    this.Size = (byte)(r_UNKNOWN1.pos + r_UNKNOWN1.size);
                this.Payload[r_UNKNOWN1.pos] = value;
            }
        }

        public SourceID SenderSrcId
        {
            get
            {
                if (this.Size <= r_SENDER_SRCID.pos)
                    throw new ArgumentNullException();
                return (SourceID)this.Payload[r_SENDER_SRCID.pos];
            }
            set
            {
                if (this.Size < r_SENDER_SRCID.pos + r_SENDER_SRCID.size)
                    this.Size = (byte)(r_SENDER_SRCID.pos + r_SENDER_SRCID.size);
                this.Payload[r_SENDER_SRCID.pos] = (byte)value;
            }
        }


        public ushort CCU_NO1
        {
            get
            {
                ushort _id = 0;
                if (this.Size > r_CCU_NO1.pos)
                    _id = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(this.Payload, r_CCU_NO1.pos));
                return _id;
            }
            set
            {
                if (this.Size < r_CCU_NO1.pos + r_CCU_NO1.size)
                    this.Size = (byte)(r_CCU_NO1.pos + r_CCU_NO1.size);

                byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(value));
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_CCU_NO1.pos, _buffer.Length);
            }
        }

        public Message50.SPpCommands Commands
        {
            get
            {
                return _commands;
            }
        }

        public Message50(ushort _id)
            : base(_id)
        {
            this.Type = 0x50;
            this.SubType = Message50SubType.Request;
            this.CCU_NO = 0;
            this.CCU_NO1 = 0;
            this.Unknown = r_UNKNOWN.value;
            this.Unknown1 = r_UNKNOWN1.value;
            this.SenderSrcId = SourceID.RCP;

            this._commands = new Message50.SPpCommands(this);
            this._commands.Size = 0;
        }

        public Message50(byte[] rawPacket)
            : base(rawPacket)
        {
            this._commands = new Message50.SPpCommands(this);
        }

    }
}
