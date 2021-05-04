using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SONY.PTP700.SPP.Utils;

namespace SONY.PTP700.SPP.PacketFactory.Command
{
    public class CcuFunction01 : Message50.SPpCommandPair
    {

        static public byte _PARAM_0 = 0x12;

        public bool Mono
        {
            get
            {
                return Utils.BitUtils.IsBitSet(this.PARAM1, 2);
            }
            set
            {
                var param_1 = this.PARAM1;
                Utils.BitUtils.BitSet(ref param_1, 2, value);
                this.PARAM1 = param_1;
            }
        }

        public CcuFunction01(PacketFactory.SppCommnadGroup cmdGP)
            : base((byte)cmdGP, CcuFunction00._PARAM_0)
        {

        }

        public CcuFunction01(byte[] data, int offset = 0)
            : base(data, offset)
        {

        }

    }
}
