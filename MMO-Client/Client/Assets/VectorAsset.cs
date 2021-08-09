using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MMO_Client.Client.Assets.Controls;

namespace MMO_Client.Client.Assets
{
    internal class VectorAsset : Asset
    {
        public CustomSvgViewbox Viewbox { get; init; }
        public DrawingGroup InitialDrawing { get; private set; }
        public DrawingGroup LastDrawnDrawing { get; private set; }

        public List<DrawingGroup> Frames { get; private set; }
        public int FPS { get; set; } = 24;
        public bool Loop { get; set; }
        public bool PlayingAnimation { get; private set; }

        public Rect MaxBounds { get => maxBounds; }

        private Rect maxBounds;

        public VectorAsset() => 
            Viewbox = new();

        public void Initialize(DrawingGroup drawing)
        {
            InitialDrawing = drawing;
            maxBounds = drawing.Bounds;

            Draw(drawing);
        }

        public void Initialize(DrawingGroup drawing, List<DrawingGroup> frames)
        {
            InitialDrawing = drawing;
            Frames = frames;

            maxBounds = drawing.Bounds;
            foreach (DrawingGroup d in frames)
            {
                if (d.Bounds.X > maxBounds.X)
                    maxBounds.X = d.Bounds.X;

                if (d.Bounds.Y > maxBounds.Y)
                    maxBounds.Y = d.Bounds.Y;
            }

            Draw(drawing);
        }

        public void Initialize(DrawingGroup drawing, List<DrawingGroup> frames, Rect maxBounds)
        {
            InitialDrawing = drawing;
            Frames = frames;

            this.maxBounds = maxBounds;

            Draw(drawing);
        }

        public void Draw(DrawingGroup drawing)
        {
            LastDrawnDrawing = drawing;
            Viewbox.AddDrawing(drawing, maxBounds);
        }

        public async void StartAnimation()
        {
            if (PlayingAnimation)
            {
                Logger.Warn("Animation is already playing", false, ID);
                return;
            }

            if (Frames == null)
            {
                Logger.Warn("No animation frames provided", false, ID);
                return;
            }

            PlayingAnimation = true;

            int delayTime = 1000 / FPS;

            int frame = 0;
            while (PlayingAnimation)
            {
                await Task.Delay(delayTime);
                frame++;
                if (frame == Frames.Count)
                {
                    if (Loop)
                        frame = 0;
                    else
                    {
                        PlayingAnimation = false;
                        break;
                    }
                }

                Draw(Frames[frame]);
            }
        }

        public void StopAnimation() => 
            PlayingAnimation = false;

        public void Recycle()
        {
            IsFree = false;

            if (LastDrawnDrawing != InitialDrawing)
                Draw(InitialDrawing);
        }

        public override void Free()
        {
            IsFree = true;
            StopAnimation();
        }
    }
}
