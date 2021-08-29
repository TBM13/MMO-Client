using MMO_Client.Client.Attributes;
using MMO_Client.Client.Net.Mines;
using System;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class RoomSceneObject : SceneObject, IDisposable
    {
        public Coord Coord { get; private set; }
        public virtual int Direction { get; protected set; }
        public virtual bool Blocks { get; private set; }

        public bool IsGrabbable
        {
            get => (bool)Attributes.GetValue("grabbable", false);
        }

        public Tile Tile
        {
            get => Room.CurrentRoom.TilesMatrix[Coord.X][Coord.Y];
        }

        public RoomSceneObject(CustomAttributeList attributes) : base(attributes) { }

        public override void BuildFromMobject(Mobject mobj)
        {
            base.BuildFromMobject(mobj);

            Direction = mobj.Integers["direction"];
            Blocks = mobj.Booleans["blockingHint"];

            InitializeAsset();
            SetCoord(new Coord(mobj.IntegerArrays["coord"]));
        }

        public virtual void SetCoord(Coord newCoord) => 
            Coord = newCoord;

        protected virtual void InitializeAsset() { }

        public virtual void Dispose() { }
    }
}
