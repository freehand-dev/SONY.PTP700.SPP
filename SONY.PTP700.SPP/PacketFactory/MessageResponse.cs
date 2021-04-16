using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    /// <summary>
    /// </summary>
    public class MessageResponse : BasicPacket
    {
        internal readonly (int pos, int size) r_ID = (0, 2);

        public ushort ID
        {
            get
            {
                ushort _id = 0;
                if (this.Size > r_ID.pos)
                    _id = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(this.Payload, r_ID.pos));
                return _id;
            }
            set
            {
                if (this.Size < r_ID.pos + r_ID.size)
                    this.Size = (byte)(r_ID.pos + r_ID.size);

                byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(value));
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_ID.pos, _buffer.Length);
            }
        }

        public MessageResponse(ushort _id)
            : base(null)
        {
            this.Header = PacketHeader.MessageResponse;
            this.ID = _id;
        }

        public MessageResponse(ushort _id, BasicPacket _packet)
            : base(null)
        {
            this.Header = PacketHeader.MessageResponse;
            this.ID = _id;
            this.NextPacket = _packet;
        }

        public MessageResponse(byte[] rawPacket)
            : base(rawPacket)
        {

        }


    }
}