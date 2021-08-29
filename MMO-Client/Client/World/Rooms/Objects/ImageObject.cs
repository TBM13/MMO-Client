using MMO_Client.Client.Assets;
using MMO_Client.Client.Attributes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class ImageObject : VisualRoomSceneObject
    {
        public ImageAsset ImageAsset { get; private set; }

        public double? XCorrection { get; private set; } = null;
        public double? YCorrection { get; private set; } = null;

        public double ScaleX
        {
            get => scaleTransform.ScaleX;
            set => scaleTransform.ScaleX = value;
        }

        public double ScaleY
        {
            get => scaleTransform.ScaleY;
            set => scaleTransform.ScaleY = value;
        }

        private Dictionary<string, dynamic> properties;
        private ScaleTransform scaleTransform;

        public ImageObject(CustomAttributeList attributes) : base(attributes) { }

        protected override void InitializeAsset()
        {
            ImageAsset = AssetsManager.Instance.GetOrCreateImageAsset(Name);
            DrawAsset(ImageAsset.Image);

            scaleTransform = new(1, 1);
            ImageAsset.Image.RenderTransform = scaleTransform;

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

            ImageAsset.LoadAllFrames(animations);
            ImageAsset.DrawFrame(1);
        }

        protected override void UpdatePosition()
        {
            Tile tile = Tile;
            
            if (XCorrection == null)
            {
                if (properties == null)
                {
                    tile = Room.CurrentRoom.TilesMatrix[Coord.X + (int)Size.Width - 1][Coord.Y + (int)Size.Height - 1];

                    // Try an alternative method to position the object
                    // It's very inaccurate, but better than nothing
                    XCorrection = -(ImageAsset.Image.Width - (Room.CurrentRoom.TilesProperties.Width / 2));
                    YCorrection = -(ImageAsset.Image.Height - (Room.CurrentRoom.TilesProperties.Height / 2));

                    XCorrection /= Size.Width;
                    YCorrection /= Size.Height;
                }
                else
                {
                    XCorrection = properties["bounds"][0];
                    YCorrection = properties["bounds"][1];

                    // TODO: Improve correction and figure out how to interact with Size.Width and Size.Height
                }
            }

            double xPos = tile.PositionInCanvas.X;
            double yPos = tile.PositionInCanvas.Y;

            Canvas.SetLeft(ImageAsset.Image, xPos + (double)XCorrection);
            Canvas.SetTop(ImageAsset.Image, yPos + (double)YCorrection);
        }

        public override void Dispose()
        {
            base.Dispose();
            ImageAsset.StopAnimation();
        }
    }
}
