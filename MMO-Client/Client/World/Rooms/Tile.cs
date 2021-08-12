using MMO_Client.Screens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms
{
    internal class Tile
    {
        public const double Width = 71 * GameScreen.SizeMultiplier;
        public const double Height = 23 * GameScreen.SizeMultiplier;

        // Why do we need to decrease 42 and 21? I don't know
        public const double OffsetX = 42;
        public const double OffsetY = 21;

        public Coord Coord { get; init; }
        public bool Blocked { get; init; }
        public Point PositionInCanvas { get; private set; }

        private Rectangle rectangle;

        public Tile(Coord coord, bool blocked)
        {
            Coord = coord;
            Blocked = blocked;

            CreateTile();
        }

        /// <summary>
        /// Creates and adds the tile to the room.
        /// </summary>
        private void CreateTile()
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

            rectangle.Stroke = Brushes.Black;
            rectangle.Fill = Blocked ? Brushes.Red : null;
            rectangle.Opacity = 0.5;

            Room.CurrentRoom.Canvas.Children.Add(rectangle);
        }
    }
}
