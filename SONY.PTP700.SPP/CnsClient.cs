using System;
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
    public enum CNSMode : byte
    {
        LEGACY = 0x00,
        BRIDGE = 0x01,
        MCS = 0x02
    };

    public class DeviceInfo
    {
        public UInt32 SerialNumber { get; set; }
        public PacketFactory.DeviceModel Model { get; set; }
        public PacketFactory.SRCID Type { get; set; }
    }


    public partial class CnsClient
    {
        internal readonly string Name = "CNS";
        internal readonly string Description = "Sony Camera Network System";

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        internal CancellationTokenSource cancelTokenSource;

        internal readonly object _lockObject = new object();

        // Consumers register to receive data.
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;
        public event EventHandler<EventArgs> OnCloseSession;
        public event EventHandler<PacketReceivedEventArgs> OnError;
        public event EventHandler<PacketReceivedEventArgs> OnHandShake;
        public event EventHandler<PacketReceivedEventArgs> OnMessage;
        public event EventHandler<PacketReceivedEventArgs> OnMessageResponse;

        private PacketFactory.Command.CommandHandlerList _commandHandlerList = new PacketFactory.Command.CommandHandlerList();

        public PacketFactory.Command.CommandHandlerList CommandHandlerList { 
            get
            {
                return _commandHandlerList;
            }
        }


        private ushort _requestID = 0x6468;
        private ushort _responseID;

        private TcpClient _client;
        private CnsReceiver _receiver;
        private CnsSender _sender;

        public bool IsHandshake { get 
            {
                return (this.RemoteInfo?.SerialNumber > 0);
            } 
        }


        /// <summary>
        /// The information for the remote device (CCU/MSU) from Handshake packet
        /// </summary>
        public DeviceInfo RemoteInfo { get; internal set; }

        public bool Connected
        {
            get
            {
                bool result = this.IsHandshake;
                if (result)
                    result = (_client != null) ? this._client.Connected == this.IsHandshake : false;

                return result;
            }
        }

        public int Port { get; set; } = 7700;
        public IPAddress Host { get; set; } = IPAddress.Parse("127.0.0.1");
        public CNSMode Mode { get; set; } = CNSMode.BRIDGE;
        public virtual UInt32 SerialNumber { get; set; } = 0;
        public ushort RequestID
        {
            get
            {
                try
                {
                    return this._requestID;
                }
                finally
                {
                    this._requestID += 1;
                }
            }
            set
            {
                this._requestID = value;
            }
        }

        public ushort ResponseID
        {
            get
            {
                this._responseID += 1;
                return this._responseID;
            }
            set
            {
                this._responseID = value;
            }
        }

        public bool HeartBeat {
            get {
                return _sender?.HeartBeat ?? false;
            }
            set {
                _sender.HeartBeat = value;
            }
        } 

        public CnsClient(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger("CnsClient");
            this._loggerFactory = loggerFactory;
        }

        internal void EventOnDataReceived(byte[] data, long length)
        {
            var handler = OnDataReceived;
            if (handler != null)
            {
                OnDataReceived(this, new DataReceivedEventArgs()
                {
                    Data = data,
                    BytesRead = length
                });
            }

            _logger?.LogDebug($"[ <--- ] { this.Host.ToString() } - { ByteUtils.ToHexString(data) }");
        }

        internal void EventOnNotifyPacket(PacketFactory.BasicPacket packet)
        {
            switch (packet.Header)
            {
                case PacketFactory.PacketHeader.Notify:
                    PacketFactory.Notify _packet = new PacketFactory.Notify();
                    _packet.Header = PacketFactory.PacketHeader.NotifyACK;
                    _sender.WriteBuffer(_packet);
                    break;
                case PacketFactory.PacketHeader.NotifyACK:
                    break;
                default:
                    break;
            }
        }

        internal void EventOnHeartBeatPacket(PacketFactory.BasicPacket packet)
        {
            switch (packet.Header)
            {
                case PacketFactory.PacketHeader.HeartBeat: // heartbeat
                    _sender.WriteBuffer(new PacketFactory.HeartBeat()
                    {
                        Header = PacketFactory.PacketHeader.HeartBeatACK
                    });
                    break;
                case PacketFactory.PacketHeader.HeartBeatACK: // heartbeat acknowledge
                    break;
                default:
                    break;
            }
        }
        internal void EventOnErrorPacket(PacketFactory.BasicPacket packet)
        {
            this.BasicResponse();
            this.OnError?.Invoke(this, new PacketReceivedEventArgs()
                {
                    Packet = packet,
                });
        }

        internal void EventOnCloseSession(PacketFactory.BasicPacket packet)
        {
            switch (packet.Header)
            {
                case PacketFactory.PacketHeader.Close: // close

                    // send CloseACK packet
                    _sender.WriteBuffer(new PacketFactory.CloseSession()
                    {
                        Header = PacketFactory.PacketHeader.CloseACK
                    });

                    // disconnect
                    this.Disconnect(false);

                    // OnCloseSession Event
                    this.OnCloseSession?.Invoke(this, new EventArgs());
                    break;
                case PacketFactory.PacketHeader.CloseACK: // close acknowledge
                    break;
                default:
                    break;
            }
        }

        internal void EventOnMessagePacket(PacketFactory.BasicPacket packet)
        {
            //temp
            this.ResponseID = ((PacketFactory.BasicMessage)packet).ID;

            this.OnMessage?.Invoke(this, new PacketReceivedEventArgs()
                {
                    Packet = packet,
                });
        }

        internal void EventOnMessageResponsePacket(PacketFactory.BasicPacket packet)
        {
            this.OnMessageResponse?.Invoke(this, new PacketReceivedEventArgs()
            {
                Packet = packet,
            });
        }

        internal void EventSenderOnSocketException(Object sender, SocketErrorEventArgs args)
        {
            // debug
            _logger?.LogDebug($"[ EventSenderOnSocketException ] { args.SocketErrorCode.ToString() } - { args.Message }");  
        }

        public virtual void Connect()
        {
            // if connected then disconnect;    
            this.Disconnect();

            // TCP socket connect
            this._client = new TcpClient();
            this._client.Connect(this.Host, this.Port);
            try
            {
                // Info
                _logger?.LogInformation("Socket connected to {0}", this._client.Client.RemoteEndPoint.ToString());

                // create CancellationTokenSource
                cancelTokenSource = new CancellationTokenSource();

                // CnsReceiver
                _receiver = new CnsReceiver(_client.GetStream(), cancelTokenSource.Token, _loggerFactory.CreateLogger("CnsClientReceiver"));
                _receiver.OnDataReceived += EventOnDataReceived;
                _receiver.OnNotifyPacket += EventOnNotifyPacket;
                _receiver.OnHeartBeatPacket += EventOnHeartBeatPacket;
                _receiver.OnErrorPacket += EventOnErrorPacket;
                _receiver.OnCloseSession += EventOnCloseSession;
                _receiver.OnMessagePacket += EventOnMessagePacket;
                _receiver.OnMessageResponsePacket += EventOnMessageResponsePacket;



                // CnsSender
                _sender = new CnsSender(_client.GetStream(), cancelTokenSource.Token, _loggerFactory.CreateLogger("CnsClientSender"));
                _sender.OnSocketException += EventSenderOnSocketException;

                // HandShake
                this.HandShake();
                
                // Enabled Hearbeats
                this.HeartBeat = true;

                _sender.Start();
                _receiver.Start();
                 
            }
            catch
            {
                this.Disconnect(false);
                throw;
            }

        }

        public virtual void Disconnect(bool CloseGracefully = true)
        {
            if (this._client != null && this._client.Connected)
            { 
                if (this.IsHandshake && CloseGracefully)
                {
                    // Send close session packet 0x0400
                    this.WriteBuffer(new PacketFactory.CloseSession());
                }

                // cancel
                cancelTokenSource?.Cancel();

                _sender?.Wait(
                    TimeSpan.FromMilliseconds(1000));

                _receiver?.Wait(
                    TimeSpan.FromMilliseconds(1000));

                this._client.Close();
                this.RemoteInfo = null;
                _logger?.LogInformation("Disconnected from server");
            }
        }

        public void HandShake()
        {

            if (!this._client.Connected)
            {
                throw new SystemException($"[HandShake] TCP Client is not connected");
            }

            NetworkStream stream = this._client.GetStream();

            // send first packet
            {
                PacketFactory.HandShake _packet = PacketFactory.HandShake.InitHandShake(PacketFactory.PacketHeader.HandShake, this.Mode, this._requestID, this.SerialNumber);
                byte[] _packetBuffer = _packet.ToBytes();
                stream.Write(_packetBuffer, 0, _packetBuffer.Length);
                stream.Flush();
            }

            byte[] receivedBytes = new byte[1024];
            int byte_count = stream.Read(receivedBytes, 0, receivedBytes.Length);
            if (byte_count == 0)
            {
                throw new SystemException($"[HandShake] host interrupted connection.");
            }

            if (receivedBytes[0] != (byte)PacketFactory.PacketHeader.HandShakeResponse)
            {
                throw new SystemException($"[HandShake]Response header incorect. [response][data] { Utils.ByteUtils.ToHexString(receivedBytes) }");
            }

            int _size = receivedBytes[1];
            if (_size != 0x12)
            {
                throw new SystemException($"[HandShake]Response size incorect. [response][data] { Utils.ByteUtils.ToHexString(receivedBytes) }");
            }
            _size += 2; /* header & size */

            if (byte_count != _size)
            {
                throw new SystemException($"[HandShake]Response data to long. [response][data] { Utils.ByteUtils.ToHexString(receivedBytes) }");
            }

            Array.Resize(ref receivedBytes, byte_count);
            PacketFactory.HandShake _responsePacket = new PacketFactory.HandShake(receivedBytes);
            this.ResponseID = _responsePacket.ID;

            // send next handshake packet
            {
                PacketFactory.HandShake _packet = PacketFactory.HandShake.InitHandShake(PacketFactory.PacketHeader.HandShakeACK, this.Mode, this._requestID, this.SerialNumber);
                byte[] _packetBuffer = _packet.ToBytes();
                stream.Write(_packetBuffer, 0, _packetBuffer.Length);
                stream.Flush();
            }


            this.RemoteInfo = new DeviceInfo()
            {
                SerialNumber = _responsePacket.SerialNumber,
                Model = _responsePacket.Model,
                Type = _responsePacket.Type
            };

            var handler = this.OnHandShake;
            if (handler != null) this.OnHandShake(this, new PacketReceivedEventArgs()
            {
                Packet = _responsePacket
            });

        }

        internal void WriteBuffer(PacketFactory.BasicPacket _packet)
        {
            _sender.WriteBuffer(_packet);

            // debug
            _logger?.LogDebug($"[ ---> ] { this.Host.ToString() } - { ByteUtils.ToHexString(_packet.ToArray()) }");
        }

        public PacketFactory.BasicPacket WriteBuffer(PacketFactory.BasicPacket _packet, int timeOut = 1000)
        {
            PacketFactory.BasicPacket _response = default;
            lock (_lockObject)
            {
                ManualResetEvent _resetEvent = default;
                if (_packet is PacketFactory.BasicMessage)
                {
                    _resetEvent = this.SubscribeResponse(((PacketFactory.BasicMessage)_packet).ID, delegate (PacketFactory.BasicPacket Packet)
                    {
                        _response = Packet;
                    });
                }
                else
                if (_packet is PacketFactory.MessageResponse)
                {
                    var request = _packet.NextPacket;
                    if (request != null && (request is PacketFactory.BasicMessage))
                    {
                        _resetEvent = this.SubscribeResponse(((PacketFactory.BasicMessage)request).ID, delegate (PacketFactory.BasicPacket Packet)
                        {
                            _response = Packet;
                        });
                    }
                }
                this.WriteBuffer(_packet);
                _resetEvent?.WaitOne(timeOut);
            }
            return _response;
        }

        public async Task<PacketFactory.BasicPacket> WriteBufferAsync(PacketFactory.BasicPacket _packet)
        {
            PacketFactory.BasicPacket x = await Task.Run(() => this.WriteBuffer(_packet, 1000));
            return x;
        }

        public void BasicResponse()
        {
            this.WriteBuffer(
                new PacketFactory.MessageResponse(this.ResponseID)
            );
        }

        public void BasicResponse(PacketFactory.BasicPacket _packet)
        {
            this.WriteBuffer(
                new PacketFactory.MessageResponse(this.ResponseID, _packet)
            );
        }

        internal ManualResetEvent SubscribeResponse(int _id, ResponseCallbackDelegate _callback)
        {
            ManualResetEvent _event = new ManualResetEvent(false);
            _receiver._callback.Add((ushort)(_id + 1), new MessageCallback()
            {
                Event = _event,
                Callback = _callback
            });

            return _event;
        }

    }
}
