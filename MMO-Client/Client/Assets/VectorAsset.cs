using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MMO_Client.Common;
using MMO_Client.Client.Assets.Controls;

namespace MMO_Client.Client.Assets
{
    class VectorAsset : Asset
    {
        public CustomSvgViewbox Viewbox;

        public DrawingGroup Drawing;
        public List<DrawingGroup> Frames = null;
        public Rect MaxBounds;
        public int FPS = 24;
        public bool Loop;

        private bool playingAnimation = false;

        public VectorAsset() => 
            Viewbox = new();

        public void Initialize(DrawingGroup drawing)
        {
            Drawing = drawing;
            MaxBounds = drawing.Bounds;

            Draw(drawing);
        }

        public void Initialize(DrawingGroup drawing, List<DrawingGroup> frames, bool loop)
        {
            Drawing = drawing;

            Frames = frames;
            Loop = loop;

            MaxBounds = drawing.Bounds;
            foreach (DrawingGroup d in frames)
            {
                if (d.Bounds.X > MaxBounds.X)
                    MaxBounds.X = d.Bounds.X;

                if (d.Bounds.Y > MaxBounds.Y)
                    MaxBounds.Y = d.Bounds.Y;
            }

            Draw(drawing);
        }

        public void Draw(DrawingGroup drawing) => 
            Viewbox.AddDrawing(drawing, MaxBounds);

        public async void StartAnimation()
        {
            if (playingAnimation)
            {
                Logger.Warn("Animation is already playing", ID);
                return;
            }

            if (Frames == null)
            {
                Logger.Warn("No animation frames provided", ID);
                return;
            }

            playingAnimation = true;

            int delayTime = 1000 / FPS;

            int frame = 0;
            while (playingAnimation)
            {
                await Task.Delay(delayTime);
                frame++;
                if (frame == Frames.Count)
                {
                    if (Loop)
                        frame = 0;
                    else
                    {
                        playingAnimation = false;
                        break;
                    }
                }

                Draw(Frames[frame]);
            }

            Logger.Debug("Task End", ID);
        }

        public void StopAnimation()
        {
            if (!playingAnimation)
            {
                Logger.Warn("Animation isn't playing", ID);
                return;
            }

            playingAnimation = false;
        }
    }
}
