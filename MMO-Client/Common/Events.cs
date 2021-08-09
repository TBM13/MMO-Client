using MMO_Client.Client.Net.Mines;

namespace MMO_Client
{
    internal class Events
    {
        public delegate void Event();

        public delegate void String1Event(string arg1);
        public delegate void String2Event(string arg1, string arg2);
        public delegate void String2BoolEvent(string arg1, string arg2, bool arg3);

        public delegate void Bool1Event(bool arg1);

        public delegate void Mines1Event(MinesEvent arg1);
    }
}
