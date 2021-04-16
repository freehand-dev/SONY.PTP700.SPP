using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SONY.PTP700.SPP.PacketFactory.Command;
using SONY.PTP700.SPP.Utils;

namespace SONY.PTP700.SPP.PacketFactory
{
    partial class Message30 : BasicMessage
    {

        [Flags]
        public enum PermissionControl
        {
            PanleActive,
            IrisActive,
            ParaActive,
        }
                                

        static internal readonly (int pos, int size) r_CCU_NO = (3, 2);
        static internal readonly (int pos, int size, byte value) r_UNKNOWN = (5, 1, 0x12);
        static internal readonly (int pos, int size) r_SENDER_SRCID = (6, 1);
        static internal readonly (int pos, int size) r_RCP_FLAG = (7, 2);
        static internal readonly (int pos, int size) r_CMD_PAIRS = (9, 0);


        Message30.SPpCommands _commands;

        public ushort CcuNo
        {
            get
            {
                ushort _id = 0;
                if (this.Size > r_CCU_NO.pos)
                    _id = (BitConverter.ToUInt16(this.Payload, r_CCU_NO.pos));
                return _id;
            }
            set
            {
                if (this.Size < r_CCU_NO.pos + r_CCU_NO.size)
                    this.Size = (byte)(r_CCU_NO.pos + r_CCU_NO.size);

                byte[] _buffer = BitConverter.GetBytes(value);
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_CCU_NO.pos, _buffer.Length);
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

        public byte RcpNo
        {
            get
            {
                byte _return = 0;
                if (this.Size > r_RCP_FLAG.pos)
                    _return = SppUtils.ParseRcpID(Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(this.Payload, r_RCP_FLAG.pos))).id;
                return _return;
            }
            set
            {
                if (this.Size < r_RCP_FLAG.pos + r_RCP_FLAG.size)
                    this.Size = (byte)(r_RCP_FLAG.pos + r_RCP_FLAG.size);

                byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(SppUtils.RcpIdToRaw(value, this.RcpActiveFlag)));
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_RCP_FLAG.pos, _buffer.Length);
            }
        }


        public bool RcpActiveFlag
        {
            get
            {
                bool _return = default;
                if (this.Size > r_RCP_FLAG.pos)
                    _return = SppUtils.ParseRcpID(Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(this.Payload, r_RCP_FLAG.pos))).enabled;
                return _return;
            }
            set
            {
                if (this.Size < r_RCP_FLAG.pos + r_RCP_FLAG.size)
                    this.Size = (byte)(r_RCP_FLAG.pos + r_RCP_FLAG.size);

                byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(SppUtils.RcpIdToRaw(this.RcpNo, value)));
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_RCP_FLAG.pos, _buffer.Length);
            }
        }

        public Message30.SPpCommands Commands
        {
            get
            {
                return _commands;
            }
        }


        public Message30(ushort _id)
            : base(_id)
        {
            this.Type = 0x30;
            this.CcuNo = 0;
            this.Unknown = r_UNKNOWN.value;
            this.SenderSrcId = SourceID.RCP;
            this.RcpNo = 1; 
            this.RcpActiveFlag = true;
            this._commands = new Message30.SPpCommands(this);

        }

        public Message30(byte[] rawPacket)
            : base(rawPacket)
        {
            this._commands = new Message30.SPpCommands(this);
        }

    }
}
