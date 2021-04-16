using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    public enum SRCID : byte
    {
        RCP = 0x90,
        HSCU = 0x40,
        MSU = 0x70,
    };

    public enum DeviceModel : byte
    {
        CNA_1 = 0x00,
        MSU_1500 = 0x06,
        RCP_1500 = 0x0a,
        HSCU_100 = 0x10,
        HSCU_300 = 0x0f,
        HSCU_1700 = 0x1a,
    };

    public enum PacketType 
    {
        HandShake,
        Close,
        Message, 
        Notify, 
        Error, 
        HeartBeat,
        Unknown,
    };

    public enum PacketHeader : byte
    {  
        HandShakeACK = 0x01,
        HandShake = 0x02,
        HandShakeResponse = 0x03,
        Close = 0x04,     
        CloseACK = 0x05,   
        HeartBeat = 0x08,
        HeartBeatACK = 0x09,
        Notify = 0x0A,
        NotifyACK = 0x0B,
        ErrorID = 0x0C,
        Error = 0x0D,
        Message = 0x0E,
        MessageResponse = 0x0F,
    };

    /// <summary>
    /// </summary>
    public abstract class BasicPacket : IEnumerable<byte>
    {
        private byte _header;
        private byte[] _payload = { };


        public WeakReference Parent { get; set; }
        public PacketHeader Header {
            get
            {
                return (PacketHeader)this._header;
            }
            set
            {
                this._header = (byte)value;
            }
        }

        public bool ParentIsResponse {
            get 
            {
                return (Parent != null) ? (Parent.IsAlive && ((PacketFactory.BasicPacket)(Parent.Target)).Header == PacketFactory.PacketHeader.MessageResponse) : false;
            }
                
         }

        public byte Size {
            get 
            {
                return (byte)this._payload.Length;
            }
            set
            {
                if (this._payload == null)
                {
                    this._payload = new byte[value];
                } 
                else
                {
                    if (this.Payload.Length != value)
                    {
                         Array.Resize<byte>(ref this._payload, value);
                    }
                }
                 
            } 
        }
        public byte[] Payload {
            get 
            {
                return this._payload;
            }
        }
        public BasicPacket NextPacket {get; set; }


        public PacketType PacketType {
            get 
            {
                switch ((byte)this.Header)
                {
                    case 0x01:
                    case 0x02:
                    case 0x03:
                        return PacketType.HandShake;
                    case 0x04:
                    case 0x05:
                        return PacketType.Close;
                    case 0x08:
                    case 0x09:
                        return PacketType.HeartBeat;
                    case 0x0A:
                    case 0x0B:
                        return PacketType.Notify;
                    case 0x0C:
                    case 0x0D:
                        return PacketType.Error;
                    case 0x0E:                     
                    case 0x0F:
                        return PacketType.Message;
                    default:
                        return PacketType.Unknown;
                }
            }
        }


        public BasicPacket(byte[] rawPacket)
        {
            if (rawPacket?.Length > 0)
            {
                int place = 0;
                this.Header = (PacketHeader)rawPacket[place];
                place += 1;
                this.Size = rawPacket[place];
                place += 1;
                Buffer.BlockCopy(rawPacket, place, this.Payload, 0, this.Size);

                int _packetSize = this.GetPacketSize();
                if (rawPacket.Length > _packetSize)
                {
                    byte[] _nexPacketBuffer = new byte[rawPacket.Length - _packetSize];
                    Buffer.BlockCopy(rawPacket, _packetSize, _nexPacketBuffer, 0, _nexPacketBuffer.Length);
                    this.NextPacket = BasicPacket.InitPacket(_nexPacketBuffer);
                    this.NextPacket.Parent = new WeakReference(this);
                }
            }
        }

        public static BasicPacket InitPacket(byte[] rawPacket)
        {
            if (rawPacket.Length < 2)
            {
                throw new Exception("The data packet is too small");
            }

            switch (rawPacket[0])
            {
                case 0x01:
                case 0x02:
                case 0x03:
                    return new PacketFactory.HandShake(rawPacket);
                case 0x05:
                case 0x04:
                    return new PacketFactory.CloseSession(rawPacket);
                case 0x08:
                case 0x09:
                    return new PacketFactory.HeartBeat(rawPacket);
                case 0x0A:
                case 0x0B:
                    return new PacketFactory.Notify(rawPacket);
                case 0x0C:
                case 0x0D:
                    return new PacketFactory.Error(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x01):
                    return new PacketFactory.Message01(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x10):
                    return new PacketFactory.Message10(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x20):
                    return new PacketFactory.Message20(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x21):
                    return new PacketFactory.Message21(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x30):
                    return new PacketFactory.Message30(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x40):
                    return new PacketFactory.Message40(rawPacket);
                case 0x0E when (PacketFactory.BasicMessage.GetType(rawPacket) == 0x50):
                    return new PacketFactory.Message50(rawPacket);
                case 0x0E:
                    return new PacketFactory.BasicMessage(rawPacket);
                case 0x0F:
                    return new PacketFactory.MessageResponse(rawPacket);
                default:
                    return null;
            }

        }


        public int GetPacketSize()
        {
            int _rawSize = 2 + this.Payload.Length; // header size (1 byte) + size payload (1 byte) + payload size
            if (this.NextPacket != null && this.Header == PacketHeader.MessageResponse)
            {
                _rawSize += this.NextPacket.GetPacketSize();
            }
            return _rawSize;
        }

        public byte[] ToBytes()
        {
            var _rawSize = this.GetPacketSize();
            byte[] _buffer = new byte[_rawSize];

            int place = 0;
            Buffer.SetByte(_buffer, place, (byte)this.Header);
            place += 1;

            Buffer.SetByte(_buffer, place, this.Size);
            place += 1;

            Buffer.BlockCopy(this.Payload, 0, _buffer, place, this.Payload.Length);
            place += this.Payload.Length;

            if (this.NextPacket != null && this.Header == PacketHeader.MessageResponse)
            {
                var _nexPacketRaw = this.NextPacket.ToBytes();
                Buffer.BlockCopy(_nexPacketRaw, 0, _buffer, place, _nexPacketRaw.Length);
                place += _nexPacketRaw.Length;
            }

            return _buffer;
        }


        #region Public Methods
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in this.ToBytes())
                yield return b;
        }

        public byte[] ToArray()
        {
            return this.ToBytes();
        }

        public override string ToString()
        {
            return BitConverter.ToString(this.ToBytes());
        }

        #endregion

        #region Explicit Interface Implementations
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }

        #endregion
        
    }
}