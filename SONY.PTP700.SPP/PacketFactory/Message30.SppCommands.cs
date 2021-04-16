using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SONY.PTP700.SPP.PacketFactory
{
    public partial class Message30 : BasicMessage
    {
        public class SPpCommandPair : IEnumerable<byte>
        {

            public string Name
            {
                get
                {
                    return Message30.GetCommandName(this).Name;
                }
            }

            public string Description
            {
                get
                {
                    return Message30.GetCommandName(this).Description;
                }
            }

            internal byte[] _data = new byte[] { };

            public byte CMD_GP
            {
                get
                {
                    return _data[0];
                }
                set
                {
                    if (_data.Length < 1)
                        Array.Resize<byte>(ref _data, 1);
                    _data[0] = value;
                }
            }
            public byte PARAM0
            {
                get
                {
                    return _data[1];
                }
                set
                {
                    if (_data.Length < 2)
                        Array.Resize<byte>(ref _data, 2);
                    _data[1] = value;
                }
            }
            public byte? PARAM1
            {
                get
                {
                    return (_data.Length >= 3) ? _data[2] : default; ;
                }
                set
                {
                    if (_data.Length < 3)
                        Array.Resize<byte>(ref _data, 3);
                    _data[2] = value.Value;
                }
            }
            public byte? PARAM2
            {
                get
                {
                    return (_data.Length >= 4) ? _data[3] : default;
                }
                set
                {
                    if (_data.Length < 4)
                        Array.Resize<byte>(ref _data, 4);
                    _data[3] = value.Value;
                }
            }
            public byte? PARAM3
            {
                get
                {
                    return (_data.Length >= 5) ? _data[4] : default;
                }
                set
                {
                    if (_data.Length < 5)
                        Array.Resize<byte>(ref _data, 5);
                    _data[4] = value.Value;
                }
            }

            public int Size
            {
                get
                {
                    return _data.Length;
                }
            }


            public SPpCommandPair(byte cmd_gp, byte param_0, byte param_1, byte param_2, byte param_3)
            {
                Array.Resize<byte>(ref _data, 5);
                this._data[0] = cmd_gp;
                this._data[1] = param_0;
                this._data[2] = param_1;
                this._data[3] = param_2;
                this._data[4] = param_3;
            }

            public SPpCommandPair(byte cmd_gp, byte param_0, byte param_1, byte param_2)
            {
                Array.Resize<byte>(ref _data, 4);
                this._data[0] = cmd_gp;
                this._data[1] = param_0;
                this._data[2] = param_1;
                this._data[3] = param_2;
            }

            public SPpCommandPair(byte cmd_gp, byte param_0, byte param_1)
            {
                Array.Resize<byte>(ref _data, 3);
                this._data[0] = cmd_gp;
                this._data[1] = param_0;
                this._data[2] = param_1;
            }

            public SPpCommandPair(byte cmd_gp, byte param_0)
            {
                Array.Resize<byte>(ref _data, 2);
                this._data[0] = cmd_gp;
                this._data[1] = param_0;
            }

            public SPpCommandPair(byte[] data, int offset = 0)
            {
                Array.Resize<byte>(ref _data, data.Length);
                Buffer.BlockCopy(data, offset, _data, 0, data.Length);
            }


            public byte[] ToBytes()
            {
                return _data;
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
            Message30 Owner;

            public SPpCommands(Message30 owner)
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

            public SPpCommandPair Get(byte cmd_gp, byte? param_0 = null, byte? param_1 = null, byte? param_2 = null, byte? param_3 = null)
            {
                int _index = this.IndexOf(cmd_gp, param_0, param_1, param_2, param_3);
                SPpCommandPair[] _commands = this.ToArray();
                return (_index >= 0 && _commands.Length > _index) ? _commands[_index] : null;
            }

            public int Count()
            {
                return this.ToArray().Length;
            }


            public int IndexOf(byte cmd_gp, byte? param_0 = null, byte? param_1 = null, byte? param_2 = null, byte? param_3 = null)
            {

                SPpCommandPair[] items = this.ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].CMD_GP == cmd_gp)
                    {
                        if (param_0.HasValue)
                        {
                            if (items[i].PARAM0 == param_0)
                            {
                                if (param_1.HasValue)
                                {
                                    if (items[i].PARAM1 == param_1)
                                    {
                                        if (param_2.HasValue)
                                        {
                                            if (items[i].PARAM2 == param_2)
                                            {
                                                if (param_3.HasValue)
                                                {
                                                    if (items[i].PARAM3 == param_3)
                                                    {
                                                        return i;
                                                    }
                                                }
                                                else
                                                {
                                                    return i;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return i;
                                        }
                                    }
                                }
                                else
                                {
                                    return i;
                                }
                            }
                        }
                        else
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }

            public bool Contains(byte cmd_gp)
            {
                return IndexOf(cmd_gp, null, null, null, null) >= 0;
            }

            public bool Contains(byte cmd_gp, byte param_0)
            {
                return IndexOf(cmd_gp, param_0, null, null, null) >= 0;
            }

            public bool Contains(byte cmd_gp, byte param_0, byte param_1)
            {
                return IndexOf(cmd_gp, param_0, param_1, null, null) >= 0;
            }

            public SPpCommandPair[] ToArray()
            {
                List<SPpCommandPair> _list = new List<SPpCommandPair>();

                int _place = r_CMD_PAIRS.pos;
                while (this.Owner.Size > _place)
                {
                    _list.Add(
                         new SPpCommandPair(
                             this.Owner.Payload[_place++], // cmd_gp
                             this.Owner.Payload[_place++] // param_0
                         ));

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

        private static (string Name, string Description) GetCommandName(SPpCommandPair command)
        {
            switch (command.CMD_GP)
            {
                // ::BUTTON::CALL"
                case 0x01:
                    return ("::PANEL_ACTIVE", "::BUTTON::PANEL");
                case 0x02:
                    return ("::IRIS_ACTIVE", "::BUTTON::IRIS");
                case 0x03:
                    return ("::PARA_ACTIVE", "::BUTTON::PARA");
                case 0x81:
                    return ("UNKNOWN", "");
                case 0x82:
                    return ("UNKNOWN", "");
                default:
                    return ("UNKNOWN", "");
            }
        }
    }

}
