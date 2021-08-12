using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.World.Rooms;

namespace MMO_Client.Client.Net.Requests.Room
{
    internal class ChangeRoomActionRequest : MamboRequest
    {
        private readonly RoomLink link;

        public ChangeRoomActionRequest(RoomLink link) : base("ChangeRoomActionRequest") => 
            this.link = link;

        public override Mobject Build() => 
            link.ToMobject();
    }
}
