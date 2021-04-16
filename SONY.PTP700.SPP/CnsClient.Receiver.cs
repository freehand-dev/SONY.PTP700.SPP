using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SONY.PTP700.SPP.Utils;
using Microsoft.Extensions.Logging;

namespace SONY.PTP700.SPP
{


    public partial class CnsClient
    {

        public delegate void ResponseCallbackDelegate(PacketFactory.BasicPacket Packet);
        private class MessageCallback
        {
            public ManualResetEvent Event { get; set; }
            public ResponseCallbackDelegate Callback { get; set; }
        }


        private sealed class CnsReceiver
        {
            internal const int READ_BUFFER_SIZE = 1024;

            internal readonly object _lockObject = new object();

            private readonly ILogger _logger;

            internal delegate void PacketReceivedDelegate(PacketFactory.BasicPacket packet);
            internal delegate void DataReceivedDelegate(byte[] data, long length);

            internal DataReceivedDelegate OnDataReceived;
            internal PacketReceivedDelegate OnUnknownPacket;
            internal PacketReceivedDelegate OnHandShake;
            internal PacketReceivedDelegate OnCloseSession;
            internal PacketReceivedDelegate OnNotifyPacket;
            internal PacketReceivedDelegate OnErrorPacket;
            internal PacketReceivedDelegate OnMessagePacket;
            internal PacketReceivedDelegate OnMessageResponsePacket;
            internal PacketReceivedDelegate OnHeartBeatPacket;

            internal Action OnLeaveThread;

            private ManualResetEvent LeaveEvent = new ManualResetEvent(false);

            internal CancellationToken _token;

            public Dictionary<ushort, MessageCallback> _callback = new Dictionary<ushort, MessageCallback>();

            internal CnsReceiver(NetworkStream stream, CancellationToken token, ILogger logger)
            {
                _logger = logger;
                _token = token;
                _stream = stream;
                _thread = new Thread(Run);
                _thread.IsBackground = true;
            }

            public void Start()
            {
                _thread.Start();
            }

            public void Wait(TimeSpan timeout)
            {
                LeaveEvent.WaitOne(timeout);
            }

            private void Run()
            {
                _logger?.LogDebug($"[CnsReceiver][Thread] Enter");
                try
                {
                    while (!_token.IsCancellationRequested)
                    {
                        try
                        {
                            if (!_stream.DataAvailable)
                            {
                                    Thread.Sleep(1);
                                    continue;
                            }

                            lock (_lockObject)
                            {

                                using (MemoryStream memStream = new MemoryStream())
                                {

                                    while (_stream.DataAvailable)
                                    {
                                        byte[] data = new byte[READ_BUFFER_SIZE];
                                        int bytes = _stream.Read(data, 0, data.Length);
                                        if ( bytes > 0)
                                        {
                                            memStream.Append(data, bytes);
                                        }
                                        else
                                        {
                                            this.DoLeaveThread();
                                        }
                                    }
                                   
                                    this.DoDataProcessor(memStream.ToArray(), memStream.Length);

                                }
                            }
                        }
                        catch (IOException e)
                        {
                            _logger?.LogError(e, $"{e.Message}");
#if DEBUG
                            throw;
#endif
                        }
                        catch (Exception e)
                        {
                            _logger?.LogError(e, $"{e.Message}");
#if DEBUG
                            throw;
#endif
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"{e.Message}");
#if DEBUG
                    throw;
#endif
                }
                finally
                {
                    this.DoLeaveThread();
                }
            }

            private void DoLeaveThread()
            {
                _stream.Close();

                // 
                OnLeaveThread?.Invoke();

                // leave event
                LeaveEvent.Set();

                _logger?.LogDebug($"[CnsReceiver][Thread] Leave");
            }

            private void DoDataProcessor(byte[] data, long bytesRead)
            {
                if (bytesRead <= 0)
                {
                    return;
                }

                if (this.OnDataReceived != null)
                {
                    this.OnDataReceived(data, bytesRead);
                }

                PacketFactory.BasicPacket _basicPacket = PacketFactory.BasicPacket.InitPacket(data);
                do
                {
                    switch (_basicPacket.PacketType)
                    {
                        case PacketFactory.PacketType.HandShake :
                            {
                                if (this.OnHandShake != null)
                                {
                                    OnHandShake(_basicPacket);
                                }
                            }
                            break;
                        case PacketFactory.PacketType.Close:
                            {
                                if (this.OnCloseSession != null)
                                {
                                    OnCloseSession(_basicPacket);
                                }
                            }
                            break;
                        case PacketFactory.PacketType.Notify:
                            {
                                if (this.OnNotifyPacket != null)
                                {
                                    OnNotifyPacket(_basicPacket);
                                }
                            }
                            break;
                        case PacketFactory.PacketType.HeartBeat:
                            {
                                if (this.OnHeartBeatPacket != null)
                                {
                                    OnHeartBeatPacket(_basicPacket);
                                }
                            }
                            break;
                        case PacketFactory.PacketType.Error:
                            {
                                if (this.OnErrorPacket != null)
                                {
                                    OnErrorPacket(_basicPacket);
                                }
                            }
                            break;
                        case PacketFactory.PacketType.Message when _basicPacket.Header == PacketFactory.PacketHeader.Message:
                            if (this.OnMessagePacket != null)
                            {
                                OnMessagePacket(_basicPacket);
                            }
                            break;
                        case PacketFactory.PacketType.Message when _basicPacket.Header == PacketFactory.PacketHeader.MessageResponse:
                            if (this.OnMessageResponsePacket != null)
                            {

                                OnMessageResponsePacket(_basicPacket);

                                ushort _responseID = ((PacketFactory.MessageResponse)_basicPacket).ID;
                                if (_callback.TryGetValue(_responseID, out MessageCallback _messageCallback))
                                {
                                    ManualResetEvent _event = _messageCallback.Event;
                                    ResponseCallbackDelegate _responseCallback = _messageCallback.Callback;

                                    // callback
                                    try
                                    {
                                        _responseCallback?.Invoke(_basicPacket);
                                    } 
                                    catch (Exception e)
                                    {
                                        _logger?.LogError(e, $"{ e.Message }");
                                        throw;
                                    }
                                    

                                    //
                                    _event?.Set();
 
                                    _callback.Remove(_responseID);
                                }

                            }
                            break;
                        default:
                            {
                                if (this.OnUnknownPacket != null)
                                {
                                    OnUnknownPacket(_basicPacket);
                                }
                            }
                            break;
                    }

                    _basicPacket = _basicPacket?.NextPacket;
                } while (_basicPacket != null);

            }

            private NetworkStream _stream;
            private Thread _thread;
        }
    }

}
