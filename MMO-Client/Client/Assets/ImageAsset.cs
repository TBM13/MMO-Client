using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MMO_Client.Client.Assets;
using MMO_Client.Common;

namespace MMO_Client.Client.Assets
{
    class ImageAsset : Asset
    {
        public System.Windows.Controls.Image Image { get; init; }
        public BitmapImage InitialImage { get; private set; } = null;
        public BitmapImage LastDrawnImage { get; private set; } = null;

        public List<BitmapImage> Frames { get; private set; } = null;
        public int FPS { get; set; } = 24;
        public bool Loop { get; set; }
        public bool PlayingAnimation { get; private set; }

        public ImageAsset()
        {
            Image = new();
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.HighQuality);
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.LowQuality);
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
        }

        public void LoadFrames(string directory)
        {
            int filesCount = Directory.GetFiles(directory).Length;

            InitialImage = new BitmapImage(new Uri($@"{directory}\1.png", UriKind.RelativeOrAbsolute));

            if (filesCount > 1)
            {
                Frames = new();
                Frames.Add(InitialImage);

                for (int i = 1; i < filesCount; i++)
                {
                    using StreamReader stream = new($@"{directory}\{i}.png");
                    if (stream.BaseStream.Length <= 4)  // This means the file is referencing another frame
                    {
                        int frameToCopy = int.Parse(stream.ReadLine());
                        Frames.Add(Frames[frameToCopy - 1]);
                    }
                    else
                    {
                        BitmapImage frame = new();
                        frame.BeginInit();
                        frame.StreamSource = stream.BaseStream;
                        frame.CacheOption = BitmapCacheOption.OnLoad;
                        frame.EndInit();

                        Frames.Add(frame);
                    }
                }
            }

            Draw(InitialImage);
        }

        public void Draw(BitmapImage image)
        {
            Image.Source = image;
            LastDrawnImage = image;
        }

        public async void StartAnimation()
        {
            if (PlayingAnimation)
            {
                Logger.Warn("Animation is already playing", ID);
                return;
            }

            if (Frames == null)
            {
                Logger.Warn("No animation frames loaded", ID);
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

            if (LastDrawnImage != InitialImage)
                Draw(InitialImage);
        }

        public override void Free()
        {
            IsFree = true;
            StopAnimation();
        }
    }
}
