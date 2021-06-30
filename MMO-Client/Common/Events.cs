using MMO_Client.Client.Net.Mines.Event;

namespace MMO_Client.Common
{
    internal class Events
    {
        internal delegate void Event();
        internal delegate void String2Event(string arg1, string arg2);
        internal delegate void String1Event(string arg1);
        internal delegate void Mines1Event(MinesEvent arg1);
    }
}
