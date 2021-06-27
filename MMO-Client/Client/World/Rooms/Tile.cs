using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms
{
    class Tile
    {
        public const double Width = 71 * 1.3;
        public const double Height = 23 * 1.5;
        public const int MarginX = 0;
        public const int MarginY = 200;

        public Rectangle Rectangle { get; private set; }
        public Coord Coord;
        public bool Blocked { get; init; }

        public Tile(Coord coord, bool blocked)
        {
            Coord = coord;
            Blocked = blocked;

            CreateTile();
        }

        private void CreateTile()
        {
            Rectangle = new()
            {
                Width = Width,
                Height = Height,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Canvas.SetLeft(Rectangle, Coord.X * Width + MarginX);
            Canvas.SetTop(Rectangle, Coord.Y * Height + MarginY);

            Rectangle.Stroke = Brushes.Black;
            Rectangle.Fill = Blocked ? Brushes.Red : null;
            Rectangle.Opacity = 0.5;

            Room.CurrentRoom.Canvas.Children.Add(Rectangle);
        }
    }
}
