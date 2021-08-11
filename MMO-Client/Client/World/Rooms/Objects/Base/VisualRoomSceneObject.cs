using MMO_Client.Client.Attributes;
using System.Windows;
using System.Windows.Media;

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
            // Cancel the 3D perspective of the visual element
            visualElement.RenderTransform = new SkewTransform(40, 0);
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
