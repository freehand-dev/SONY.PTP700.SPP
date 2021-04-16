using System;
using System.Collections.Generic;
using System.Text;

namespace SONY.PTP700.SPP.PacketFactory.Command
{


    public class CommandHandlerList : Dictionary<string, Action<PacketFactory.Message50.SPpCommandPair>>
    {
        public CommandHandlerList()
            : base()
        {

        }

        public void EventSubscriber(string name, Action<PacketFactory.Message50.SPpCommandPair> action)
        {
            if (this.ContainsKey(name.ToUpper()))
            {
                this[name.ToUpper()] += action;
            }
            else
            {
                this.Add(name.ToUpper(), action);
            }
        }

        public void EventUnsubscriber(string name, Action<PacketFactory.Message50.SPpCommandPair> action)
        {
            if (this.ContainsKey(name.ToUpper()))
            {
                this[name.ToUpper()] -= action;
            }
        }


    }
}
