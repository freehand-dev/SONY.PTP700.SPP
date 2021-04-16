using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.Events
{
    public class PermissionControlEventArgs : EventArgs
    {
        public PacketFactory.Message30.PermissionControl Permisions { get; set; }
    }
}
