using MMO_Client.Client.Attributes;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class ShapeObject : VisualRoomSceneObject
    {
        protected Shape Shape { get; set; }
        
        public ShapeObject(CustomAttributeList attributes) : base (attributes) { }

        protected override void InitializeAsset() => 
            DrawAsset(Shape);

        protected override void UpdatePosition()
        {
            double xPos = Coord.X * Tile.Width;
            double yPos = Coord.Y * Tile.Height;

            Canvas.SetLeft(Shape, xPos);
            Canvas.SetTop(Shape, yPos);
        }
    }
}
