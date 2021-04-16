using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    /// <summary>
    /// </summary>
    public class BasicMessage : BasicPacket
    {
        static internal readonly (int pos, int size) r_ID          = (0, 2);
        static internal readonly (int pos, int size) r_TYPE        = (2, 1);
        static internal readonly (int pos, int size) r_COMMAND     = (3, 0);

        public enum SourceID : byte
        {
            RCP = 0x90,
            MSU = 0x70,
            HSCU = 0x40
        }

        public byte Type
        {
            get
            {
                byte _type = 0;
                if (this.Size > r_TYPE.pos)
                    _type = this.Payload[r_TYPE.pos];
                return _type;
            }
            set
            {
                if (this.Size < r_TYPE.pos + r_TYPE.size)
                    this.Size = (byte)(r_TYPE.pos + r_TYPE.size);
                this.Payload[r_TYPE.pos] = (byte)value;
            }
        }

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

        public byte[] Block 
        {
            get
            {
                byte[] _bytes = new byte[] { };
                if (this.Size >= r_COMMAND.pos)
                {
                    Array.Resize(ref _bytes, this.Payload.Length - r_COMMAND.pos);
                    Buffer.BlockCopy(this.Payload, r_COMMAND.pos, _bytes, 0, this.Payload.Length - r_COMMAND.pos);
                }
                    
                return _bytes;
            }
            set
            {
                if (value != null)
                {
                    if (this.Size != r_COMMAND.pos + value.Length)
                        this.Size = (byte)(r_COMMAND.pos + value.Length);
                    Buffer.BlockCopy(value, 0, this.Payload, r_COMMAND.pos, value.Length);
                }
            }
        }
       
        public BasicMessage(ushort _id)
            : base(null)
        {
            this.Header = PacketHeader.Message;
            this.ID = _id;
        }

        public BasicMessage(byte[] rawPacket)
            : base(rawPacket)
        {

        }

        public static byte GetType(byte[] rawPacke)
        {
            if (rawPacke.Length <= 4)
                throw new OverflowException();

            return rawPacke[4];
        }

        public MessageResponse BasicResponse(PacketFactory.BasicPacket _packet = null)
        {
            return (_packet != null) ?
                new PacketFactory.MessageResponse((ushort)(this.ID + 1), _packet) :
                    new PacketFactory.MessageResponse((ushort)(this.ID + 1));
        }


        

    }
}