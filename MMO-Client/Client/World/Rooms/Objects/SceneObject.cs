using MMO_Client.Client.Attributes;
using MMO_Client.Client.Net.Mines;
using System.Windows;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class SceneObject
    {
        protected string Name { get; private set; }
        protected int Id { get; private set; }
        protected Size Size { get; private set; }
        protected CustomAttributeList Attributes { get; init; }

        public SceneObject(CustomAttributeList attributes) =>
            Attributes = attributes;

        public virtual void BuildFromMobject(Mobject mobj)
        {
            Id = int.Parse(mobj.Strings["id"]);
            Name = mobj.Strings["name"];
            Size = new(mobj.IntegerArrays["size"][0], mobj.IntegerArrays["size"][1]);
        }
    }
}
