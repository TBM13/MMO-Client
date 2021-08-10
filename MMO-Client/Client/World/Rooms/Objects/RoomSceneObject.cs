using MMO_Client.Client.Assets;
using MMO_Client.Client.Attributes;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Screens;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class RoomSceneObject : SceneObject
    {
        public Coord Coord { get; private set; }
        public int Direction { get; private set; }
        public bool Blocks { get; private set; }

        private ImageAsset imageAsset;
        private Dictionary<string, dynamic> properties;

        public bool IsGrabbable
        {
            get => (bool)Attributes.GetValue("grabbable");
        }

        public Tile Tile
        {
            get => Room.CurrentRoom.TilesMatrix[Coord.X][Coord.Y];
        }

        public RoomSceneObject(CustomAttributeList attributes) : base(attributes) { }

        public override void BuildFromMobject(Mobject mobj)
        {
            base.BuildFromMobject(mobj);

            Direction = mobj.Integers["direction"];
            Blocks = mobj.Booleans["blockingHint"];

            LoadAsset();
            SetCoord(new Coord(mobj.IntegerArrays["coord"]));
        }

        protected void SetCoord(Coord newCoord)
        {
            Coord = newCoord;
            UpdateAssetPosition();
        }

        private void LoadAsset()
        {
            imageAsset = AssetsManager.Instance.GetOrCreateImageAsset(Name);

            // Cancel the 3D perspective of the image
            imageAsset.Image.RenderTransform = new SkewTransform(40, 0);
            Room.CurrentRoom.Canvas.Children.Add(imageAsset.Image);

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

            if (properties?.ContainsKey("animations") != null)
            {
                animations = new();
                foreach (KeyValuePair<string, int> pair in properties["animations"])
                    animations.Add(pair.Key, pair.Value);
            }

            imageAsset.LoadAllFrames(animations);
            imageAsset.DrawFrame(1);

            imageAsset.Image.Width *= GameScreen.SizeMultiplier;
            imageAsset.Image.Height *= GameScreen.SizeMultiplier;
        }

        private void UpdateAssetPosition()
        {
            double xPos = Coord.X * Tile.Width;
            double yPos = (Coord.Y * Tile.Height) - 6; // Why do we need to decrease 6? I don't know
            double xCorrection;
            double yCorrection;

            if (properties == null)
            {
                // Try an alternative method to position the object
                // It's very inaccurate, but better than nothing
                yCorrection = -(imageAsset.Image.Height - (Tile.Height / 2));
                xCorrection = -(imageAsset.Image.Width - (Tile.Width / 1.5)) + yCorrection;

                xCorrection /= Size.Width;
                yCorrection /= Size.Height;
            }
            else
            {
                xCorrection = (properties["bounds"][0] * GameScreen.SizeMultiplier) + (properties["bounds"][1] * 1.07); // Where did the 1.07 come from? I don't know
                yCorrection = (properties["bounds"][1] * GameScreen.SizeMultiplier);

                // TODO: Improve correction and figure out how to interact with Size.Width and Size.Height
            }

            Canvas.SetLeft(imageAsset.Image, xPos + xCorrection);
            Canvas.SetTop(imageAsset.Image, yPos + yCorrection);
        }
    }
}
