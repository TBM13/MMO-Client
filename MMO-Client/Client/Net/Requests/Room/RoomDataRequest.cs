using MMO_Client.Client.Net.Mines;

namespace MMO_Client.Client.Net.Requests.Room
{
    internal class RoomDataRequest : MamboRequest
    {
        public RoomDataRequest() : base("RoomDataRequest") { }

        public override Mobject Build() => 
            new Mobject();
    }
}
