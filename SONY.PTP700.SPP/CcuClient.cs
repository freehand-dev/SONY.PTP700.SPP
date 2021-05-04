using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SONY.PTP700.SPP.Utils;
using System.Threading.Tasks;
using SONY.PTP700.SPP.Events;
using Microsoft.Extensions.Logging;

namespace SONY.PTP700.SPP
{
    public class CcuClient: CnsClient
    {

        // trigger to change rcp assiment
        public event EventHandler<CameraPowerStateEventArgs> OnChangeCameraPowerState;


        private readonly ILogger _logger;

        public byte CcuId { get; set; } = 0;
        public byte RcpId { get; set; } = 0;

        public CcuClient(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            // ILoggerFactory
            this._logger = loggerFactory.CreateLogger("CcuClient");

            this.Mode = CNSMode.BRIDGE;

            this.OnHandShake += delegate (Object sender, PacketReceivedEventArgs args)
            {
                // this.ScanCameraStatus();
            };

            this.OnMessageResponse += delegate (Object sender, PacketReceivedEventArgs args)
            {
                PacketFactory.MessageResponse _packet = (PacketFactory.MessageResponse)args.Packet;
            };

            this.OnMessage += delegate (Object sender, PacketReceivedEventArgs args)
            {
                if (args.Packet is PacketFactory.Message01)
                {

                    _logger?.LogDebug("!!!!!!!!!!!!!!! 0x01");
                }
                else
                if (args.Packet is PacketFactory.Message10)
                {
                    PacketFactory.Message10 _packet = (PacketFactory.Message10)args.Packet;
                    // CAM_PW
                    if (_packet.Commands.Contains(new byte[] { 0x18, 0x20, 0x00, 0x00 }))
                    {
                        this.UNKNOWN_10001018200000();
                    }
                    else
                    {
                        this.BasicResponse();
                    }


                    // trigers ?????
                    var commands = ((PacketFactory.Message10)args.Packet).Commands.ToArray();
                    for (int i = 0; i < commands.Length; i++)
                    {
                        // CAM_PW
                        if (Utils.ByteUtils.IndexOf(commands[i].ToArray(), new byte[] { 0x18, 0x20, 0x00, 0x00 }, 0) >= 0)
                        {
                            if (_packet.SubType == PacketFactory.Message10.Message10SubType.Response)
                                this.DoChangeCameraPowerState(commands[i].ToArray()[0]);
                            continue;
                        }
                    }

                }               
                else
                if (args.Packet is PacketFactory.Message21)
                {

                    _logger?.LogDebug("!!!!!!!!!!!!!!! 0x21");

                    if (args.Packet.Payload[3] == 0x10)
                    {
                        this.BasicResponse(new PacketFactory.Message21(this.RequestID)
                        {
                            Block = new byte[] { 0x80, 0x00 }
                        });
                    }
                    else
                    {
                        this.BasicResponse();
                    }
                }
                else
                if (args.Packet is PacketFactory.Message50)
                {
                    this.BasicResponse();

                    PacketFactory.Message50 _packet = (PacketFactory.Message50)args.Packet;
                    // doCommand Event
                    foreach (PacketFactory.Message50.SPpCommandPair command in _packet.Commands)
                    {
                        if (this.CommandHandlerList.TryGetValue(command.Name, out Action<PacketFactory.Message50.SPpCommandPair> _action))
                        {
                            _action(command);
                        }

                        // debug
                        _logger?.LogDebug($"[MESSAGE50][COMMAND]: { command.Name }");
                    }
                }
                else
                if (args.Packet is PacketFactory.BasicMessage)
                {
                    this.BasicResponse();
                    PacketFactory.BasicMessage _packet = (PacketFactory.BasicMessage)args.Packet;
                }
            };

        }


        public override void Connect()
        {
            base.Connect();

            if (this.Mode == CNSMode.MCS)
            {
                // register 
                this.Register();
            }

            Thread.Sleep(400);


            this.ScanCameraStatus();


            if (this.Mode == CNSMode.MCS)
            {
                // send confirm 0x21 packet (subscribe to riceive messages from CCU)
                this.WriteBuffer(new PacketFactory.Message21(this.RequestID)
                {
                    Block = new byte[] { 0x10 }
                });
            }


            //this.HeartBeat = false;

            //this.ScanCameraStatus();

            //this.HeartBeat = true;
        }

