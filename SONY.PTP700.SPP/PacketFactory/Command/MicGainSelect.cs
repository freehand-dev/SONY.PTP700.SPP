using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory.Command
{
    /*
     * 
     * bit7-6: Inc/Dec control
     *   [00] Set a value directory or status query
     *   [01] Increment the parameter specified by the PARAM0 address
     *   [10] Decrement the parameter specified by the PARAM0 address
     *   [11] N/A
     * bit5-0: Value of the parameter
     * 
     */



    public class MicGainSelect
    {
        public enum CmdGPEnum : byte
        {
            Relative = 0x20,
            Absolute = 0x21,
        }
        public enum MicGainValue : byte
        {
            Inc = 0x80,
            Dec = 0x40,
            QueryStatus = 0x00,
            _60dB = 0x1C,
            _50dB = 0x1D,
            _40dB = 0x1E,
            _30dB = 0x1F,
            _20dB = 0x20,
        }

        public enum MicGainChannel : byte
        {
            Ch01 = 0x08,
            Ch02 = 0x09,
        }

    }


    public class mic1_gain_select : Message50.SPpCommandPair
    {
        public MicGainSelect.MicGainValue Value
        {
            get
            {
                return (MicGainSelect.MicGainValue)this.PARAM1;
            }
            set
            {
                this.PARAM1 = (byte)value;
            }
        }

        public MicGainSelect.MicGainChannel Channel
        {
            get
            {
                return (MicGainSelect.MicGainChannel)this.PARAM0;
            }
            set
            {
                this.PARAM0 = (byte)value;
            }
        }

        public MicGainSelect.CmdGPEnum CmdGP
        {
            get
            {
                return (MicGainSelect.CmdGPEnum)this.CMD_GP;
            }
            set
            {
                this.CMD_GP = (byte)value;
            }
        }

        public mic1_gain_select(MicGainSelect.CmdGPEnum cmdGP)
            : base((byte)cmdGP, (byte)MicGainSelect.MicGainChannel.Ch01)
        {

        }

        public mic1_gain_select(byte[] data, int offset = 0)
            : base(data, offset)
        {

        }

    }

    public class mic2_gain_select : Message50.SPpCommandPair
    {
        static public string _Name = "MIC2:GAIN:SELECT";
        static public string _Description = "::MENU::MAINTENANCE::CAMERA::MICROPHONEGAIN::CH02";

        public MicGainSelect.MicGainValue Value
        {
            get
            {
                return (MicGainSelect.MicGainValue)this.PARAM1;
            }
            set
            {
                this.PARAM1 = (byte)value;
            }
        }

        public MicGainSelect.MicGainChannel Channel
        {
            get
            {
                return (MicGainSelect.MicGainChannel)this.PARAM0;
            }
            set
            {
                this.PARAM0 = (byte)value;
            }
        }

        public MicGainSelect.CmdGPEnum CmdGP
        {
            get
            {
                return (MicGainSelect.CmdGPEnum)this.CMD_GP;
            }
            set
            {
                this.CMD_GP = (byte)value;
            }
        }


        public mic2_gain_select(MicGainSelect.CmdGPEnum cmdGP)
            : base((byte)cmdGP, (byte)MicGainSelect.MicGainChannel.Ch02)
        {

        }

        public mic2_gain_select(byte[] data, int offset = 0)
            : base(data, offset)
        {

        }

    }


}