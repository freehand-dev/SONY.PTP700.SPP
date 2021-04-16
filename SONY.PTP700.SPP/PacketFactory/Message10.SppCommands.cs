using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SONY.PTP700.SPP.PacketFactory
{
    public partial class Message10 : BasicMessage
    {
        public class SPpCommandPair : IEnumerable<byte>
        {
            public byte Value { get; set; }
            public byte[] Command { get; set; } = { };

            public int Size
            {
                get
                {
                    return 1 + this.Command.Length;
                }
            }


            public SPpCommandPair(byte value, byte[] command)
            {
                this.Value = value;
                this.Command = command;
            }

            public SPpCommandPair(byte value, uint command)
            {
                this.Value = value;
                this.Command = BitConverter.GetBytes(command);
            }


            public byte[] ToBytes()
            {
                byte[] _buffer = { };
                if (this.Size > 0)
                {
                    Array.Resize<byte>(ref _buffer, this.Size);
                    _buffer[0] = this.Value;
                    System.Buffer.BlockCopy(this.Command, 0, _buffer, 1, this.Command.Length);
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
            Message10 Owner;

            public SPpCommands(Message10 owner)
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


            public int IndexOf(byte[] command)
            {
                SPpCommandPair[] items = this.ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    if (Utils.ByteUtils.ByteArrayCompare(items[i].Command, command))
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

                int _place = r_CMD_PAIRS.pos;
                while (this.Owner.Size > _place)
                {
                    _list.Add(
                        new SPpCommandPair(
                            this.Owner.Payload[_place],
                            BitConverter.ToUInt32(this.Owner.Payload, _place + 1)
                        )
                    );
                    _place += 5;
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