        public void Disconnect()
        {
            base.Disconnect(true);
        }

        private void Register()
        {
            byte[] _rcpID = BitConverter.GetBytes(
               Utils.ByteUtils.ReverseBytes(
                   Utils.SppUtils.RcpIdToRaw(this.RcpId, true)));

            PacketFactory.BasicMessage _packet = new PacketFactory.BasicMessage(this.RequestID)
            {
                Type = 0x01,
                Block = new byte[] { 0x01, 0xd9, 0xfe, 0x12, 0x90, _rcpID[0], _rcpID[1], 0x58, this.CcuId}
            };

            // send request
            /*
            this.WriteBuffer(_packet,
                delegate (PacketFactory.BasicPacket Packet)
                {
                    if (Packet.NextPacket != null)
                    {
                        Console.WriteLine("RegisterRCP in CCU");

                    }


                    //tttt
                    this.BasicResponse();

                },
                out ManualResetEvent _event);

            // wait response
            _event.WaitOne(TimeSpan.FromMilliseconds(1000));
            */

            var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
            _logger?.LogDebug($"RegisterRCP in CCU: { ((_response != null) ? "true" : "false") }" );
        }

        private void ScanCameraStatus()
        {
            // get camera status 1
            /*
            this.WriteBuffer(
                new PacketFactory.BasicMessage(this.RequestID)
                {
                    Type = 0x10,
                    Block = new byte[] { 0x00, 0x40, 0x18, 0x20, 0x00, 0x00 },
                }
            );
            */
            // get camera status 2
            this.WriteBuffer(
                new PacketFactory.BasicMessage(this.RequestID)
                {
                    Type = 0x10,
                    Block = new byte[] { 0x00, 0x40, 0x18, 0x40, 0x00, 0x00 },
                }
            );

            // get camera status 3
            this.WriteBuffer(
                new PacketFactory.BasicMessage(this.RequestID)
                {
                    Type = 0x10,
                    Block = new byte[] { 0x00, 0x40, 0x18, 0xD3, 0x00, 0x00 },
                }
            );


            // get camera status 3
            this.WriteBuffer(
                new PacketFactory.BasicMessage(this.RequestID)
                {
                    Type = 0x10,
                    Block = new byte[] { 0x00, 0x40, 0x18, 0xD4, 0x00, 0x00 },
                }
            );

            // get camera status 4
            this.WriteBuffer(
                new PacketFactory.BasicMessage(this.RequestID)
                {
                    Type = 0x10,
                    Block = new byte[] { 0x00, 0x40, 0x18, 0x60, 0x00, 0x00 },
                }
            );

        }

        internal void UNKNOWN_10001018200000()
        {
            var _request = new PacketFactory.Message10(this.RequestID)
            {
                SubType = PacketFactory.Message10.Message10SubType.Response
            };

            _request.Commands.Add(
                new PacketFactory.Message10.SPpCommandPair(
                    0x91, 
                    new byte[] { 0x18, 0x20, 0x00, 0x00 }
                    )
                );

            // response
            PacketFactory.MessageResponse _packet = new PacketFactory.MessageResponse(this.ResponseID, _request);

            this.WriteBuffer(_packet);
        }

        public bool SetBars()
        {
            if (!this.Connected)
                throw new SystemException($"Connected failed. Execute a command, not possible. "); 

            bool _bars = default;
            try
            {
                // Message50
                PacketFactory.Message50 _packet = new PacketFactory.Message50(this.RequestID)
                {
                    SubType = PacketFactory.Message50.Message50SubType.Request,
                    CcuID = this.CcuId
                };

                // Add Command
                _packet.Commands.Add(new PacketFactory.Command.CcuFunction00(PacketFactory.SppCommnadGroup.CCU_SWITCH_REL)
                {
                    Bars = true,
                    Chroma = false,
                    CCUSkinGate = false
                }); ;


                var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
                if (_response.NextPacket != null)
                {
                    var _message50 = (_response.NextPacket as PacketFactory.Message50);
                    var _command = _message50?.Commands.Get(
                        PacketFactory.SppCommnadGroup.CCU_SWITCH_ABS,
                        PacketFactory.Command.CcuFunction00._PARAM_0
                    );

                    if (_command != null)
                    {
                        var _ccu_function_00 = new PacketFactory.Command.CcuFunction00(_command.ToBytes());
                        _bars = _ccu_function_00.Bars;
                    }
                }



            } 
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][BARS] { e.Message }");
            }

