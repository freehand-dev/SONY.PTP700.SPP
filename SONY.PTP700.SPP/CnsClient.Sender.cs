using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SONY.PTP700.SPP.Utils;
using SONY.PTP700.SPP.Events;
using Microsoft.Extensions.Logging;

namespace SONY.PTP700.SPP
{
    public partial class CnsClient
    {
        private sealed class CnsSender
        {

            public event EventHandler<SocketErrorEventArgs> OnSocketException;

            private ManualResetEvent LeaveEvent = new ManualResetEvent(false);

            internal readonly object _lockObject = new object();

            private readonly ILogger _logger;

            internal Action OnLeaveThread;

            internal CancellationToken _token;

            // enable HearBeat packet sender
            public bool HeartBeat { get; set; } = (false);

            // start send HearBeat packet after 1000 msec last activity
            internal Stopwatch _lastActivity = Stopwatch.StartNew();

            private NetworkStream _stream;
            private Thread _thread;

            internal CnsSender(NetworkStream stream, CancellationToken token, ILogger logger)
            {
                _logger = logger;
                _token = token;
                _stream = stream;
                _thread = new Thread(Run);
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
                // debug
                _logger?.LogDebug($"[CnsSender][Thread] Enter");
                try
                {
                    while (!_token.IsCancellationRequested)
                    {
                        if (HeartBeat && _lastActivity.ElapsedMilliseconds > 1000)
                            this.WriteBuffer(new PacketFactory.HeartBeat());
                        Thread.Sleep(10);  
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"[Exception] { e.Message }");
                    throw;
                }
                finally
                {
                    this.DoLeaveThread();
                }    
            }

            private void DoLeaveThread()
            {
                _stream?.Close();

                // 
                OnLeaveThread?.Invoke();

                // leave event
                LeaveEvent.Set();

                _logger?.LogDebug($"[CnsSender][Thread] Leave");
            }

            public void WriteBuffer(PacketFactory.BasicPacket packet)
            {
                try
                {
                    var _packetBuffer = packet.ToBytes();
                    lock (_lockObject)
                    {
                        using (var writer = new BinaryWriter(this._stream, Encoding.Default, true))
                        {
                            writer.Write(_packetBuffer, 0, _packetBuffer.Length);
                            writer.Flush();
                             _lastActivity.Restart();
                        }
                    }
                }
                catch (SocketException e)
                {
                    _logger?.LogError(e, $"[SocketException] { e.Message }");

                    if (e.SocketErrorCode == SocketError.WouldBlock ||
                          e.SocketErrorCode == SocketError.IOPending ||
                          e.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        _logger?.LogError(e, $"[SocketException1] { e.Message }");
                        Thread.Sleep(30);
                    }
                    else
                    if (e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        _logger?.LogError(e, $"[SocketException2] { e.Message }");
                        this.OnSocketException?.Invoke(this, new SocketErrorEventArgs()
                        {
                            Exception = e
                        });
                    }
                    else
                    {
                        _logger?.LogError(e, $"[SocketException3] { e.Message }");
                    }
                    throw;
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"[Exception] { e.Message }");
                    throw;
                }
            }
        }
    }
}
