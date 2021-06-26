using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms
{
    class Tile
    {
        public int X { get; init; }
        public int Y { get; init; }
        public bool Blocked { get; init; }

        private const int marginX = 0;
        private const int marginY = 200;

        public Tile(int x, int y, bool blocked)
        {
            X = x;
            Y = y;
            Blocked = blocked;

            CreateTile();
        }

        private void CreateTile()
        {
            Rectangle rectangle = new()
            {
                Width = 71 * 1.3,
                Height = 23 * 1.5,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            rectangle.Margin = new Thickness((X * rectangle.Width ) + marginX,
                                             (Y * rectangle.Height) + marginY, 0, 0);

            rectangle.Stroke = Brushes.Black;
            rectangle.Fill = Blocked ? Brushes.Red : null;
            rectangle.Opacity = 0.5;

            Room.CurrentRoom.TilesGrid.Children.Add(rectangle);
        }
    }
}
