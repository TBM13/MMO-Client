using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MMO_Client.Client.Assets;

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

        public RoomObject(string id, string name, Coord coord, Size size, int direction, bool blockingHint)
        {
            ID = id;
            Name = name;
            Coord = coord;
            Size = size;
            Direction = direction;
            BlockingHint = blockingHint;

            ImageAsset imageAsset = AssetsManager.CreateImageAsset(Name);

            // The objects looked too small in comparison to MG, so i had to multiply their size
            // This value (1.3) is most likely not perfect, as i figured it with the naked eye
            imageAsset.Image.Width *= 1.3;
            imageAsset.Image.Height *= 1.3;

            double xPos = Coord.X * Tile.Width + Tile.MarginX;
            double widthCorrection = (imageAsset.Image.Width - Tile.Width / 1.5) / Size.Width;
            double xCenterCorrection = Tile.Width / (imageAsset.Image.Width / 3);
            double xImageCenterCorrection = imageAsset.Image.Width / 20;

            Canvas.SetLeft(imageAsset.Image, xPos - widthCorrection);

            double yPos = Coord.Y * Tile.Height + Tile.MarginY;
            double heightCorrection = (imageAsset.Image.Height - Tile.Height) / Size.Height;
            double yCenterCorrection = Tile.Height / (imageAsset.Image.Height / 3);
            Canvas.SetTop(imageAsset.Image, yPos - heightCorrection);

            // Cancel the 3D perspective of the image
            imageAsset.Image.RenderTransform = new SkewTransform(40, 0);
            Room.CurrentRoom.Canvas.Children.Add(imageAsset.Image);
        }
    }
}