            return _bars;
        }

        public PacketFactory.Command.CcuFunction00 ccu_function_00()
        {
            PacketFactory.Command.CcuFunction00 _ccu_function_00 = default;
            try
            {
                // Message50
                PacketFactory.Message50 _packet = new PacketFactory.Message50(this.RequestID)
                {
                    SubType = PacketFactory.Message50.Message50SubType.Request,
                    CcuID = this.CcuId
                };

                // Add Command
                _packet.Commands.Add(new PacketFactory.Command.CcuFunction00(PacketFactory.SppCommnadGroup.CCU_SWITCH_REL)
                {
                    Bars = false,
                    Chroma = false,
                    CCUSkinGate = false
                }); ;

               
                var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
                if (_response.NextPacket != null)
                {
                    var _message50 = (_response.NextPacket as PacketFactory.Message50);
                    var _command = _message50?.Commands.Get(
                        PacketFactory.SppCommnadGroup.CCU_SWITCH_ABS,
                        PacketFactory.Command.CcuFunction00._PARAM_0
                    );

                    if (_command != null)
                    {
                        _ccu_function_00 = new PacketFactory.Command.CcuFunction00(_command.ToBytes());
                    }
                }


            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][ccu_function_00] { e.Message }");
            }

            return _ccu_function_00;
        }

        public (PacketFactory.Command.mic1_gain_select, PacketFactory.Command.mic2_gain_select) mic_gain_select(PacketFactory.Command.MicGainSelect.MicGainValue micGainValue)
        {
            PacketFactory.Command.mic1_gain_select _mic1_gain_select = default;
            PacketFactory.Command.mic2_gain_select _mic2_gain_select = default;
            try
            {
                // Message50
                PacketFactory.Message50 _packet = new PacketFactory.Message50(this.RequestID)
                {
                    SubType = PacketFactory.Message50.Message50SubType.Request,
                    CcuID = this.CcuId
                };

                // Add Command
                _packet.Commands.Add(new PacketFactory.Command.mic1_gain_select(PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS)
                {
                    Value = micGainValue
                });

                _packet.Commands.Add(new PacketFactory.Command.mic2_gain_select(PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS)
                {
                    Value = micGainValue
                });


                var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
                if (_response.NextPacket != null)
                {
                    var _message50 = (_response.NextPacket as PacketFactory.Message50);

                    {
                        var _command = _message50?.Commands.Get(
                            PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS,
                            (byte)PacketFactory.Command.MicGainSelect.MicGainChannel.Ch01
                        );

                        if (_command != null)
                        {
                            _mic1_gain_select = new PacketFactory.Command.mic1_gain_select(_command.ToBytes());
                        }
                    }

                    {
                        var _command = _message50?.Commands.Get(
                            PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS,
                            (byte)PacketFactory.Command.MicGainSelect.MicGainChannel.Ch02
                        );

                        if (_command != null)
                        {
                            _mic2_gain_select = new PacketFactory.Command.mic2_gain_select(_command.ToBytes());
                        }
                    }

                }

            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][mic_gain_select] { e.Message }");
            }

            return (_mic1_gain_select, _mic2_gain_select);
        }

        public PacketFactory.Command.mic1_gain_select mic1_gain_select(PacketFactory.Command.MicGainSelect.MicGainValue micGainValue)
        {
            PacketFactory.Command.mic1_gain_select _mic1_gain_select = default;
            try
            {
                // Message50
                PacketFactory.Message50 _packet = new PacketFactory.Message50(this.RequestID)
                {
                    SubType = PacketFactory.Message50.Message50SubType.Request,
                    CcuID = this.CcuId
                };

                // Add Command
                _packet.Commands.Add(new PacketFactory.Command.mic1_gain_select(PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS)
                {
                    Value = micGainValue
                }); ;


                var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
                if (_response.NextPacket != null)
                {
                    var _message50 = (_response.NextPacket as PacketFactory.Message50);
                    var _command = _message50?.Commands.Get(
                        PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS,
                        (byte)PacketFactory.Command.MicGainSelect.MicGainChannel.Ch01
                    );

                    if (_command != null)
                    {
                        _mic1_gain_select = new PacketFactory.Command.mic1_gain_select(_command.ToBytes());
                    }
                }

            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][mic1_gain_select] { e.Message }");
            }

            return _mic1_gain_select;
        }

        public PacketFactory.Command.mic2_gain_select mic2_gain_select(PacketFactory.Command.MicGainSelect.MicGainValue micGainValue)
        {
            PacketFactory.Command.mic2_gain_select _mic2_gain_select = default;
            try
            {
                // Message50
                PacketFactory.Message50 _packet = new PacketFactory.Message50(this.RequestID)
                {
                    SubType = PacketFactory.Message50.Message50SubType.Request,
                    CcuID = this.CcuId
                };

                // Add Command
                _packet.Commands.Add(new PacketFactory.Command.mic2_gain_select(PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS)
                {
                    Value = micGainValue
                }); ;


                var _response = this.WriteBufferAsync(_packet).GetAwaiter().GetResult();
                if (_response.NextPacket != null)
                {
                    var _message50 = (_response.NextPacket as PacketFactory.Message50);
                    var _command = _message50?.Commands.Get(
                        PacketFactory.SppCommnadGroup.CHU_SWITCH_ABS,
                        (byte)PacketFactory.Command.MicGainSelect.MicGainChannel.Ch01
                    );

                    if (_command != null)
                    {
                        _mic2_gain_select = new PacketFactory.Command.mic2_gain_select(_command.ToBytes());
                    }
                }

            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][mic2_gain_select] { e.Message }");
            }

            return _mic2_gain_select;
        }

        public bool Call(bool on, bool permanently = false)
        {
            bool _bars = default;
            try
            {
                //0e 0f f2 ff   40 01 81 18 90 05 00 18 40 05 00 02 90  ON
                PacketFactory.Message40 _message = new PacketFactory.Message40(this.RequestID)
                {
                    Block = new byte[] { 0x01, 0x81, 0x18, (byte)PacketFactory.SRCID.RCP, this.CcuId, 0x00, 0x18, (byte)PacketFactory.SRCID.HSCU, this.CcuId, 0x00, 0x02, (byte)PacketFactory.SRCID.RCP }
                };

                // send message
                var _response = this.WriteBufferAsync(_message).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"[ERROR][CALL] { e.Message }");
            }

            return _bars;
        }

        public async Task<bool> SetBarsAsync()
        {
            var x = await Task.Run(() => SetBars());
            return x;
        }

        public async Task<bool> GetBarsAsync()
        {
            var x = await Task.Run(() => this.ccu_function_00());
            return (x != null) ? x.Bars : false;
        }

        public async Task<bool> GetChromaAsync()
        {
            var x = await Task.Run(() => this.ccu_function_00());
            return (x != null) ? x.Chroma : false;
        }


        public bool SetActive()
        {
            PacketFactory.BasicMessage _packet = new PacketFactory.BasicMessage(this.RequestID)
            {
                Type = 0x50,
                Block = new byte[] { 0x18, 0x04, 0x00, 0x00, 0x18, 0x90, 0x00, 0x00, 0x00, 0x08, 0x0b, 0x90, 0x01, 0x81, 0x0b, 0x90, 0x02, 0x81 },
            };

            _ = this.WriteBufferAsync(_packet);

            return false;
        }

        public void DoChangeCameraPowerState(byte state)
        {
            this.OnChangeCameraPowerState?.Invoke(this, new CameraPowerStateEventArgs()
            {
                State = state,
            });
        }

    }
}
