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

        public string Name { get; init; }
        public int ID { get; init; }
        public Size Size { get; private set; }

        public Canvas Canvas { get; init; }
        public Dictionary<int, Dictionary<int, Tile>> TilesMatrix { get; private set; }

        public Room(dynamic roomData)
        {
            CurrentRoom = this;
            Canvas = new()
            {
                RenderTransform = new SkewTransform(-40, 0)
            };

            Name = roomData.name;
            ID = roomData.roomId;

            Logger.Debug($"Creating Room {ID}", Name);

            Size = new((double)roomData.size[0], (double)roomData.size[1]);
            CreateTiles(roomData.tiles);

            CreateObjects(roomData.objects);
        }

        private void CreateTiles(dynamic tiles)
        {
            TilesMatrix = new();

            Canvas.Margin = new Thickness(GameScreen.GameWidth / 3, 0, -GameScreen.GameWidth / 3, 0);

            for (int i = 0; i < Size.Width; i++)
                TilesMatrix[i] = new Dictionary<int, Tile>();

            foreach (var obj in tiles)
            {
                string[] coords = obj.Name.Split(";");
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);

                Tile tile = new(new Coord(x, y), (bool)obj.Value);
                TilesMatrix[x][y] = tile;
            }
        }

        private void CreateObjects(dynamic objects)
        {
            foreach (var obj in objects)
            {
                Coord coord = new((int)obj.coord[0], (int)obj.coord[1]);
                Size size = new((int)obj.size[0], (int)obj.size[1]);
                new RoomObject((string)obj.id, (string)obj.name, coord, size,(int)obj.direction, (bool)obj.blockingHint);
            }
        }
    }
}
