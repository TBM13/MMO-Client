using MMO_Client.Client.Attributes;
using System.Windows;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class VisualRoomSceneObject : RoomSceneObject
    {
        public VisualRoomSceneObject(CustomAttributeList attributes) : base(attributes) { }

        protected override void InitializeAsset()
        {
            Logger.Warn("InitializeAsset() not implemented!");
        }

        protected void DrawAsset(FrameworkElement visualElement)
        {
            Room.CurrentRoom.Canvas.Children.Add(visualElement);
        }

        protected override void SetCoord(Coord newCoord)
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
