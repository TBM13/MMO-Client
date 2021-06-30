using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO_Client.Client.Net.Mines
{
    class Message
    {
        public const int HEADER_TYPE = 3;

        public bool NeedsPayload { get => payload == -1; }

        private int payload = -1;

        public Message() { }


    }
}
