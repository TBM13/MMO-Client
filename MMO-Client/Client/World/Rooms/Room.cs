using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MMO_Client.Common;
using MMO_Client.Screens;

namespace MMO_Client.Client.World.Rooms
{
    class Room
    {
        public static Room CurrentRoom;

        public Grid Grid { get; init; }
        public Grid TilesGrid { get; private set; }

        public string Name { get; init; }
        public int ID { get; init; }
        public Size Size { get; private set; }

        private Dictionary<int, Dictionary<int, Tile>> tilesMatrix;

        public Room(dynamic roomData)
        {
            CurrentRoom = this;
            Grid = new();

            Name = roomData.name;
            ID = roomData.roomId;

            Logger.Debug($"Creating Room {ID}", Name);

            Size = new((double)roomData.size[0], (double)roomData.size[1]);
            CreateTiles(roomData.tiles);
        }

        private void CreateTiles(dynamic tiles)
        {
            tilesMatrix = new();
            TilesGrid = new()
            {
                RenderTransform = new SkewTransform(-40, 0)
            };

            Grid.Children.Add(TilesGrid);
            TilesGrid.Margin = new Thickness(GameScreen.GameWidth / 3, 0, -GameScreen.GameWidth / 3, 0);

            for (int i = 0; i < Size.Width; i++)
                tilesMatrix[i] = new Dictionary<int, Tile>();

            foreach (var obj in tiles)
            {
                string[] coords = obj.Name.Split(";");
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);

                Tile tile = new(x, y, (bool)obj.Value);
                tilesMatrix[x][y] = tile;
            }
        }
    }
}
