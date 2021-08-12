using MMO_Client.Client.Attributes;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Screens;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class Portal : ShapeObject
    {
        public RoomLink Link { get; private set; }
        public new int Direction
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private readonly Coord roomCoord;
        private readonly double offsetX, offsetY;

        public Portal(CustomAttributeList attributes, Coord roomCoord) : base(attributes)
        {
            this.roomCoord = roomCoord;

            double width = (dynamic)attributes.GetValue("hitWidth", Tile.Width / GameScreen.SizeMultiplier);
            double height = (dynamic)attributes.GetValue("hitHeight", Tile.Height / GameScreen.SizeMultiplier);
            width *= GameScreen.SizeMultiplier;
            height *= GameScreen.SizeMultiplier;

            if (attributes.HasValue("hitX"))
                offsetX = (dynamic)attributes.GetValue("hitX") * GameScreen.SizeMultiplier;
            else
                offsetX = -width / 2;

            if (attributes.HasValue("hitY"))
                offsetY = (dynamic)attributes.GetValue("hitY") * GameScreen.SizeMultiplier;
            else
                offsetY = -height / 2;

            Shape = new Rectangle()
            {
                Width = width,
                Height = height,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = Brushes.Blue,
                Opacity = 0.3
            };
        }

        public override void BuildFromMobject(Mobject mobj)
        {
            base.BuildFromMobject(mobj);
            Link = new();
            Link.BuildFromMobject(mobj.Mobjects["link"]);
        }

        protected override void UpdatePosition()
        {
            Point tilePos = Room.CurrentRoom.TilesMatrix[Coord.X][Coord.Y].PositionInCanvas;
            double xPos = tilePos.X + offsetX;
            double yPos = tilePos.Y + offsetY;

            Canvas.SetLeft(Shape, xPos);
            Canvas.SetTop(Shape, yPos);
        }
    }
}
