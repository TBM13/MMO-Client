using MMO_Client.Client.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMO_Client.Client.World.Rooms.Objects
{
    internal class MovingImageObject : ImageObject
    {
        public MovingImageObject(CustomAttributeList attributes) : base(attributes) { }

        public bool Moving { get => path.Coord != null && !stopMoving; }
        public int Speed { get; protected set; } = 1;

        private Path.Path path;
        private bool stopMoving;

        private const int framerate = 1000 / 24;

        protected override void InitializeAsset()
        {
            base.InitializeAsset();

            if ((bool)Attributes.GetValue("flipped", false))
                ScaleX = -1;

            Speed = 100;
            Move(new Path.Path(new List<Coord>() { new Coord(5, 4), new Coord(5, 3), new Coord(5, 2), new Coord(5, 1) }));
        }

        public void Move(Path.Path path)
        {
            StopMoving();

            this.path = path;
            Move();
        }

        public void Move(Coord coord)
        {
            StopMoving();
            SetCoord(coord);
        }

        public void StopMoving() =>
            stopMoving = true;

        private async void Move()
        {
            stopMoving = false;

            await Task.Delay(1000);
            SetCoord(new Coord(5, 4));
            await Task.Delay(1000);

            Logger.Debug("Start");
            while (Moving)
            {
                //EvaluateSide(Coord, path.Coord);

                Tile destinationTile = Room.CurrentRoom.TilesMatrix[path.Coord.X][path.Coord.Y];
                double destinationX = destinationTile.PositionInCanvas.X + (double)XCorrection;
                double destinationY = destinationTile.PositionInCanvas.Y + (double)YCorrection;

                int xDifference = path.Coord.X - Coord.X;
                int yDifference = path.Coord.Y - Coord.Y;

                bool moveRight = xDifference > 0;
                if (xDifference == 0)
                    moveRight = yDifference < 0;

                bool moveDown = yDifference > 0;

                double speedModifier = Speed / 100;
                double xSpeedModifier = speedModifier * (moveRight ? 1 : -1);
                double ySpeedModifier = speedModifier * (moveDown ? 1 : -1);

                double destinationXAbs = Math.Abs(destinationX);
                double currentXAbs = Math.Abs(Canvas.GetLeft(ImageAsset.Image));
                double xIncreaser = (Math.Max(destinationXAbs, currentXAbs) - Math.Min(destinationXAbs, currentXAbs)) * xSpeedModifier;

                double destinationYAbs = Math.Abs(destinationY);
                double currentYAbs = Math.Abs(Canvas.GetTop(ImageAsset.Image));
                double yIncreaser = (Math.Max(destinationYAbs, currentYAbs) - Math.Min(destinationYAbs, currentYAbs)) * ySpeedModifier;

                while (Canvas.GetLeft(ImageAsset.Image) != destinationX || Canvas.GetTop(ImageAsset.Image) != destinationY)
                {
                    if (Canvas.GetLeft(ImageAsset.Image) != destinationX)
                        Canvas.SetLeft(ImageAsset.Image, Canvas.GetLeft(ImageAsset.Image) + xIncreaser);

                    if (Canvas.GetTop(ImageAsset.Image) != destinationY)
                        Canvas.SetTop(ImageAsset.Image, Canvas.GetTop(ImageAsset.Image) + yIncreaser);

                    await Task.Delay(framerate);
                }

                if (!Moving)
                    break;

                Logger.Debug($"Set coord {path.Coord.X};{path.Coord.Y}");
                SetCoord(path.Coord);
                path.Step();
            }
        }

        protected void EvaluateSide(Coord oldCoord, Coord newCoord)
        {
            int xDifference = newCoord.X - oldCoord.X;
            ScaleX = Math.Sign(xDifference) * Math.Abs(ScaleX);
        }

        public override int Direction
        {
            get
            {
                if (base.Direction == 0 && path.NextCoord != null)
                    base.Direction = World.Direction.Calculate(path.Coord, path.NextCoord);

                return base.Direction;
            }

            protected set => base.Direction = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            StopMoving();
        }
    }
}
