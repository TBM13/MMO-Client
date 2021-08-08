using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MMO_Client.Client.Net.Mines.Mobjects;
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

        public Room(Mobject mObj)
        {
            CurrentRoom = this;

            Name = mObj.Strings["name"];
            ID = mObj.Integers["id"];
            Logger.Debug($"Creating Room {ID}", Name);

            Canvas = new()
            {
                RenderTransform = new SkewTransform(-40, 0)
            };

            // Apply Margin
            Canvas.SetLeft(Canvas, -190);
            Canvas.SetTop(Canvas, 287);

            Size = new(mObj.IntegerArrays["size"][0], mObj.IntegerArrays["size"][1]);

            foreach (Mobject mobj in mObj.MobjectArrays["tiles"])
            {
                if (mobj.MobjectArrays.ContainsKey("coords"))
                {
                    CreateTiles(mobj.MobjectArrays["coords"]);
                    break;
                }
            }

            CreateObjects(mObj.MobjectArrays["sceneObjects"]);
        }

        private void CreateTiles(Mobject[] tiles)
        {
            TilesMatrix = new();

            Canvas.Margin = new Thickness(GameScreen.GameWidth / 3, 0, -GameScreen.GameWidth / 3, 0);

            for (int i = 0; i < Size.Width; i++)
                TilesMatrix[i] = new Dictionary<int, Tile>();

            foreach (Mobject obj in tiles)
            {
                int x = obj.IntegerArrays["coord"][0];
                int y = obj.IntegerArrays["coord"][1];

                Tile tile = new(new Coord(x, y), obj.Booleans["blockingHint"]);
                TilesMatrix[x][y] = tile;
            }
        }

        private void CreateObjects(Mobject[] objects)
        {
            foreach (Mobject obj in objects)
            {
                Coord coord = new(obj.IntegerArrays["coord"][0], obj.IntegerArrays["coord"][1]);
                Size size = new(obj.IntegerArrays["size"][0], obj.IntegerArrays["size"][1]);
                _ = new RoomObject(obj.Strings["id"], obj.Strings["name"], coord, size, obj.Integers["direction"], obj.Booleans["blockingHint"]);
            }
        }
    }
}
