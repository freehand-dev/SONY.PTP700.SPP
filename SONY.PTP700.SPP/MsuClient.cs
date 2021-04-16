
using Microsoft.Extensions.Logging;
using SONY.PTP700.SPP.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SONY.PTP700.SPP
{
    public class MsuClient : CnsClient
    {
        public static uint _SerialNumber = 0x0001b033;

        // trigger to change rcp assiment
        public event EventHandler<RcpAssigmentEventArgs> OnChangeAssigment;

        // trigger to change permision control
        public event EventHandler<PermissionControlEventArgs> OnChangePermisionControl;


        private readonly ILogger _logger;

        private CcuClient _ccuClient;


        public override UInt32 SerialNumber
        {
            get
            {
                return base.SerialNumber;
            }
            set
            {
                base.SerialNumber = value;
                this._ccuClient.SerialNumber = value;
            }
        }

        public CcuClient CcuClient {
            get {
                return _ccuClient;
            }
        }


        public byte RcpId { get; set; } = 0;

        public MsuClient(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            // ILoggerFactory
            _logger = loggerFactory.CreateLogger("MsuClient");

            // 
            this._ccuClient = new CcuClient(loggerFactory);
            this._ccuClient.Mode = CNSMode.MCS;

            this.Mode = CNSMode.MCS;
            this.RequestID = 0x6469;
            this.SerialNumber = MsuClient._SerialNumber;
            this.OnHandShake += EventOnHandShake;
            this.OnMessageResponse += EventOnMessageResponse;
            this.OnMessage += EventOnMessage;
        } 

        internal void EventOnMessageResponse(Object sender, PacketReceivedEventArgs args)
        {
             PacketFactory.MessageResponse _packet = (PacketFactory.MessageResponse)args.Packet;
        }

        internal void EventOnHandShake(Object sender, PacketReceivedEventArgs args)
        {

        }

        internal void EventOnMessage(Object sender, PacketReceivedEventArgs args)
        {
            if (args.Packet is PacketFactory.Message20)
            {
                try
                {
                    //_logger.Debug($"[EventOnMessage]!!!!!!!!!!![Message20]  { Utils.ByteUtils.ToHexString(args.Packet.Payload) }");
                    var commands = ((PacketFactory.Message20)args.Packet).Commands.ToArray();

                    for (int i = 0; i < commands.Length; i++)
                    {
                        // assigned ccu no (d9 fe da 00 58 00 02)
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0xda }, 0))
                        {
                            this.DoChangeAssigment(commands[i].ToArray()[5]);
                            continue;
                        }

                        // d9 fe 12 90 00 81       |    d9fe1278 0191 c0a80089
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x12 }, 0))
                        {
                            // assigned_ccu_no = commands[i].ToArray()[5];
                            continue;
                        }

                        // d9 fe 02 90 00 80
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x02 }, 0))
                        {
                            //assigned_ccu_no = commands[i].ToArray()[5];
                            continue;
                        }

                        // d9fe11ff00a0
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x11 }, 0))
                        {
                            //assigned_ccu_no = commands[i].ToArray()[5];
                            continue;
                        }

                        // Request For Assign
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x11 }, 0))
                        {
                            //assigned_ccu_no = commands[i].ToArray()[5];
                            continue;
                        }

                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"[EventOnMessage][Message20] { e.Message }");
                } 
                finally
                {
                    this.BasicResponse();
                }
            }
            else
            if (args.Packet is PacketFactory.Message21)
            {
                this.BasicResponse();
            }
            else
            if (args.Packet is PacketFactory.Message30)
            {
                try
                {
                    var commands = ((PacketFactory.Message30)args.Packet).Commands.ToArray();
                    PacketFactory.Message30.PermissionControl permisions = default;
                    for (int i = 0; i < commands.Length; i++)
                    {
                        switch (commands[i].CMD_GP)
                        {
                            case 0x01:
                                break;
                            case 0x02 when commands[i].PARAM0 == 0x82:
                                permisions |= PacketFactory.Message30.PermissionControl.IrisActive;
                                break;
                            case 0x03 when commands[i].PARAM0 == 0x82:
                                permisions |= PacketFactory.Message30.PermissionControl.ParaActive;
                                break;
                            case 0x81:
                                break;
                            case 0x82:
                                break;
                            default:
                                break;
                        }
                    }

                    this.DoChangePermision(permisions);

                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"[EventOnMessage][Message30] { e.Message }");
                }
                finally
                {
                    this.BasicResponse();
                }
            } else
            if (args.Packet is PacketFactory.BasicMessage)
            {
                this.BasicResponse();
                PacketFactory.BasicMessage _packet = (PacketFactory.BasicMessage)args.Packet;
            }
        }

        public override void Connect()
        {
            // handshake
            base.Connect();

            // register rcp_no in msu
            if (!this.Register())
            {
                throw new SystemException($"Failed register RCP in MSU");
            }

            // get assigned ccu_no
            int assigned_ccu_no = this.GetAssignedCCU();


           // this.ConnectToCcu(assigned_ccu_no);

        }

        public void Disconnect()
        {
            // disconnect from ccu
            this._ccuClient.Disconnect();

            // disconnect from msu
            base.Disconnect();
        }


        public async Task ConnectToCcuAsync(int CcuNo)
        {
            await Task.Run(() => ConnectToCcu(CcuNo));
        }

        public void ConnectToCcu(int CcuNo)
        {
            // set ccu_id  in CCuClient
            _ccuClient.CcuId = ((CcuNo == -1) ? this.RcpId : (byte)CcuNo);


            // get ccu ip address by ccu_no
            IPAddress assigned_ccu_ip = this.GetCcuIpAddress(_ccuClient.CcuId);
            if (assigned_ccu_ip != null && !assigned_ccu_ip.Equals(IPAddress.Parse("0.0.0.0")))
            {
                this.GetPermission();

                this._ccuClient.Host = assigned_ccu_ip;
                this._ccuClient.RequestID = 0x7b94;
                this._ccuClient.RcpId = this.RcpId;
                try
                {
                    this._ccuClient.Connect();
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"!!!!!!!!!Connected error { e.Message }");
                }
            }
        }


        public bool Register()
        {

            bool result = default;

            byte[] _rcpID = BitConverter.GetBytes(
                Utils.ByteUtils.ReverseBytes(
                    Utils.SppUtils.RcpIdToRaw(this.RcpId, false)));

            PacketFactory.Message01 _packet = new PacketFactory.Message01(this.RequestID)
            {
                Block = new byte[] { 0x00, 0x02, 0x90, _rcpID[0], _rcpID[1] }
            };

            // send request
            var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
            PacketFactory.Message01 _message = (_response?.NextPacket as PacketFactory.Message01);
            if (_message != null)
            {
                // 0f 02 6469   0e 06 7947      01 80       000a - conflict rcp_no
                // 0f 02 6469   0e 0a 9072      01 81       d9 fe 12  90    0191 (rcp_no & flag)
                //get rcp id and enabled flag 
                if (_message.Payload[3] == 0x81)
                {
                    var commands = _message.Commands.ToArray();

                    for (int i = 0; i < commands.Length; i++)
                    {
                        // d9 fe 12 90 00 81
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x12 }, 0))
                        {
                            ushort rcp_no = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(commands[i].ToArray(), 4));
                            result = Utils.SppUtils.ParseRcpID(rcp_no).id == this.RcpId;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        // Get Permision
        public PacketFactory.Message30.PermissionControl GetPermission()
        {
            PacketFactory.Message30.PermissionControl result = default;

            PacketFactory.Message30 _packet = new PacketFactory.Message30(this.RequestID)
            {
                CcuNo = this.CcuClient.CcuId,
                RcpNo = this.RcpId,
                RcpActiveFlag = true,

            };

            _packet.Commands.Add(
                new PacketFactory.Message30.SPpCommandPair(0x81, 0x00));
            _packet.Commands.Add(
                new PacketFactory.Message30.SPpCommandPair(0x82, 0x00));
            _packet.Commands.Add(
                new PacketFactory.Message30.SPpCommandPair(0x03, 0x00));
         
            var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
            if (_response?.NextPacket != null)
            {
                var commands = ((PacketFactory.Message30)_response.NextPacket).Commands.ToArray();

                for (int i = 0; i < commands.Length; i++)
                {
                    switch (commands[i].CMD_GP)
                    {
                        case 0x01:
                            break;
                        case 0x02:
                            if (commands[i].PARAM0 == 0x82)
                                result |= PacketFactory.Message30.PermissionControl.IrisActive;
                            break;
                        case 0x03:
                            if (commands[i].PARAM0 == 0x82)
                                result |= PacketFactory.Message30.PermissionControl.ParaActive;
                            break;
                        case 0x81:
                            break;
                        case 0x82:
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task<PacketFactory.Message30.PermissionControl> GetPermissionAsync()
        {
            var x = await Task.Run(() => this.GetPermission());
            return x;
        }


        // Get CCU number assigned to RCP
        public int GetAssignedCCU()
        {

            int assigned_ccu_no = -1;

     
            byte[] _rcpID = BitConverter.GetBytes(
                Utils.ByteUtils.ReverseBytes(
                    Utils.SppUtils.RcpIdToRaw(this.RcpId, true)));

            // new packet type message20
            PacketFactory.Message20 message20 = new PacketFactory.Message20(this.RequestID)
            {
                Block = new byte[] { 0x00 }
            };

            // add command to message packet
            message20.Commands.Add(
                new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x12, 0x90, _rcpID[0], _rcpID[1] })     
            );

            // new packet type message response + insert message packet
            PacketFactory.MessageResponse _packet1 = new PacketFactory.MessageResponse(this.ResponseID, message20);
           
            var _response = this.WriteBufferAsync(_packet1).GetAwaiter().GetResult();
            if (_response?.NextPacket != null)
            {
                var commands = ((PacketFactory.Message20)_response.NextPacket).Commands.ToArray();

                for (int i = 0; i < commands.Length; i++)
                {
                    if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0xda, 0x00 }, 0))
                    {
                        assigned_ccu_no = commands[i].ToArray()[5];

                        // debug                                  
                        _logger?.LogDebug($" Assigned CCU# is { Convert.ToString(assigned_ccu_no) }");

                        break;
                    }
                }
            }

            // create packet - unknown
            PacketFactory.BasicMessage _packet2 = new PacketFactory.BasicMessage(this.RequestID)
            {
                Type = 0x02,
                Block = new byte[] { 0x00, 0x00 }
            };

            _ = this.WriteBufferAsync(_packet2);

            return assigned_ccu_no;
        }

        /// <summary>
        /// Get available CCU list
        /// </summary>
        /// <returns>
        /// reply_of_available_ccu_list
        /// </returns>
        public List<int> GetOnlineCcuList()
        {
            List<int> ccuList = new List<int>();

            // new packet type message20
            PacketFactory.Message20 message20 = new PacketFactory.Message20(this.RequestID)
            {
                Block = new byte[] { 0x12 }
            };

            // add command to message packet
            message20.Commands.Add(
                new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x12, (byte)PacketFactory.SRCID.RCP,
                        BitConverter.GetBytes(
                           Utils.ByteUtils.ReverseBytes(
                               Utils.SppUtils.RcpIdToRaw(this.RcpId, true)))[0],
                        BitConverter.GetBytes(
                           Utils.ByteUtils.ReverseBytes(
                               Utils.SppUtils.RcpIdToRaw(this.RcpId, true)))[1],
                })
            );

            // add command to message packet
            message20.Commands.Add(
                new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x58, 0xff })
            );

            // send message and parse response
            var _response = this.WriteBufferAsync(message20).GetAwaiter().GetResult();
            if (_response?.NextPacket != null)
            {
                var commands = ((PacketFactory.Message20)_response.NextPacket).Commands.ToArray();

                for (int i = 0; i < commands.Length; i++)
                {
                    if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x58, 0xff }, 0))
                    {

                        var buffer = commands[i].ToArray();
                        int size = Utils.ByteUtils.ReverseBytes(BitConverter.ToUInt16(buffer, 4));

                        var pos = 6;
                        for (var j = 0; j < size; j++)
                        {
                            ccuList.Add((int)buffer[pos + 1]);
                            pos += 5;
                        }
                    }
                }
            }

            return ccuList;
        }


        /// <summary>
        /// request_rcp_assignment_status_change
        /// </summary>
        /// <param name="ccuId">CCU No</param>
        /// <returns>
        /// Assigned CCU No or -1 if failed
        /// </returns>
        public int Assign(byte ccuId)
        {
            int result = -1;

            // new packet type message20
            PacketFactory.Message20 message20 = new PacketFactory.Message20(this.RequestID)
            {
                Block = new byte[] { 0x51 }
            };

            // add command to message packet
            message20.Commands.Add(
                new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x12, (byte)PacketFactory.SRCID.RCP,
                    BitConverter.GetBytes(
                       Utils.ByteUtils.ReverseBytes(
                           Utils.SppUtils.RcpIdToRaw(this.RcpId, true)))[0],
                    BitConverter.GetBytes(
                       Utils.ByteUtils.ReverseBytes(
                           Utils.SppUtils.RcpIdToRaw(this.RcpId, true)))[1],
                    0x00, 0x01 
                })
            );

            message20.Commands.Add(
                new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x12, 0xff,
                    BitConverter.GetBytes(
                       Utils.ByteUtils.ReverseBytes(
                           Utils.SppUtils.RcpIdToRaw(this.RcpId, false)))[0],
                    BitConverter.GetBytes(
                       Utils.ByteUtils.ReverseBytes(
                           Utils.SppUtils.RcpIdToRaw(this.RcpId, false)))[1]
                })
            );

            message20.Commands.Add(
                new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x58, ccuId, 0x00, 0x00 })
            );

            var _response = this.WriteBufferAsync(message20).GetAwaiter().GetResult();
            // 0f 02 a8 d9 0e 1f c1 2e 20 80 01 90 90 0a 00 01 b0 33 d9 fe 02 90 00 70 d9 fe 12 90 00 71 d9 fe da 00 58 08 02
            if (_response?.NextPacket != null)
            {

                var commands = ((PacketFactory.Message20)_response.NextPacket).Commands.ToArray();
                for (int i = 0; i < commands.Length; i++)
                {
                    // assigned ccu no (d9 fe da 00 58 00 02)
                    if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0xda }, 0))
                    {
                        result = commands[i].ToArray()[5];
                        continue;
                    }
                }
            }

            return result;
        }

        public async Task<int> AssingAsync(byte ccuId)
        {
            int x = await Task.Run(() => this.Assign(ccuId));
            return x;
        }


        public async Task<IPAddress> GetCcuIpAddressAsync(byte ccuId)
        {
            IPAddress x = await Task.Run(() => GetCcuIpAddress(ccuId));
            return x;
        }


        public IPAddress GetCcuIpAddress(byte ccuId)
        {
            IPAddress ip = default;
            try
            {
                // new packet type message20
                PacketFactory.Message20 message20 = new PacketFactory.Message20(this.RequestID)
                {
                    Block = new byte[] { 0x01 }               
                };

                // add command to message packet
                message20.Commands.Add(
                    new PacketFactory.Message20.SPpCommandPair(new byte[] { 0xd9, 0xfe, 0x58, ccuId })
                );

                // new packet type message response + insert message packet
                PacketFactory.MessageResponse _packet = new PacketFactory.MessageResponse(this.ResponseID, message20);

                // send request
                /*
                this.WriteBuffer(_packet,
                    delegate (PacketFactory.BasicPacket Packet)
                    {
                        if (Packet.NextPacket != null)
                        {
                            var commands = ((PacketFactory.Message20)Packet.NextPacket).Commands.ToArray();
                            for (int i = 0; i < commands.Length; i++)
                            {
                                // cuu exists or ccu no exists
                                if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x11}, 0) || Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x14 }, 0))
                                {
                                    // parse ccu no
                                    var ccu_no = BitConverter.ToUInt16(commands[i].ToArray(), 5);
                                    if (Utils.SppUtils.ParseRcpID(ccu_no).id == ccuId)
                                    {
                                        // parse ccu ip address
                                        byte[] _addressBytes = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                                        Buffer.BlockCopy(commands[i].ToArray(), 6, _addressBytes, 0, 4);
                                        ip = new IPAddress(_addressBytes);

                                        // debug                                  
                                        Console.WriteLine($" CCU[{ Convert.ToString(ccuId) }] ip address is { ip.ToString() }");
                                    }
                                    break;
                                }
                            }                         
                        }
                    },
                    out ManualResetEvent _event);

                _event.WaitOne(TimeSpan.FromMilliseconds(1000));
                */

                PacketFactory.BasicPacket _response = this.WriteBufferAsync(message20).GetAwaiter().GetResult();
                if (_response?.NextPacket != null)
                {
                    var commands = ((PacketFactory.Message20)_response.NextPacket).Commands.ToArray();
                    for (int i = 0; i < commands.Length; i++)
                    {
                        // cuu exists or ccu no exists
                        if (Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x11 }, 0) || Utils.ByteUtils.HasPrefix(commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x14 }, 0))
                        {
                            // parse ccu no
                            var ccu_no = BitConverter.ToUInt16(commands[i].ToArray(), 5);
                            if (Utils.SppUtils.ParseRcpID(ccu_no).id == ccuId)
                            {
                                // parse ccu ip address
                                byte[] _addressBytes = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                                Buffer.BlockCopy(commands[i].ToArray(), 6, _addressBytes, 0, 4);
                                ip = new IPAddress(_addressBytes);

                                // debug                                  
                                _logger?.LogDebug($" CCU[{ Convert.ToString(ccuId) }] ip address is { ip.ToString() }");
                            }
                            break;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][GetCcuIpAddress] {e.Message}");
            }

            return ip;
        }

        public async Task<bool> SetParaAsync()
        {
            var x = await Task.Run(() => SetPara());
            return x;
        }

        public bool SetPara()
        {
            bool _para = default;
            try
            {

                // Message30
                // 30 05 00 12 90 01 91 03 01 - off
                PacketFactory.Message30 _packet = new PacketFactory.Message30(this.RequestID)
                {
                    CcuNo = this.CcuClient.CcuId,
                    RcpNo = this.RcpId,
                    RcpActiveFlag = true,
                   
                };

                _packet.Commands.Add(
                    new PacketFactory.Message30.SPpCommandPair(0x03, 0x02));


                var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();

                // get command execute return
                if (_response?.NextPacket != null)
                {
                    var _value = ((PacketFactory.Message30)_response.NextPacket).Commands.Get((byte)0x03);
                    _para = (_value != null) ? ((_value.PARAM0 == 0x82) ? true : false) : false;
                }

            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][PARA] { e.Message }");
            }

            return _para;
        }

        public void DoChangePermision(PacketFactory.Message30.PermissionControl permisions)
        {
            this.OnChangePermisionControl?.Invoke(this, new PermissionControlEventArgs()
            {
                Permisions = permisions,
            });
        }

        public async void DoChangeAssigment(byte CcuNo)
        {
            this.OnChangeAssigment?.Invoke(this, new RcpAssigmentEventArgs()
            {
                CcuNo = CcuNo
            });

            if (this._ccuClient.CcuId != CcuNo)
            {
                if (this._ccuClient.Connected)
                {
                    this._ccuClient.Disconnect();
                }

                // try connect to CCU
                await this.ConnectToCcuAsync(CcuNo);

            }

        }

 
    }
}
