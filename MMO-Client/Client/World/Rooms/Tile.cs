using MMO_Client.Screens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MMO_Client.Client.World.Rooms
{
    class Tile
    {
        public const double Width = 71 * GameScreen.SizeMultiplier;
        public const double Height = 23 * GameScreen.SizeMultiplier;

        public Coord Coord;
        public bool Blocked { get; init; }

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
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Canvas.SetLeft(rectangle, Coord.X * Width - 42);
            Canvas.SetTop(rectangle, Coord.Y * Height - 21);

            rectangle.Stroke = Brushes.Black;
            rectangle.Fill = Blocked ? Brushes.Red : null;
            rectangle.Opacity = 0.5;

            Room.CurrentRoom.Canvas.Children.Add(rectangle);
        }
    }
}
