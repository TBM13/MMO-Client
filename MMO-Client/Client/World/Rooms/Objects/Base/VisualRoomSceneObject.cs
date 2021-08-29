using MMO_Client.Client.Attributes;
using System.Windows;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class VisualRoomSceneObject : RoomSceneObject
    {
        public VisualRoomSceneObject(CustomAttributeList attributes) : base(attributes) { }

        protected void DrawAsset(FrameworkElement visualElement)
        {
            Room.CurrentRoom.Canvas.Children.Add(visualElement);
        }

        public override void SetCoord(Coord newCoord)
        {
            base.SetCoord(newCoord);
            UpdatePosition();
        }

        protected virtual void UpdatePosition()
        {
            Logger.Warn("UpdatePosition() not implemented!");
        }
    }
}
