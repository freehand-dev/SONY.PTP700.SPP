using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.Events
{


    public class CameraPowerStateEventArgs : EventArgs
    {
        [Flags]
        public enum CameraPowerStates
        {
            CameraPowerOff,
            CameraPowerOn,
            DataOff,
            DataOk,
            DataSense,
            ToneDetect,
            ToneNone,
            CableConnect,
            CableOpen,
        }

        public byte State { get; set; }
    }
}
