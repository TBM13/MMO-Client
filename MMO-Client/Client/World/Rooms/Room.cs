﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MMO_Client.Client.Attributes;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.World.Rooms.Objects;
using MMO_Client.Screens;

namespace MMO_Client.Client.World.Rooms
{
    internal class Room : IMobjectBuildable
    {
        public static Room CurrentRoom { get; private set; }

        public string Name { get; private set; }
        public int ID { get; private set; }
        public Size Size { get; private set; }
        public Coord Coord { get; private set; }

        public Canvas Canvas { get; init; } = new();
        public Dictionary<int, Dictionary<int, Tile>> TilesMatrix { get; private set; }

        public Room() => 
            CurrentRoom = this;

        public void BuildFromMobject(Mobject mObj)
        {
            Name = mObj.Strings["name"];
            ID = mObj.Integers["id"];
            Logger.Debug($"Creating Room {ID} ({Name})");

            // Apply Margin
            Canvas.SetLeft(Canvas, -380);
            Canvas.SetTop(Canvas, 225);

            Size = new(mObj.IntegerArrays["size"][0], mObj.IntegerArrays["size"][1]);
            Coord = new(mObj.IntegerArrays["coord"]);

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
                    roomObj = new Portal(attributes, Coord);
                else
                    roomObj = new ImageObject(attributes);

                roomObj.BuildFromMobject(mobj);
            }
        }
    }
}
