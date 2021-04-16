using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SONY.PTP700.SPP.PacketFactory
{
    public partial class Message20 : BasicMessage
    {

        public class SPpCommandPair : IEnumerable<byte>
        {

            static public byte[] Header = new byte[] { 0xd9, 0xfe };

            public byte[] _payload = { };

            public byte[] Payload { 
                get
                {
                    return _payload;
                }
                set 
                {
                    _payload = value;
                } 
            }

            public int Size
            {
                get
                {
                    return 2 + Payload.Length;
                }
            }


            public SPpCommandPair(byte[] buffer)
            {
                ushort header = BitConverter.ToUInt16(buffer, 0);

                if (header != BitConverter.ToUInt16(SPpCommandPair.Header, 0) || buffer.Length <= 2)
                    throw new ArgumentException();

                int payload_size = buffer.Length - 2;
                Array.Resize(ref this._payload, payload_size);
                Buffer.BlockCopy(buffer, 2, this.Payload, 0, payload_size);
            }


            public SPpCommandPair(byte[] buffer, int offset, int count)
            {
                ushort header = BitConverter.ToUInt16(buffer, offset);

                if (header != BitConverter.ToUInt16(SPpCommandPair.Header, 0) || buffer.Length <= 2)
                    throw new ArgumentException();

                int payload_size = count - 2;
                Array.Resize(ref this._payload, payload_size);
                Buffer.BlockCopy(buffer, offset + 2, this.Payload, 0, payload_size);
            }


            public byte[] ToBytes()
            {
                byte[] _buffer = { };
                if (this.Size > 0)
                {
                    Array.Resize<byte>(ref _buffer, this.Size);
                    _buffer = SPpCommandPair.Header.Concat(this.Payload).ToArray();
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
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

        }

        public sealed class SPpCommands : IEnumerable<SPpCommandPair>
        {
            Message20 Owner;

            public SPpCommands(Message20 owner)
            {
                this.Owner = owner;
            }

            public SPpCommandPair this[int index]
            {
                get
                {
                    return this.Get(index);
                }
            }

            public void Add(SPpCommandPair item)
            {
                this.Owner.Block = this.Owner.Block.Concat(item.ToArray()).ToArray();
            }

            public SPpCommandPair Get(int index)
            {
                SPpCommandPair[] _commands = this.ToArray();
                return (index >= 0 && _commands.Length > index) ? _commands[index] : null;
            }

            public int Count()
            {
                return this.ToArray().Length;
            }


            public int IndexOf(byte[] payload)
            {
                SPpCommandPair[] items = this.ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    if (Utils.ByteUtils.ByteArrayCompare(items[i].Payload, payload))
                    {
                        return i;
                    }
                }
                return -1;
            }

            public bool Contains(byte[] command)
            {
                return IndexOf(command) >= 0;
            }

            public SPpCommandPair[] ToArray()
            {
                List<SPpCommandPair> _list = new List<SPpCommandPair>();


                int _place = 0;
                while (this.Owner.Size > _place)                
                {
                    _place = Utils.ByteUtils.IndexOf(this.Owner.Payload, SPpCommandPair.Header, _place);
                    if (_place < 0)
                        break;

                    int _start = _place;
                    int _count = Utils.ByteUtils.IndexOf(this.Owner.Payload, SPpCommandPair.Header, _place + 1 ) - _place;

                    _count = (_count <= 0) ? (this.Owner.Size - _place) : _count;


                    _list.Add(
                        new SPpCommandPair(
                            this.Owner.Payload, _start, _count
                        )
                    );

                    _place += _count;


                }

                return _list.ToArray();
            }

            #region Public Methods
            public IEnumerator<SPpCommandPair> GetEnumerator()
            {
                foreach (var b in this.ToArray())
                    yield return b;
            }
            #endregion

            #region Explicit Interface Implementations
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

    }
}
