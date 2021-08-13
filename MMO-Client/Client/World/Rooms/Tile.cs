using MMO_Client.Client.Config;
using MMO_Client.Client.Net.Mines;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms
{
    internal class Tile : IMobjectBuildable
    {
        public Coord Coord { get; private set; }
        public bool BlockingHint { get; private set; }
        public Point PositionInCanvas { get; private set; }

        private Rectangle rectangle;

        public void BuildFromMobject(Mobject mobj)
        {
            BlockingHint = mobj.Booleans["blockingHint"];
            Coord = new Coord(mobj.IntegerArrays["coord"]);

            CreateRectangle();
        }

        private void CreateRectangle()
        {
            double width = Room.CurrentRoom.TilesProperties.Width;
            double height = Room.CurrentRoom.TilesProperties.Height;
            double delta = Room.CurrentRoom.TilesProperties.Delta;
            double angle = Room.CurrentRoom.TilesProperties.Angle;

            rectangle = new()
            {
                Width = width,
                Height = height,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                RenderTransform = new SkewTransform(-angle, 0) // 3D Perspective
            };

            double x = Coord.X * width;

            // Width2 is calculated in MMO's Parallelogram.draw() 
            double width2 = (height * Math.Tan(Math.PI * angle / 180)) + width + 1;
            width2 = Math.Round(width2, 1);

            x += (width2 - width - 1) * (Room.CurrentRoom.Size.Height - 1 - Coord.Y); // SkewTransform correction
            x += width2 / 2;
            x += delta;

            double y = (Coord.Y * height) + ((height + 1) / 2);
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            // We want the PositionInCanvas point to be the tile center
            PositionInCanvas = new Point(x + (width / 3), y + (height / 2));

            if (Settings.Instance.Dictionary["debug"]["showTiles"])
            {
                rectangle.Stroke = Brushes.Black;
                rectangle.Fill = BlockingHint ? Brushes.Red : null;
                rectangle.Opacity = 0.5;
            }

            Room.CurrentRoom.Canvas.Children.Add(rectangle);
        }
    }
}
