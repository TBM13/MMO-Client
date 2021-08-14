using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MMO_Client.Client.Assets;
using MMO_Client.Client.Attributes;
using MMO_Client.Client.Config;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.World.Rooms.Objects;

namespace MMO_Client.Client.World.Rooms
{
    internal class Room : IMobjectBuildable, IDisposable
    {
        public static Room CurrentRoom { get; private set; }

        public string Name { get; private set; }
        public int ID { get; private set; }
        public Size Size { get; private set; }
        public Coord Coord { get; private set; }

        public Canvas Canvas { get; init; } = new();

        public Dictionary<int, Dictionary<int, Tile>> TilesMatrix { get; private set; }
        public record TilesPropertiesRecord(double Width, double Height, double Delta, double Angle, double OffsetY);
        public TilesPropertiesRecord TilesProperties { get; private set; }

        private readonly CustomAttributeList attributes = new();
        private readonly List<RoomSceneObject> sceneObjects = new();

        public Room() => 
            CurrentRoom = this;

        public void BuildFromMobject(Mobject mObj)
        {
            Name = mObj.Strings["name"];
            ID = mObj.Integers["id"];
            Logger.Debug($"Creating Room {ID} ({Name})");
            //Clipboard.SetText(mObj.ToString());

            // Apply Margin
            Canvas.SetLeft(Canvas, -30);
            Canvas.SetTop(Canvas, 84);

            Size = new(mObj.IntegerArrays["size"][0], mObj.IntegerArrays["size"][1]);
            Coord = new(mObj.IntegerArrays["coord"]);

            attributes.BuildFromMobjectArray(mObj.MobjectArrays["customAttributes"]);

            LoadTilesProperties();
            CreateBackground();

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

        private void LoadTilesProperties()
        {
            Dictionary<string, dynamic> dic = Settings.Instance.Dictionary["_tiles_"]["normal"];
            if (Settings.Instance.Dictionary["_tiles_"].ContainsKey(ID.ToString()))
                dic = Settings.Instance.Dictionary["_tiles_"][ID.ToString()];

            TilesProperties = new(dic["width"], dic["height"], dic["delta"], dic["angle"], dic["layerY"]);
        }

        private void CreateTiles(Mobject[] tiles)
        {
            Canvas.SetTop(Canvas, Canvas.GetTop(Canvas) + TilesProperties.OffsetY);

            TilesMatrix = new();
            for (int i = 0; i < Size.Width; i++)
                TilesMatrix[i] = new Dictionary<int, Tile>();

            foreach (Mobject obj in tiles)
            {
                Tile tile = new();
                tile.BuildFromMobject(obj);
                TilesMatrix[tile.Coord.X][tile.Coord.Y] = tile;
            }
        }

        private void CreateObjects(Mobject[] objects)
        {
            foreach (Mobject mobj in objects)
            {
                CustomAttributeList attributes = new();
                attributes.BuildFromMobjectArray(mobj.MobjectArrays["customAttributes"]);

                RoomSceneObject roomObj;
                if (mobj.Mobjects.ContainsKey("link"))
                    roomObj = new Portal(attributes);
                else
                    roomObj = new ImageObject(attributes);

                sceneObjects.Add(roomObj);
                roomObj.BuildFromMobject(mobj);
            }
        }

        private void CreateBackground()
        {
            if (!attributes.HasValue("background"))
            {
                Logger.Warn($"Room '{Name}' doesn't have any background");
                return;
            }

            ImageAsset background;
            background = AssetsManager.Instance.GetOrCreateImageAsset($"backgrounds.{attributes.GetValue("background")}");
            background.LoadAllFrames();
            background.DrawFrame(1);

            Canvas.Children.Add(background.Image);
            Canvas.SetZIndex(background.Image, -1);
            Canvas.SetLeft(background.Image, -background.Image.Width / 8.5);
            Canvas.SetTop(background.Image, (-background.Image.Height / 2) + TilesProperties.OffsetY - 88 / 4);
        }

        public void Dispose()
        {
            foreach (RoomSceneObject obj in sceneObjects)
                obj.Dispose();
        }
    }
}