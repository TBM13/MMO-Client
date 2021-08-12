using MMO_Client.Client.Config;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Screens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms
{
    internal class Tile : IMobjectBuildable
    {
        public const double Width = 71 * GameScreen.SizeMultiplier;
        public const double Height = 23 * GameScreen.SizeMultiplier;

        // Why do we need to decrease 42 and 21? I don't know
        public const double OffsetX = 42;
        public const double OffsetY = 21;

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
            rectangle = new()
            {
                Width = Width,
                Height = Height,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                RenderTransform = new SkewTransform(-40, 0) // 3D Perspective
            };

            double x = Coord.X * Width - OffsetX;
            x += 25 * (Room.CurrentRoom.Size.Height - Coord.Y); // SkewTransform correction

            double y = Coord.Y * Height - OffsetY;
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            // We want the PositionInCanvas point to be the tile center
            PositionInCanvas = new Point(x + (Width / 3), y + (Height / 2));

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
