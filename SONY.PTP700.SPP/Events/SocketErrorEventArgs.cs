using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SONY.PTP700.SPP.Events
{


    public class SocketErrorEventArgs : EventArgs
    {
        public SocketException Exception { get; set; }

        public int ErrorCode { get => this.Exception.ErrorCode; }

        public string Message { get => this.Exception.Message; }

        public SocketError SocketErrorCode { get => this.Exception.SocketErrorCode; }
    }
}
