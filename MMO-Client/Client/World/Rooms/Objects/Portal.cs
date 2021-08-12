using MMO_Client.Client.Attributes;
using MMO_Client.Client.Config;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Screens;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            double width = (dynamic)attributes.GetValue("hitWidth", Tile.Width);
            double height = (dynamic)attributes.GetValue("hitHeight", Tile.Height);

            offsetX = (double)(dynamic)attributes.GetValue("hitX", -width / 2);
            offsetY = (double)(dynamic)attributes.GetValue("hitY", -height / 2);

            Shape = new Rectangle()
            {
                Width = width,
                Height = height,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Shape.MouseDown += MouseDown;
            Shape.MouseEnter += MouseEnter;
            Shape.MouseLeave += MouseLeave;

            if (Settings.Instance.Dictionary["debug"]["showTiles"])
            {
                Canvas.SetZIndex(Shape, 1);
                Shape.Fill = Brushes.Blue;
                Shape.Opacity = 0.2;

                Shape.MouseEnter += (_, _) =>
                    Shape.Opacity = 0.4;

                Shape.MouseLeave += (_, _) =>
                    Shape.Opacity = 0.2;
            }
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

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            RoomManager.Instance.ChangeRoom(Link);
        }

        private void MouseEnter(object sender, MouseEventArgs e)
        {
            Logger.Info((string)Attributes.GetValue("label", "<Null>"));
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            
        }
    }
}
