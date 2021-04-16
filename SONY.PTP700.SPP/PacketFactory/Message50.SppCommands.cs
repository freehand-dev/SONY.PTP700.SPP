using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SONY.PTP700.SPP.PacketFactory
{
    public partial class Message50 : BasicMessage
    {
        public class SPpCommandPair : IEnumerable<byte>
        {

            public string Name { get 
                {
                    return Message50.GetCommandName(this).Name;
                }
            }

            public string Description
            {
                get
                {
                    return Message50.GetCommandName(this).Description;
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
            Message50 Owner;

            public ushort Size
            {
                get
                {
                    ushort _size = 0;
                    if (this.Owner.Size >= r_CMD_SIZE.pos + r_CMD_SIZE.size)
                        _size = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(this.Owner.Payload, r_CMD_SIZE.pos));
                    return _size;
                }
                set
                {
                    if (this.Owner.Size < r_CMD_SIZE.pos + r_CMD_SIZE.size)
                        this.Owner.Size = (byte)(r_CMD_SIZE.pos + r_CMD_SIZE.size);

                    byte[] _buffer = BitConverter.GetBytes(Utils.ByteUtils.ReverseBytes(value));
                    Buffer.BlockCopy(_buffer, 0, this.Owner.Payload, r_CMD_SIZE.pos, _buffer.Length);
                }
            }

            public SPpCommands(Message50 owner)
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
                this.Size += (byte)item.Size;
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
                    byte cmd_gp = this.Owner.Payload[_place];
                    switch (cmd_gp)
                    {
                        case 0x0b:
                        case 0x22:
                        case 0x23:
                        case 0x27:
                        case 0x29:
                        case 0x3c:
                        case 0x3d:
                        case 0x40 when (this.Owner.Payload[_place + 1] == 0x0a):
                        case 0x41 when (this.Owner.Payload[_place + 1] == 0x0a):
                        case 0x42:
                        case 0x43:
                        case 0x49:
                        case 0x6c:
                            _list.Add(
                                new SPpCommandPair(
                                    this.Owner.Payload[_place++],
                                    this.Owner.Payload[_place++],
                                    this.Owner.Payload[_place++],
                                    this.Owner.Payload[_place++]
                                )
                            );
                            break;
                        case 0x4d:
                        case 0x4e:
                        case 0x4f:
                        case 0x6d:
                            _place += 4;
                            break;
                        case 0x20:
                        case 0x21:
                        case 0x25:
                        case 0x40:
                        case 0x41:
                        case 0x60:
                        case 0x61:
                            _list.Add(
                                new SPpCommandPair(
                                    this.Owner.Payload[_place++],
                                    this.Owner.Payload[_place++],
                                    this.Owner.Payload[_place++]
                                )
                            );
                            break;
                        default:
                            _place += this.Size;
                            Console.WriteLine($"[MESSAGE50][SPpCommandPair][ToArray][Unknown][{ string.Format("{0:X2}", cmd_gp) }] { Utils.ByteUtils.ToHexString(this.Owner.Payload) }");
                            break;
                    }

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
                #region ::CHU:FUNCTION

                // ::CHU:FUNCTION:SHUTTER_SPEED"
                case 0x20 when (command.PARAM0 == 0x00):
                case 0x21 when (command.PARAM0 == 0x00):
                    return ("::CHU:FUNCTION:SHUTTER_SPEED", "");

                // ::CHU:FUNCTION:MASTER_GAIN"
                case 0x20 when (command.PARAM0 == 0x01):
                case 0x21 when (command.PARAM0 == 0x01):
                    return ("::CHU:FUNCTION:MASTER_GAIN", "");

                // ::CHU:FUNCTION:ND_FILTER"
                case 0x20 when (command.PARAM0 == 0x03):
                case 0x21 when (command.PARAM0 == 0x03):
                    return ("::CHU:FUNCTION:ND_FILTER", "");

                // ::CHU:FUNCTION:CC_FILTER"
                case 0x20 when (command.PARAM0 == 0x04):
                case 0x21 when (command.PARAM0 == 0x04):
                    return ("::CHU:FUNCTION:CC_FILTER", "");

                // ::CHU:FUNCTION:MASTER_GAMMA_SELECT"
                case 0x20 when (command.PARAM0 == 0x06):
                case 0x21 when (command.PARAM0 == 0x06):
                    return ("::CHU:FUNCTION:MASTER_GAMMA_SELECT", "");

                // ::CHU:FUNCTION:MIC1_GAIN_SELECT"
                case 0x20 when (command.PARAM0 == 0x08):
                case 0x21 when (command.PARAM0 == 0x08):
                    return ("::CHU:FUNCTION:MIC1_GAIN_SELECT", "::MENU::MAINTENANCE::CAMERA::MICROPHONEGAIN::CH01");

                //::CHU:FUNCTION:MIC2_GAIN_SELECT"
                case 0x20 when (command.PARAM0 == 0x09):
                case 0x21 when (command.PARAM0 == 0x09):
                    return ("::CHU:FUNCTION:MIC2_GAIN_SELECT", "::MENU::MAINTENANCE::CAMERA::MICROPHONEGAIN::CH02");

                // ::CHU:FUNCTION:AUTO_IRIS_WINDOW_SELECR"
                case 0x20 when (command.PARAM0 == 0x0a):
                case 0x21 when (command.PARAM0 == 0x0a):
                    return ("::CHU:FUNCTION:AUTO_IRIS_WINDOW_SELECR", "");

                // ::CHU:FUNCTION:PRESET_MTX_SELECT"
                case 0x20 when (command.PARAM0 == 0x0d):
                case 0x21 when (command.PARAM0 == 0x0d):
                    return ("::CHU:FUNCTION:PRESET_MTX_SELECT", "");

                // ::CHU:FUNCTION:STANDARD_GAMMA_TABLE_MODE"
                case 0x20 when (command.PARAM0 == 0x13):
                case 0x21 when (command.PARAM0 == 0x13):
                    return ("::CHU:FUNCTION:STANDARD_GAMMA_TABLE_MODE", "");

                // ::CHU:FUNCTION:STANDARD_GAMMA_SELECT"
                case 0x20 when (command.PARAM0 == 0x14):
                case 0x21 when (command.PARAM0 == 0x14):
                    return ("::CHU:FUNCTION:STANDARD_GAMMA_SELECT", "");

                // ::CHU:FUNCTION:SPECIAL_GAMMA_SELECT"
                case 0x20 when (command.PARAM0 == 0x15):
                case 0x21 when (command.PARAM0 == 0x15):
                    return ("::CHU:FUNCTION:SPECIAL_GAMMA_SELECT", "");

                // ::CHU:FUNCTION:HYPER_GAMMA_SELECT"
                case 0x20 when (command.PARAM0 == 0x16):
                case 0x21 when (command.PARAM0 == 0x16):
                    return ("::CHU:FUNCTION:HYPER_GAMMA_SELECT", "");

                // ::CHU:FUNCTION:USER_GAMMA_SELECT"
                case 0x20 when (command.PARAM0 == 0x17):
                case 0x21 when (command.PARAM0 == 0x17):
                    return ("::CHU:FUNCTION:USER_GAMMA_SELECT", "");

                // ::CHU:FUNCTION:BLK_GAMMA_RGB_LOW_RANGE"
                case 0x20 when (command.PARAM0 == 0x18):
                case 0x21 when (command.PARAM0 == 0x18):
                    return ("::CHU:FUNCTION:BLK_GAMMA_RGB_LOW_RANGE", "");

                // ::CHU:FUNCTION:LOW_KEY_SAT_LOW_RANGE
                case 0x20 when (command.PARAM0 == 0x1d):
                case 0x21 when (command.PARAM0 == 0x1d):
                    return ("::CHU:FUNCTION:LOW_KEY_SAT_LOW_RANGE", "");

                // ::CHU:FUNCTION:SIS_SELECT
                case 0x20 when (command.PARAM0 == 0x20):
                case 0x21 when (command.PARAM0 == 0x20):
                    return ("::CHU:FUNCTION:SIS_SELECT", "");

                // ::CHU:FUNCTION:DIGITAL_EXTENDER
                case 0x20 when (command.PARAM0 == 0x27):
                case 0x21 when (command.PARAM0 == 0x27):
                    return ("::CHU:FUNCTION:DIGITAL_EXTENDER", "");

                // ::CHU:FUNCTION:FLICKER_REDUCE_AREA_SELECT
                case 0x20 when (command.PARAM0 == 0x28):
                case 0x21 when (command.PARAM0 == 0x28):
                    return ("::CHU:FUNCTION:FLICKER_REDUCE_AREA_SELECT", "");

                // ::CHU:FUNCTION:COMPENSATION
                case 0x20 when (command.PARAM0 == 0x29):
                case 0x21 when (command.PARAM0 == 0x29):
                    return ("::CHU:FUNCTION:COMPENSATION", "");

                // ::CHU:FUNCTION:NS_LEVEL_MODE
                case 0x20 when (command.PARAM0 == 0x2a):
                case 0x21 when (command.PARAM0 == 0x2a):
                    return ("::CHU:FUNCTION:NS_LEVEL_MODE", "");

                // ::CHU:FUNCTION:FLICKER_REDUCE_AVE_MODE
                case 0x20 when (command.PARAM0 == 0x2d):
                case 0x21 when (command.PARAM0 == 0x2d):
                    return ("::CHU:FUNCTION:FLICKER_REDUCE_AVE_MODE", "");

                // ::CHU:FUNCTION:3D_CAMERA_SELECT
                case 0x20 when (command.PARAM0 == 0x2e):
                case 0x21 when (command.PARAM0 == 0x2e):
                    return ("::CHU:FUNCTION:3D_CAMERA_SELECT", "");

                // ::CHU:FUNCTION:01
                case 0x20 when (command.PARAM0 == 0x81):
                case 0x21 when (command.PARAM0 == 0x81):
                    return ("::CHU:FUNCTION:01", "");

                // ::CHU:FUNCTION:02
                case 0x20 when (command.PARAM0 == 0x82):
                case 0x21 when (command.PARAM0 == 0x82):
                    return ("::CHU:FUNCTION:02", "");

                // ::CHU:FUNCTION:03
                case 0x20 when (command.PARAM0 == 0x83):
                case 0x21 when (command.PARAM0 == 0x83):
                    return ("::CHU:FUNCTION:03", "");

                // ::CHU:FUNCTION:04
                case 0x20 when (command.PARAM0 == 0x84):
                case 0x21 when (command.PARAM0 == 0x84):
                    return ("::CHU:FUNCTION:04", "");

                // ::CHU:FUNCTION:SYSTEM_MODE
                case 0x20 when (command.PARAM0 == 0x85):
                case 0x21 when (command.PARAM0 == 0x85):
                    return ("::CHU:FUNCTION:SYSTEM_MODE", "");

                // ::CHU:FUNCTION:TEST_SIGNAL_SELECT
                case 0x20 when (command.PARAM0 == 0x86):
                case 0x21 when (command.PARAM0 == 0x86):
                    return ("::CHU:FUNCTION:TEST_SIGNAL_SELECT", "");

                // ::CHU:FUNCTION:05
                case 0x20 when (command.PARAM0 == 0x87):
                case 0x21 when (command.PARAM0 == 0x87):
                    return ("::CHU:FUNCTION:05", "");

                // ::CHU:FUNCTION:06
                case 0x20 when (command.PARAM0 == 0x89):
                case 0x21 when (command.PARAM0 == 0x89):
                    return ("::CHU:FUNCTION:06", "");

                // ::CHU:FUNCTION:07
                case 0x20 when (command.PARAM0 == 0x8b):
                case 0x21 when (command.PARAM0 == 0x8b):
                    return ("::CHU:FUNCTION:07", "");

                // ::CHU:FUNCTION:SKIN_DETAIL_CH
                case 0x20 when (command.PARAM0 == 0x8d):
                case 0x21 when (command.PARAM0 == 0x8d):
                    return ("::CHU:FUNCTION:SKIN_DETAIL_CH", "");

                // ::CHU:FUNCTION:SKIN_DETAIL_GATE_CH
                case 0x20 when (command.PARAM0 == 0x8e):
                case 0x21 when (command.PARAM0 == 0x8e):
                    return ("::CHU:FUNCTION:SKIN_DETAIL_GATE_CH", "");

                // ::CHU:FUNCTION:08
                case 0x20 when (command.PARAM0 == 0x94):
                case 0x21 when (command.PARAM0 == 0x94):
                    return ("::CHU:FUNCTION:08", "");

                // ::CHU:FUNCTION:FLICKER_REDUCTION_POWER_FREQUENCY
                case 0x20 when (command.PARAM0 == 0x99):
                case 0x21 when (command.PARAM0 == 0x99):
                    return ("::CHU:FUNCTION:FLICKER_REDUCTION_POWER_FREQUENCY", "");

                // ::CHU:FUNCTION:MODE_SW00
                case 0x20 when (command.PARAM0 == 0xa0):
                case 0x21 when (command.PARAM0 == 0xa0):
                    return ("::CHU:FUNCTION:MODE_SW00", "");

                // ::CHU:FUNCTION:MODE_SW02
                case 0x20 when (command.PARAM0 == 0xa2):
                case 0x21 when (command.PARAM0 == 0xa2):
                    return ("::CHU:FUNCTION:MODE_SW02", "");

                // ::CHU:FUNCTION:MODE_SW03
                case 0x20 when (command.PARAM0 == 0xa3):
                case 0x21 when (command.PARAM0 == 0xa3):
                    return ("::CHU:FUNCTION:MODE_SW03", "");

                // ::CHU:FUNCTION:MODE_SW04
                case 0x20 when (command.PARAM0 == 0xa4):
                case 0x21 when (command.PARAM0 == 0xa4):
                    return ("::CHU:FUNCTION:MODE_SW04", "");

                // ::CHU:FUNCTION:WHITE_R
                case 0x22 when (command.PARAM0 == 0x01):
                case 0x23 when (command.PARAM0 == 0x01):
                    return ("::CHU:FUNCTION:WHITE_R", "");

                // ::CHU:FUNCTION:WHITE_G
                case 0x22 when (command.PARAM0 == 0x02):
                case 0x23 when (command.PARAM0 == 0x02):
                    return ("::CHU:FUNCTION:WHITE_G", "");

                // ::CHU:FUNCTION:WHITE_B
                case 0x22 when (command.PARAM0 == 0x03):
                case 0x23 when (command.PARAM0 == 0x03):
                    return ("::CHU:FUNCTION:WHITE_B", "");

                // ::CHU:FUNCTION:MASTER_MOD_SHD_V_SAW
                case 0x22 when (command.PARAM0 == 0x04):
                case 0x23 when (command.PARAM0 == 0x04):
                    return ("::CHU:FUNCTION:MASTER_MOD_SHD_V_SAW", "");

                // ::CHU:FUNCTION:MOD_SHD_V_SAW_R
                case 0x22 when (command.PARAM0 == 0x05):
                case 0x23 when (command.PARAM0 == 0x05):
                    return ("::CHU:FUNCTION:MOD_SHD_V_SAW_R", "");

                // ::CHU:FUNCTION:MOD_SHD_V_SAW_G
                case 0x22 when (command.PARAM0 == 0x06):
                case 0x23 when (command.PARAM0 == 0x06):
                    return ("::CHU:FUNCTION:MOD_SHD_V_SAW_G", "");

                // ::CHU:FUNCTION:MOD_SHD_V_SAW_B
                case 0x22 when (command.PARAM0 == 0x07):
                case 0x23 when (command.PARAM0 == 0x07):
                    return ("::CHU:FUNCTION:MOD_SHD_V_SAW_B", "");

                // ::CHU:FUNCTION:MASTER_FLARE
                case 0x22 when (command.PARAM0 == 0x08):
                case 0x23 when (command.PARAM0 == 0x08):
                    return ("::CHU:FUNCTION:MASTER_FLARE", "");

                // ::CHU:FUNCTION:FLARE_R
                case 0x22 when (command.PARAM0 == 0x09):
                case 0x23 when (command.PARAM0 == 0x09):
                    return ("::CHU:FUNCTION:FLARE_R", "");

                // ::CHU:FUNCTION:FLARE_G
                case 0x22 when (command.PARAM0 == 0x0a):
                case 0x23 when (command.PARAM0 == 0x0a):
                    return ("::CHU:FUNCTION:FLARE_G", "");

                // ::CHU:FUNCTION:FLARE_B
                case 0x22 when (command.PARAM0 == 0x0b):
                case 0x23 when (command.PARAM0 == 0x0b):
                    return ("::CHU:FUNCTION:FLARE_B", "");

                // ::CHU:FUNCTION:DETAIL_LIMITER
                case 0x22 when (command.PARAM0 == 0x0c):
                case 0x23 when (command.PARAM0 == 0x0c):
                    return ("::CHU:FUNCTION:DETAIL_LIMITER", "");

                // ::CHU:FUNCTION:DETAIL_WHITE_LIMITER
                case 0x22 when (command.PARAM0 == 0x0d):
                case 0x23 when (command.PARAM0 == 0x0d):
                    return ("::CHU:FUNCTION:DETAIL_WHITE_LIMITER", "");

                // ::CHU:FUNCTION:DETAIL_BLACK_LIMITER
                case 0x22 when (command.PARAM0 == 0x0e):
                case 0x23 when (command.PARAM0 == 0x0e):
                    return ("::CHU:FUNCTION:DETAIL_BLACK_LIMITER", "");

                // ::CHU:FUNCTION:MASTER_BLACK_GAMMA
                case 0x22 when (command.PARAM0 == 0x10):
                case 0x23 when (command.PARAM0 == 0x10):
                    return ("::CHU:FUNCTION:MASTER_BLACK_GAMMA", "");

                // ::CHU:FUNCTION:BLACK_GAMMA_R
                case 0x22 when (command.PARAM0 == 0x11):
                case 0x23 when (command.PARAM0 == 0x11):
                    return ("::CHU:FUNCTION:BLACK_GAMMA_R", "");

                // ::CHU:FUNCTION:BLACK_GAMMA_G
                case 0x22 when (command.PARAM0 == 0x12):
                case 0x23 when (command.PARAM0 == 0x12):
                    return ("::CHU:FUNCTION:BLACK_GAMMA_G", "");

                // ::CHU:FUNCTION:BLACK_GAMMA_B
                case 0x22 when (command.PARAM0 == 0x13):
                case 0x23 when (command.PARAM0 == 0x13):
                    return ("::CHU:FUNCTION:BLACK_GAMMA_B", "");

                // ::CHU:FUNCTION:MASTER_KNEE_POINT
                case 0x22 when (command.PARAM0 == 0x14):
                case 0x23 when (command.PARAM0 == 0x14):
                    return ("::CHU:FUNCTION:MASTER_KNEE_POINT", "");

                // ::CHU:FUNCTION:KNEE_POINT_R
                case 0x22 when (command.PARAM0 == 0x15):
                case 0x23 when (command.PARAM0 == 0x15):
                    return ("::CHU:FUNCTION:KNEE_POINT_R", "");

                // ::CHU:FUNCTION:KNEE_POINT_G
                case 0x22 when (command.PARAM0 == 0x16):
                case 0x23 when (command.PARAM0 == 0x16):
                    return ("::CHU:FUNCTION:KNEE_POINT_G", "");

                // ::CHU:FUNCTION:KNEE_POINT_B
                case 0x22 when (command.PARAM0 == 0x17):
                case 0x23 when (command.PARAM0 == 0x17):
                    return ("::CHU:FUNCTION:KNEE_POINT_B", "");

                // ::CHU:FUNCTION:MASTER_KNEE_SLOPE
                case 0x22 when (command.PARAM0 == 0x18):
                case 0x23 when (command.PARAM0 == 0x18):
                    return ("::CHU:FUNCTION:MASTER_KNEE_SLOPE", "");

                // ::CHU:FUNCTION:KNEE_SLOPE_R
                case 0x22 when (command.PARAM0 == 0x19):
                case 0x23 when (command.PARAM0 == 0x19):
                    return ("::CHU:FUNCTION:KNEE_SLOPE_R", "");

                // ::CHU:FUNCTION:KNEE_SLOPE_G
                case 0x22 when (command.PARAM0 == 0x1a):
                case 0x23 when (command.PARAM0 == 0x1a):
                    return ("::CHU:FUNCTION:KNEE_SLOPE_G", "");

                // ::CHU:FUNCTION:KNEE_SLOPE_B
                case 0x22 when (command.PARAM0 == 0x1b):
                case 0x23 when (command.PARAM0 == 0x1b):
                    return ("::CHU:FUNCTION:KNEE_SLOPE_B", "");

                // ::CHU:FUNCTION:MASTER_GAMMA
                case 0x22 when (command.PARAM0 == 0x1c):
                case 0x23 when (command.PARAM0 == 0x1c):
                    return ("::CHU:FUNCTION:MASTER_GAMMA", "");

                // ::CHU:FUNCTION:GAMMA_R
                case 0x22 when (command.PARAM0 == 0x1d):
                case 0x23 when (command.PARAM0 == 0x1d):
                    return ("::CHU:FUNCTION:GAMMA_R", "");

                // ::CHU:FUNCTION:GAMMA_G
                case 0x22 when (command.PARAM0 == 0x1e):
                case 0x23 when (command.PARAM0 == 0x1e):
                    return ("::CHU:FUNCTION:GAMMA_G", "");

                // ::CHU:FUNCTION:GAMMA_B
                case 0x22 when (command.PARAM0 == 0x1f):
                case 0x23 when (command.PARAM0 == 0x1f):
                    return ("::CHU:FUNCTION:GAMMA_B", "");

                // ::CHU:FUNCTION:MASTER_WHITE_CLIP
                case 0x22 when (command.PARAM0 == 0x20):
                case 0x23 when (command.PARAM0 == 0x20):
                    return ("::CHU:FUNCTION:MASTER_WHITE_CLIP", "");

                // ::CHU:FUNCTION:WHITE_CLIP_R
                case 0x22 when (command.PARAM0 == 0x21):
                case 0x23 when (command.PARAM0 == 0x21):
                    return ("::CHU:FUNCTION:WHITE_CLIP_R", "");

                // ::CHU:FUNCTION:WHITE_CLIP_G
                case 0x22 when (command.PARAM0 == 0x22):
                case 0x23 when (command.PARAM0 == 0x22):
                    return ("::CHU:FUNCTION:WHITE_CLIP_G", "");

                // ::CHU:FUNCTION:WHITE_CLIP_B
                case 0x22 when (command.PARAM0 == 0x23):
                case 0x23 when (command.PARAM0 == 0x23):
                    return ("::CHU:FUNCTION:WHITE_CLIP_B", "");

                // ::CHU:FUNCTION:FLICKER_REDUCE_GAIN_M
                case 0x22 when (command.PARAM0 == 0x24):
                case 0x23 when (command.PARAM0 == 0x24):
                    return ("::CHU:FUNCTION:FLICKER_REDUCE_GAIN_M", "");

                // ::CHU:FUNCTION:FLICKER_REDUCE_OFS_M
                case 0x22 when (command.PARAM0 == 0x28):
                case 0x23 when (command.PARAM0 == 0x28):
                    return ("::CHU:FUNCTION:FLICKER_REDUCE_OFS_M", "");

                // ::CHU:FUNCTION:ECS_FREQUENCY
                case 0x22 when (command.PARAM0 == 0x41):
                case 0x23 when (command.PARAM0 == 0x41):
                    return ("::CHU:FUNCTION:ECS_FREQUENCY", "");

                // ::CHU:FUNCTION:EVS_DATA
                case 0x22 when (command.PARAM0 == 0x42):
                case 0x23 when (command.PARAM0 == 0x42):
                    return ("::CHU:FUNCTION:EVS_DATA", "");

                // ::CHU:FUNCTION:SKIN_DETAIL_PHASE
                case 0x22 when (command.PARAM0 == 0x43):
                case 0x23 when (command.PARAM0 == 0x43):
                    return ("::CHU:FUNCTION:SKIN_DETAIL_PHASE", "");

                // ::CHU:FUNCTION:SKIN_DETAIL_WIDTH
                case 0x22 when (command.PARAM0 == 0x44):
                case 0x23 when (command.PARAM0 == 0x44):
                    return ("::CHU:FUNCTION:SKIN_DETAIL_WIDTH", "");

                // ::CHU:FUNCTION:OPTICAL_LEVEL
                case 0x22 when (command.PARAM0 == 0x47):
                case 0x23 when (command.PARAM0 == 0x47):
                    return ("::CHU:FUNCTION:OPTICAL_LEVEL", "");

                // ::CHU:FUNCTION:SKIN_DETAIL2_PHASE
                case 0x22 when (command.PARAM0 == 0x54):
                case 0x23 when (command.PARAM0 == 0x54):
                    return ("::CHU:FUNCTION:SKIN_DETAIL2_PHASE", "");

                // ::CHU:FUNCTION:SKIN_DETAIL2_WIDTH
                case 0x22 when (command.PARAM0 == 0x55):
                case 0x23 when (command.PARAM0 == 0x55):
                    return ("::CHU:FUNCTION:SKIN_DETAIL2_WIDTH", "");

                // ::CHU:FUNCTION:SKIN_DETAIL3_PHASE
                case 0x22 when (command.PARAM0 == 0x56):
                case 0x23 when (command.PARAM0 == 0x56):
                    return ("::CHU:FUNCTION:SKIN_DETAIL3_PHASE", "");

                // ::CHU:FUNCTION:SKIN_DETAIL3_WIDTH
                case 0x22 when (command.PARAM0 == 0x57):
                case 0x23 when (command.PARAM0 == 0x57):
                    return ("::CHU:FUNCTION:SKIN_DETAIL3_WIDTH", "");

                // ::CHU:FUNCTION:IRIS
                case 0x22 when (command.PARAM0 == 0x60):
                case 0x23 when (command.PARAM0 == 0x60):
                    return ("::CHU:FUNCTION:IRIS", "");

                // ::CHU:FUNCTION:DETAIL_LEVEL
                case 0x22 when (command.PARAM0 == 0x9b):
                case 0x23 when (command.PARAM0 == 0x9b):
                    return ("::CHU:FUNCTION:DETAIL_LEVEL", "");

                // ::CHU:FUNCTION:DETAIL_CRISPENING
                case 0x22 when (command.PARAM0 == 0x9c):
                case 0x23 when (command.PARAM0 == 0x9c):
                    return ("::CHU:FUNCTION:DETAIL_CRISPENING", "");

                // ::CHU:FUNCTION:DETAIL_MIX_RATIO
                case 0x22 when (command.PARAM0 == 0x9d):
                case 0x23 when (command.PARAM0 == 0x9d):
                    return ("::CHU:FUNCTION:DETAIL_MIX_RATIO", "");

                // ::CHU:FUNCTION:DETAIL_HV_RATIO
                case 0x22 when (command.PARAM0 == 0x9e):
                case 0x23 when (command.PARAM0 == 0x9e):
                    return ("::CHU:FUNCTION:DETAIL_HV_RATIO", "");

                // ::CHU:FUNCTION:H_DETAIL_HL_RATIO
                case 0x22 when (command.PARAM0 == 0x9f):
                case 0x23 when (command.PARAM0 == 0x9f):
                    return ("::CHU:FUNCTION:H_DETAIL_HL_RATIO", "");

                // ::CHU:FUNCTION:DETAIL_LEVEL_DEPEND
                case 0x22 when (command.PARAM0 == 0xa0):
                case 0x23 when (command.PARAM0 == 0xa0):
                    return ("::CHU:FUNCTION:DETAIL_LEVEL_DEPEND", "");

                // ::CHU:FUNCTION:SKIN_DETAIL_LEVEL
                case 0x22 when (command.PARAM0 == 0xa1):
                case 0x23 when (command.PARAM0 == 0xa1):
                    return ("::CHU:FUNCTION:SKIN_DETAIL_LEVEL", "");

                // ::CHU:FUNCTION:SKIN_DETAIL_SAT
                case 0x22 when (command.PARAM0 == 0xa2):
                case 0x23 when (command.PARAM0 == 0xa2):
                    return ("::CHU:FUNCTION:SKIN_DETAIL_SAT", "");

                // ::CHU:FUNCTION:MATRIX_GR_R
                case 0x22 when (command.PARAM0 == 0xa3):
                case 0x23 when (command.PARAM0 == 0xa3):
                    return ("::CHU:FUNCTION:MATRIX_GR_R", "");

                // ::CHU:FUNCTION:MATRIX_BR_R
                case 0x22 when (command.PARAM0 == 0xa4):
                case 0x23 when (command.PARAM0 == 0xa4):
                    return ("::CHU:FUNCTION:MATRIX_BR_R", "");

                // ::CHU:FUNCTION:MATRIX_RG_G
                case 0x22 when (command.PARAM0 == 0xa5):
                case 0x23 when (command.PARAM0 == 0xa5):
                    return ("::CHU:FUNCTION:MATRIX_RG_G", "");

                // ::CHU:FUNCTION:MATRIX_BG_G
                case 0x22 when (command.PARAM0 == 0xa6):
                case 0x23 when (command.PARAM0 == 0xa6):
                    return ("::CHU:FUNCTION:MATRIX_BG_G", "");

                // ::CHU:FUNCTION:MATRIX_RB_B
                case 0x22 when (command.PARAM0 == 0xa7):
                case 0x23 when (command.PARAM0 == 0xa7):
                    return ("::CHU:FUNCTION:MATRIX_RB_B", "");

                // ::CHU:FUNCTION:MATRIX_GB_B
                case 0x22 when (command.PARAM0 == 0xa8):
                case 0x23 when (command.PARAM0 == 0xa8):
                    return ("::CHU:FUNCTION:MATRIX_GB_B", "");

                // ::CHU:FUNCTION:MASTER_BLACK
                case 0x22 when (command.PARAM0 == 0xa9):
                case 0x23 when (command.PARAM0 == 0xa9):
                    return ("::CHU:FUNCTION:MASTER_BLACK", "");

                // ::CHU:FUNCTION:BLACK_R
                case 0x22 when (command.PARAM0 == 0xaa):
                case 0x23 when (command.PARAM0 == 0xaa):
                    return ("::CHU:FUNCTION:BLACK_R", "");

                // ::CHU:FUNCTION:BLACK_G
                case 0x22 when (command.PARAM0 == 0xab):
                case 0x23 when (command.PARAM0 == 0xab):
                    return ("::CHU:FUNCTION:BLACK_G", "");

                // ::CHU:FUNCTION:BLACK_B
                case 0x22 when (command.PARAM0 == 0xac):
                case 0x23 when (command.PARAM0 == 0xac):
                    return ("::CHU:FUNCTION:BLACK_B", "");


                // ::CHU:FUNCTION:KNEE_SAT_SLOPE
                case 0x22 when (command.PARAM0 == 0xae):
                case 0x23 when (command.PARAM0 == 0xae):
                    return ("::CHU:FUNCTION:KNEE_SAT_SLOPE", "");

                // ::CHU:FUNCTION:KNEE_APERTURE
                case 0x22 when (command.PARAM0 == 0xaf):
                case 0x23 when (command.PARAM0 == 0xaf):
                    return ("::CHU:FUNCTION:KNEE_APERTURE", "");

                // ::CHU:FUNCTION:COMB_FILTER
                case 0x22 when (command.PARAM0 == 0xb0):
                case 0x23 when (command.PARAM0 == 0xb0):
                    return ("::CHU:FUNCTION:COMB_FILTER", "");

                // ::CHU:FUNCTION:LOW_KEY_CLIP_LEVEL
                case 0x22 when (command.PARAM0 == 0xb7):
                case 0x23 when (command.PARAM0 == 0xb7):
                    return ("::CHU:FUNCTION:LOW_KEY_CLIP_LEVEL", "");

                // ::CHU:FUNCTION:ADAPTIVE_KNEE_POINT
                case 0x22 when (command.PARAM0 == 0xc4):
                case 0x23 when (command.PARAM0 == 0xc4):
                    return ("::CHU:FUNCTION:ADAPTIVE_KNEE_POINT", "");

                // ::CHU:FUNCTION:ADAPTIVE_KNEE_SLOPE
                case 0x22 when (command.PARAM0 == 0xc5):
                case 0x23 when (command.PARAM0 == 0xc5):
                    return ("::CHU:FUNCTION:ADAPTIVE_KNEE_SLOPE", "");

                // ::CHU:FUNCTION:SLIM_DETAIL
                case 0x22 when (command.PARAM0 == 0xc6):
                case 0x23 when (command.PARAM0 == 0xc6):
                    return ("::CHU:FUNCTION:SLIM_DETAIL", "");

                // ::CHU:FUNCTION:SKIN_DETAIL2_LEVEL
                case 0x22 when (command.PARAM0 == 0xc7):
                case 0x23 when (command.PARAM0 == 0xc7):
                    return ("::CHU:FUNCTION:SKIN_DETAIL2_LEVEL", "");

                // ::CHU:FUNCTION:SKIN_DETAIL2_SAT
                case 0x22 when (command.PARAM0 == 0xc8):
                case 0x23 when (command.PARAM0 == 0xc8):
                    return ("::CHU:FUNCTION:SKIN_DETAIL2_SAT", "");

                // ::CHU:FUNCTION:SKIN_DETAIL3_LEVEL
                case 0x22 when (command.PARAM0 == 0xc9):
                case 0x23 when (command.PARAM0 == 0xc9):
                    return ("::CHU:FUNCTION:SKIN_DETAIL3_LEVEL", "");

                // ::CHU:FUNCTION:SKIN_DETAIL3_SAT
                case 0x22 when (command.PARAM0 == 0xca):
                case 0x23 when (command.PARAM0 == 0xca):
                    return ("::CHU:FUNCTION:SKIN_DETAIL3_SAT", "");

                // ::CHU:FUNCTION:SATURATION
                case 0x22 when (command.PARAM0 == 0xd2):
                case 0x23 when (command.PARAM0 == 0xd2):
                    return ("::CHU:FUNCTION:SATURATION", "");


                // ::CHU:FUNCTION:WHITE_COLOR_TEMP_CTRL
                case 0x22 when (command.PARAM0 == 0xdc):
                case 0x23 when (command.PARAM0 == 0xdc):
                    return ("::CHU:FUNCTION:WHITE_COLOR_TEMP_CTRL", "");


                // ::CHU:FUNCTION:COLOR_TEMP_BALANCE
                case 0x22 when (command.PARAM0 == 0xde):
                case 0x23 when (command.PARAM0 == 0xde):
                    return ("::CHU:FUNCTION:COLOR_TEMP_BALANCE", "");


                // ::CHU:FUNCTION:SELECT_FPS
                case 0x22 when (command.PARAM0 == 0xdf):
                case 0x23 when (command.PARAM0 == 0xdf):
                    return ("::CHU:FUNCTION:SELECT_FPS", "");

                // ::CHU:FUNCTION:SD_DETAIL_LEVEL
                case 0x22 when (command.PARAM0 == 0xe0):
                case 0x23 when (command.PARAM0 == 0xe0):
                    return ("::CHU:FUNCTION:SD_DETAIL_LEVEL", "");

                // ::CHU:FUNCTION:SD_DETAIL_CRISPENING
                case 0x22 when (command.PARAM0 == 0xe1):
                case 0x23 when (command.PARAM0 == 0xe1):
                    return ("::CHU:FUNCTION:SD_DETAIL_CRISPENING", "");

                // ::CHU:FUNCTION:SD_DETAIL_HV_RATIO
                case 0x22 when (command.PARAM0 == 0xe2):
                case 0x23 when (command.PARAM0 == 0xe2):
                    return ("::CHU:FUNCTION:SD_DETAIL_HV_RATIO", "");

                // ::CHU:FUNCTION:SD_DETAIL_LIMITTER
                case 0x22 when (command.PARAM0 == 0xe3):
                case 0x23 when (command.PARAM0 == 0xe3):
                    return ("::CHU:FUNCTION:SD_DETAIL_LIMITTER", "");

                // ::CHU:FUNCTION:SD_DETAIL_WHITE_LIMITTER
                case 0x22 when (command.PARAM0 == 0xe4):
                case 0x23 when (command.PARAM0 == 0xe4):
                    return ("::CHU:FUNCTION:SD_DETAIL_WHITE_LIMITTER", "");

                // ::CHU:FUNCTION:SD_DETAIL_BLACK_LIMITTER
                case 0x22 when (command.PARAM0 == 0xe5):
                case 0x23 when (command.PARAM0 == 0xe5):
                    return ("::CHU:FUNCTION:SD_DETAIL_BLACK_LIMITTER", "");

                // ::CHU:FUNCTION:SD_DETAIL_FREQUENCY
                case 0x22 when (command.PARAM0 == 0xe6):
                case 0x23 when (command.PARAM0 == 0xe6):
                    return ("::CHU:FUNCTION:SD_DETAIL_FREQUENCY", "");

                // ::CHU:FUNCTION:SD_DETAIL_LEVEL_DEPEND
                case 0x22 when (command.PARAM0 == 0xe7):
                case 0x23 when (command.PARAM0 == 0xe7):
                    return ("::CHU:FUNCTION:SD_DETAIL_LEVEL_DEPEND", "");

                // ::CHU:FUNCTION:SD_DETAIL_COMB
                case 0x22 when (command.PARAM0 == 0xeb):
                case 0x23 when (command.PARAM0 == 0xeb):
                    return ("::CHU:FUNCTION:SD_DETAIL_COMB", "");

                // ::CHU:FUNCTION:MASTER_WHITE_GAIN
                case 0x22 when (command.PARAM0 == 0xf2):
                case 0x23 when (command.PARAM0 == 0xf2):
                    return ("::CHU:FUNCTION:MASTER_WHITE_GAIN", "");

                #endregion

                #region ::CCU:FUNCTION:

                // ::CCU:FUNCTION:CHARACTER_NEXT_PAGE
                case 0x40 when (command.PARAM0 == 0x00):
                case 0x41 when (command.PARAM0 == 0x00):
                    return ("::CCU:FUNCTION:CHARACTER_NEXT_PAGE", "Switches to the next page character output of the CCU.");

                // ::CCU:FUNCTION:GENLOCK_MODE
                case 0x40 when (command.PARAM0 == 0x0a):
                case 0x41 when (command.PARAM0 == 0x0a):
                    return ("::CCU:FUNCTION:GENLOCK_MODE", "Selects the type of signal using synchronization.");

                // ::CCU:FUNCTION:00
                case 0x40 when (command.PARAM0 == 0x10):
                case 0x41 when (command.PARAM0 == 0x10):
                    return ("::CCU:FUNCTION:00", "");

                // ::CCU:FUNCTION:CAM_PW
                case 0x40 when (command.PARAM0 == 0x11):
                case 0x41 when (command.PARAM0 == 0x11):
                    return ("::CCU:FUNCTION:CAM_PW", "");

                // ::CCU:FUNCTION:01
                case 0x40 when (command.PARAM0 == 0x12):
                case 0x41 when (command.PARAM0 == 0x12):
                    return ("::CCU:FUNCTION:01", "");

                // ::CCU:FUNCTION:BARS_CHARACTER
                case 0x40 when (command.PARAM0 == 0x1a):
                case 0x41 when (command.PARAM0 == 0x1a):
                    return ("::CCU:FUNCTION:BARS_CHARACTER", "Add characters to color bars signals.");

                // ::CCU:FUNCTION:PREVIEW
                case 0x40 when (command.PARAM0 == 0x31):
                case 0x41 when (command.PARAM0 == 0x31):
                    return ("::CCU:FUNCTION:PREVIEW_CONTROL", "");

                // ::CCU:FUNCTION:MENU_CONTROL
                case 0x40 when (command.PARAM0 == 0x32):
                case 0x41 when (command.PARAM0 == 0x32):
                    return ("::CCU:FUNCTION:MENU_CONTROL", "");

                // ::CCU:FUNCTION:SD_LETTERBOX_MODE
                case 0x40 when (command.PARAM0 == 0x40):
                case 0x41 when (command.PARAM0 == 0x40):
                    return ("::CCU:FUNCTION:SD_LETTERBOX_MODE", "");

                // ::CCU:FUNCTION:CHANNEL_ID
                case 0x40 when (command.PARAM0 == 0x83):
                case 0x41 when (command.PARAM0 == 0x83):
                    return ("::CCU:FUNCTION:CHANNEL_ID", "Sets the Channel ID display for direct output. Turns ON the Channel ID display for direct output.");

                // ::CCU:FUNCTION:SD_FUNCTION_02
                case 0x40 when (command.PARAM0 == 0xc2):
                case 0x41 when (command.PARAM0 == 0xc2):
                    return ("::CCU:FUNCTION:SD_FUNCTION_02", "");

                // ::CCU:FUNCTION:SD_FUNCTION_03
                case 0x40 when (command.PARAM0 == 0xc3):
                case 0x41 when (command.PARAM0 == 0xc3):
                    return ("::CCU:FUNCTION:SD_FUNCTION_03", "");

                // ::CCU:FUNCTION:CROP_CONTROL
                case 0x40 when (command.PARAM0 == 0xe0):
                case 0x41 when (command.PARAM0 == 0xe0):
                    return ("::CCU:FUNCTION:CROP_CONTROL", "");

                // ::CCU:FUNCTION:MONO_SATURATION
                case 0x42 when (command.PARAM0 == 0x07):
                case 0x43 when (command.PARAM0 == 0x07):
                    return ("::CCU:FUNCTION:MONO_SATURATION", "");

                // ::CCU:FUNCTION:MONO_HUE
                case 0x42 when (command.PARAM0 == 0x08):
                case 0x43 when (command.PARAM0 == 0x08):
                    return ("::CCU:FUNCTION:MONO_HUE", "");

                // ::CCU:FUNCTION:CROP_POSITION
                case 0x42 when (command.PARAM0 == 0x70):
                case 0x43 when (command.PARAM0 == 0x70):
                    return ("::CCU:FUNCTION:CROP_POSITION", "");

                // ::CCU:FUNCTION:CROP_POSITION
                case 0x42 when (command.PARAM0 == 0x74):
                case 0x43 when (command.PARAM0 == 0x74):
                    return ("::CCU:FUNCTION:MENU_CONTROL_ARCDIAL", "");

                // ::CCU:FUNCTION:CHANNEL_ID
                case 0x40 when (command.PARAM0 == 0x83):
                case 0x41 when (command.PARAM0 == 0x83):
                    return ("::CCU:FUNCTION:CHANNEL_ID", "Add characters to color bars signals.");

                // ::CCU:FUNCTION:SD_DETAIL_LIMITER
                case 0x42 when (command.PARAM0 == 0x8c):
                case 0x43 when (command.PARAM0 == 0x8c):
                    return ("::CCU:FUNCTION:SD_DETAIL_LIMITER", "");

                // ::CCU:FUNCTION:SD_DETAIL_WHITE_LIMITER
                case 0x42 when (command.PARAM0 == 0x8d):
                case 0x43 when (command.PARAM0 == 0x8d):
                    return ("::CCU:FUNCTION:SD_DETAIL_WHITE_LIMITER", "");

                // ::CCU:FUNCTION:SD_DETAIL_BLACK_LIMITER
                case 0x42 when (command.PARAM0 == 0x8e):
                case 0x43 when (command.PARAM0 == 0x8e):
                    return ("::CCU:FUNCTION:SD_DETAIL_BLACK_LIMITER", "");

                // ::CCU:FUNCTION:SD_MASTER_GAMMA
                case 0x42 when (command.PARAM0 == 0x9c):
                case 0x43 when (command.PARAM0 == 0x9c):
                    return ("::CCU:FUNCTION:SD_MASTER_GAMMA", "");

                // ::CCU:FUNCTION:SD_MATRIX_GR_R
                case 0x42 when (command.PARAM0 == 0xa3):
                case 0x43 when (command.PARAM0 == 0xa3):
                    return ("::CCU:FUNCTION:SD_MATRIX_GR_R", "");

                // ::CCU:FUNCTION:SD_MATRIX_BR_R
                case 0x42 when (command.PARAM0 == 0xa4):
                case 0x43 when (command.PARAM0 == 0xa4):
                    return ("::CCU:FUNCTION:SD_MATRIX_BR_R", "");

                // ::CCU:FUNCTION:SD_MATRIX_RG_G
                case 0x42 when (command.PARAM0 == 0xa5):
                case 0x43 when (command.PARAM0 == 0xa5):
                    return ("::CCU:FUNCTION:SD_MATRIX_RG_G", "");

                // ::CCU:FUNCTION:SD_MATRIX_BG_G
                case 0x42 when (command.PARAM0 == 0xa6):
                case 0x43 when (command.PARAM0 == 0xa6):
                    return ("::CCU:FUNCTION:SD_MATRIX_BG_G", "");

                // ::CCU:FUNCTION:SD_MATRIX_RB_B
                case 0x42 when (command.PARAM0 == 0xa7):
                case 0x43 when (command.PARAM0 == 0xa7):
                    return ("::CCU:FUNCTION:SD_MATRIX_RB_B", "");

                // ::CCU:FUNCTION:SD_MATRIX_GB_B
                case 0x42 when (command.PARAM0 == 0xa8):
                case 0x43 when (command.PARAM0 == 0xa8):
                    return ("::CCU:FUNCTION:SD_MATRIX_GB_B", "");

                // ::CCU:FUNCTION:SD_DETAIL_COMB
                case 0x42 when (command.PARAM0 == 0xb0):
                case 0x43 when (command.PARAM0 == 0xb0):
                    return ("::CCU:FUNCTION:SD_DETAIL_COMB", "");

                // ::CCU:FUNCTION:SD_DETAIL_LEVEL
                case 0x42 when (command.PARAM0 == 0xdb):
                case 0x43 when (command.PARAM0 == 0xdb):
                    return ("::CCU:FUNCTION:SD_DETAIL_LEVEL", "");

                // ::CCU:FUNCTION:SD_DETAIL_CRISPENING
                case 0x42 when (command.PARAM0 == 0xdc):
                case 0x43 when (command.PARAM0 == 0xdc):
                    return ("::CCU:FUNCTION:SD_DETAIL_CRISPENING", "");

                // ::CCU:FUNCTION:SD_DETAIL_HV_RATIO
                case 0x42 when (command.PARAM0 == 0xde):
                case 0x43 when (command.PARAM0 == 0xde):
                    return ("::CCU:FUNCTION:SD_DETAIL_HV_RATIO", "");

                // ::CCU:FUNCTION:SD_DETAIL_FREQUENCY
                case 0x42 when (command.PARAM0 == 0xdf):
                case 0x43 when (command.PARAM0 == 0xdf):
                    return ("::CCU:FUNCTION:SD_DETAIL_FREQUENCY", "");

                // ::CCU:FUNCTION:SD_DETAIL_LEVEL_DEPEND
                case 0x42 when (command.PARAM0 == 0xe0):
                case 0x43 when (command.PARAM0 == 0xe0):
                    return ("::CCU:FUNCTION:SD_DETAIL_LEVEL_DEPEND", "");

                // ::CCU:FUNCTION:OPTICAL_LEVEL
                case 0x42 when (command.PARAM0 == 0xf0):
                case 0x43 when (command.PARAM0 == 0xf0):
                    return ("::CCU:FUNCTION:OPTICAL_LEVEL", "");

                #endregion



                default:
                    return ("UNKNOWN","");
            }   
        }
    }

}
