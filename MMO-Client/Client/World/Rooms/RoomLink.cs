using MMO_Client.Client.Net.Mines;

namespace MMO_Client.Client.World.Rooms
{
    internal class RoomLink : IMobjectable, IMobjectBuildable
    {
        public Coord Coord { get; private set; }
        public Coord WorldCoord { get; private set; }
        public int? RoomId { get; set; }
        public string Owner { get; private set; }

        public RoomLink() { }

        public RoomLink(Coord coord, Coord worldCoord)
        {
            Coord = coord ?? new Coord();
            WorldCoord = worldCoord;
        }

        public RoomLink(Coord coord, int roomId)
        {
            Coord = coord ?? new Coord();
            RoomId = roomId;
        }

        public RoomLink(Coord coord, string owner)
        {
            Coord = coord ?? new Coord();
            Owner = owner;
        }

        public Mobject ToMobject()
        {
            Mobject mobj = new();
            mobj.IntegerArrays["coord"] = Coord.ToArray();

            if (RoomId != null)
                mobj.Integers["roomId"] = (int)RoomId;
            if (WorldCoord != null)
                mobj.IntegerArrays["worldCoord"] = WorldCoord.ToArray();
            if (Owner != null)
                mobj.Strings["ownerUsername"] = Owner;

            return mobj;
        }

        public void BuildFromMobject(Mobject mobj)
        {
            RoomId = mobj.Integers["roomId"];
            Coord = new Coord(mobj.IntegerArrays["coord"]);

            if (mobj.IntegerArrays.ContainsKey("worldCoord"))
                WorldCoord = new Coord(mobj.IntegerArrays["worldCoord"]);
        }
    }
}
