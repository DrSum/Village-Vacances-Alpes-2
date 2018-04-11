using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliRencontre
{
    class Message
    {
        DateTime timeSent;
        Profile sender;
        string textSent;

        public Message(DateTime t, Profile p, string text)
        {
            timeSent = t;
            sender = p;
            textSent = text;
        }

        public string TextSent { get => textSent; set => textSent = value; }
        public DateTime TimeSent { get => timeSent; set => timeSent = value; }

        public override string ToString()
        {
            return timeSent + " [" + sender.FirstName + "] : " + TextSent;
        }
    }
}
