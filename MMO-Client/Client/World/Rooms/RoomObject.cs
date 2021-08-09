using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MMO_Client.Client.Assets;
using MMO_Client.Screens;

namespace MMO_Client.Client.World.Rooms
{
    class RoomObject
    {
        public string ID { get; init; }
        public string Name { get; init; }

        public Coord Coord { get; init; }
        public Size Size { get; init; }
        public int Direction { get; init; }
        public bool BlockingHint { get; init; }

        private ImageAsset imageAsset;
        private dynamic properties;

        public RoomObject(string id, string name, Coord coord, Size size, int direction, bool blockingHint)
        {
            ID = id;
            Name = name;
            Coord = coord;
            Size = size;
            Direction = direction;
            BlockingHint = blockingHint;

            LoadAsset();
            LoadProperties();
        }

        /// <summary>
        /// Loads and adds the asset to the room.
        /// </summary>
        private void LoadAsset()
        {
            imageAsset = AssetsManager.Instance.GetOrCreateImageAsset(Name);

            double xPos = Coord.X * Tile.Width;
            Canvas.SetLeft(imageAsset.Image, xPos);

            double yPos = (Coord.Y * Tile.Height) - 6; // Why do we need to decrease 6? I don't know
            Canvas.SetTop(imageAsset.Image, yPos);

            // Cancel the 3D perspective of the image
            imageAsset.Image.RenderTransform = new SkewTransform(40, 0);
            Room.CurrentRoom.Canvas.Children.Add(imageAsset.Image);
        }

        /// <summary>
        /// Loads and applies the asset properties.
        /// </summary>
        private void LoadProperties()
        {
            DrawAsset();
            properties = AssetsManager.GetAssetProperties(Name);

            double xCorrection;
            double yCorrection;

            if (properties == null)
            {
                Logger.Warn($"Object doesn't have any properties. Position in room may be inaccurate!", false, Name);

                // Try an alternative method to position the object
                // It's very inaccurate, but better than nothing
                yCorrection = -(imageAsset.Image.Height - (Tile.Height / 2));
                xCorrection = -(imageAsset.Image.Width - (Tile.Width / 1.5)) + yCorrection;

                xCorrection /= Size.Width;
                yCorrection /= Size.Height;
            }
            else
            {
                xCorrection = ((double)properties.bounds[0] * GameScreen.SizeMultiplier) + ((double)properties.bounds[1] * 1.07); // Where did the 1.07 come from? I don't know
                yCorrection = ((double)properties.bounds[1] * GameScreen.SizeMultiplier);

                // TODO: Improve correction and figure out how to interact with Size.Width and Size.Height
            }

            Canvas.SetLeft(imageAsset.Image, Canvas.GetLeft(imageAsset.Image) + xCorrection);
            Canvas.SetTop(imageAsset.Image,  Canvas.GetTop(imageAsset.Image)  + yCorrection);
        }

        private void DrawAsset()
        {
            Dictionary<string, int> animations = null;

            if (properties?.animations != null)
            {
                animations = new();
                foreach(var obj in properties.animations)
                    animations.Add((string)obj.Name, (int)obj.Value);
            }

            imageAsset.LoadAllFrames(animations);
            imageAsset.DrawFrame(1);

            imageAsset.Image.Width *= GameScreen.SizeMultiplier;
            imageAsset.Image.Height *= GameScreen.SizeMultiplier;
        }
    }
}
