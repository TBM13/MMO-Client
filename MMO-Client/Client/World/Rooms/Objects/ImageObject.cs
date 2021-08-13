using MMO_Client.Client.Assets;
using MMO_Client.Client.Attributes;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class ImageObject : VisualRoomSceneObject
    {
        private ImageAsset imageAsset;
        private Dictionary<string, dynamic> properties;

        public ImageObject(CustomAttributeList attributes) : base(attributes) { }

        protected override void InitializeAsset()
        {
            imageAsset = AssetsManager.Instance.GetOrCreateImageAsset(Name);
            DrawAsset(imageAsset.Image);

            LoadAssetProperties();
        }

        private void LoadAssetProperties()
        {
            properties = AssetsManager.GetAssetProperties(Name);
            if (properties == null)
                Logger.Warn($"Object doesn't have any properties. Position in room may be inaccurate!", false, Name);

            DrawAsset();
        }

        private void DrawAsset()
        {
            Dictionary<string, int> animations = null;

            if (properties != null && properties.ContainsKey("animations"))
            {
                animations = new();
                foreach (KeyValuePair<string, object> pair in properties["animations"])
                    animations.Add(pair.Key, (int)pair.Value);
            }

            imageAsset.LoadAllFrames(animations);
            imageAsset.DrawFrame(1);
        }

        protected override void UpdatePosition()
        {
            Tile tile = Tile;
            
            double xCorrection;
            double yCorrection;

            if (properties == null)
            {
                tile = Room.CurrentRoom.TilesMatrix[Coord.X + (int)Size.Width - 1][Coord.Y + (int)Size.Height - 1];

                // Try an alternative method to position the object
                // It's very inaccurate, but better than nothing
                xCorrection = -(imageAsset.Image.Width - (Room.CurrentRoom.TilesProperties.Width / 2));
                yCorrection = -(imageAsset.Image.Height - (Room.CurrentRoom.TilesProperties.Height / 2));

                xCorrection /= Size.Width;
                yCorrection /= Size.Height;
            }
            else
            {
                xCorrection = properties["bounds"][0];
                yCorrection = properties["bounds"][1];

                // TODO: Improve correction and figure out how to interact with Size.Width and Size.Height
            }

            double xPos = tile.PositionInCanvas.X;
            double yPos = tile.PositionInCanvas.Y;

            Canvas.SetLeft(imageAsset.Image, xPos + xCorrection);
            Canvas.SetTop(imageAsset.Image, yPos + yCorrection);
        }
    }
}
