using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MMO_Client.Common.Logger;
using MMO_Client.Client.Assets.Controls;

namespace MMO_Client.Client.Assets
{
    class VectorAsset : Asset
    {
        public readonly CustomSvgViewbox Viewbox;
        public int FPS = 24;
        public bool Loop;

        private readonly List<DrawingGroup> frames = null;
        private bool playingAnimation = false;
        private readonly Rect maxBounds;

        public VectorAsset() { }

        public VectorAsset(DrawingGroup drawing)
        {
            Viewbox = new();
            maxBounds = drawing.Bounds;

            Draw(drawing);
        }

        public VectorAsset(DrawingGroup drawing, List<DrawingGroup> frames, bool loop)
        {
            Viewbox = new();

            this.frames = frames;
            Loop = loop;

            maxBounds = drawing.Bounds;
            foreach(DrawingGroup d in frames)
            {
                if (d.Bounds.X > maxBounds.X)
                    maxBounds.X = d.Bounds.X;

                if (d.Bounds.Y > maxBounds.Y)
                    maxBounds.Y = d.Bounds.Y;
            }

            Draw(drawing);
        }

        public void Draw(DrawingGroup drawing) => 
            Viewbox.AddDrawing(drawing, maxBounds);

        public async void StartAnimation()
        {
            if (playingAnimation)
            {
                Logger.Warn("Animation is already playing", ID);
                return;
            }

            if (frames == null)
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
                if (frame == frames.Count)
                {
                    if (Loop)
                        frame = 0;
                    else
                    {
                        playingAnimation = false;
                        break;
                    }
                }

                Draw(frames[frame]);
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
