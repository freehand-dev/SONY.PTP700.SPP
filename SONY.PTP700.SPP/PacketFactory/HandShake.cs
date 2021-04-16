using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory
{
    /// <summary>
    /// </summary>
    public class HandShake : BasicPacket
    {

        internal readonly (int pos, int size, byte[] value) r_UNKNOWN       = (0, 3, new byte[] { 0x02, 0x01, 0x00 });
        internal readonly (int pos, int size) r_CNS_MODE                    = (3, 1);
        internal readonly (int pos, int size) r_ID                          = (4, 2);
        internal readonly (int pos, int size, byte[] value) r_UNKNOWN1      = (6, 1, new byte[] { 0x01 });
        internal readonly (int pos, int size) r_DEVICETYE                   = (7, 1);
        internal readonly (int pos, int size) r_DEVICETYE1                  = (8, 1);
        internal readonly (int pos, int size) r_MODEL                       = (9, 1);
        internal readonly (int pos, int size) r_SERIAL_NUMBER               = (10, 4);
        internal readonly (int pos, int size, byte[] value) r_UNKNOWN2      = (14, 4, new byte[] { 0x64, 0x32, 0x0a, 0x05 });

        public uint SerialNumber {
            get 
            {
                uint _serialNumber = 0;
                if (this.Size >= r_SERIAL_NUMBER.pos + r_SERIAL_NUMBER.size)
                    _serialNumber = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt32(this.Payload, r_SERIAL_NUMBER.pos));
                return _serialNumber;
            }
            set {
                if (this.Size < r_SERIAL_NUMBER.pos + r_SERIAL_NUMBER.size)
                    this.Size = (byte)(r_SERIAL_NUMBER.pos + r_SERIAL_NUMBER.size);

                byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(value));
                Buffer.BlockCopy(_buffer, 0, this.Payload, r_SERIAL_NUMBER.pos, _buffer.Length);
            }
        }
        public ushort ID {
            get
            {
                ushort _id = 0;
                if (this.Size >= r_ID.pos + r_ID.size)
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
        public CNSMode CNSMode {
            get
            {
                CNSMode _cnsMode = CNSMode.LEGACY;
                if (this.Size >= r_CNS_MODE.pos + r_CNS_MODE.size)
                    _cnsMode = (CNSMode)this.Payload[r_CNS_MODE.pos];
                return _cnsMode;
            }
            set {
                if (this.Size < r_CNS_MODE.pos + r_CNS_MODE.size)
                    this.Size = (byte)(r_CNS_MODE.pos + r_CNS_MODE.size);

                this.Payload[r_CNS_MODE.pos] = (byte)value;
            }
        }

        public DeviceModel Model
        {
            get
            {
                if (this.Size <= r_MODEL.pos)
                    throw new ArgumentNullException();                   
                return (DeviceModel)this.Payload[r_MODEL.pos];
            }
            set
            {
                if (this.Size < r_MODEL.pos + r_MODEL.size)
                    this.Size = (byte)(r_MODEL.pos + r_MODEL.size);
                this.Payload[r_MODEL.pos] = (byte)value;
            }
        }

        public SRCID Type
        {
            get
            {
                if (this.Size <= r_DEVICETYE.pos)
                    throw new ArgumentNullException();
                return (SRCID)this.Payload[r_DEVICETYE.pos];
            }
            set
            {
                if (this.Size < r_DEVICETYE.pos + r_DEVICETYE.size)
                    this.Size = (byte)(r_DEVICETYE.pos + r_DEVICETYE.size);
                this.Payload[r_DEVICETYE.pos] = (byte)value;
            }
        }

        public SRCID Type1
        {
            get
            {
                if (this.Size <= r_DEVICETYE1.pos)
                    throw new ArgumentNullException();
                return (SRCID)this.Payload[r_DEVICETYE1.pos];
            }
            set
            {
                if (this.Size < r_DEVICETYE1.pos + r_DEVICETYE1.size)
                    this.Size = (byte)(r_DEVICETYE1.pos + r_DEVICETYE1.size);
                this.Payload[r_DEVICETYE1.pos] = (byte)value;
            }
        }

        public byte[] Unknown {
            get
            {
                byte[] _bytes = new byte[] { };
                if (this.Size >= r_UNKNOWN.pos + r_UNKNOWN.size)
                    Buffer.BlockCopy(this.Payload, r_UNKNOWN.pos, _bytes, 0, r_UNKNOWN.size);
                return _bytes;
            }
            set {
                if (this.Size < r_UNKNOWN.pos + r_UNKNOWN.size)
                    this.Size = (byte)(r_UNKNOWN.pos + r_UNKNOWN.size);
                SetUnknown(value, r_UNKNOWN.pos, r_UNKNOWN.size);
            }
        }

        public byte[] Unknown1
        {
            get
            {
                byte[] _bytes = new byte[] { };
                if (this.Size >= r_UNKNOWN1.pos + r_UNKNOWN1.size)
                    Buffer.BlockCopy(this.Payload, r_UNKNOWN1.pos, _bytes, 0, r_UNKNOWN1.size);
                return _bytes;
            }
            set {
                if (this.Size < r_UNKNOWN1.pos + r_UNKNOWN1.size)
                    this.Size = (byte)(r_UNKNOWN1.pos + r_UNKNOWN1.size);
                SetUnknown(value, r_UNKNOWN1.pos, r_UNKNOWN1.size);
            }
        }

        public byte[] Unknown2
        {
            get
            {
                byte[] _bytes = new byte[] { };
                if (this.Size >= r_UNKNOWN2.pos + r_UNKNOWN2.size)
                    Buffer.BlockCopy(this.Payload, r_UNKNOWN2.pos, _bytes, 0, r_UNKNOWN2.size);
                return _bytes;
            }
            set {
                if (this.Size < r_UNKNOWN2.pos + r_UNKNOWN2.size)
                    this.Size = (byte)(r_UNKNOWN2.pos + r_UNKNOWN2.size);
                SetUnknown(value, r_UNKNOWN2.pos, r_UNKNOWN2.size);
            }
        }

        public HandShake()
            : base(null)
        {
            this.Size = 18;
            this.Unknown = r_UNKNOWN.value;
            this.Unknown1 = r_UNKNOWN1.value;
            this.Unknown2 = r_UNKNOWN2.value; 
        }

        public HandShake(byte[] rawPacket)
            : base(rawPacket)
        {
    
        }

        internal void SetUnknown(byte[] value, int offset, int count)
        {
            if (value != null) {
                Buffer.BlockCopy(value, 0, this.Payload, offset, count);
            }
        }

        static public HandShake InitHandShake(PacketHeader Header, CNSMode Mode, ushort RequestID,  uint SerialNumber)
        {
            return new HandShake() {
                Header = Header,
                CNSMode = Mode,
                ID = RequestID,
                SerialNumber = SerialNumber,
                Model = DeviceModel.RCP_1500,
                Type = SRCID.RCP,
                Type1 = SRCID.RCP,
            };
        }

        
    }
}