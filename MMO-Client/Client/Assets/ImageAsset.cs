using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MMO_Client.Common;

namespace MMO_Client.Client.Assets
{
    class ImageAsset : Asset
    {
        public Image Image { get; init; }
        public BitmapImage InitialImage { get; private set; } = null;
        public BitmapImage LastDrawnImage { get; private set; } = null;

        public double Width { get; private set; }
        public double Height { get; private set; }

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

        public void Initialize(BitmapImage initialImage, List<BitmapImage> frames, double width, double height)
        {
            InitialImage = initialImage;
            Frames = frames;

            Image.Width = width;
            Image.Height = height;
            Width = width;
            Height = height;

            Draw(initialImage);
        }

        public void LoadFrames(string directory)
        {
            if (!Directory.Exists(directory))
                goto error;

            int filesCount = Directory.GetFiles(directory).Length;
            if (filesCount == 0)
                goto error;

            InitialImage = new();
            InitialImage.BeginInit();
            InitialImage.UriSource = new Uri($@"{directory}\1.png", UriKind.RelativeOrAbsolute);
            InitialImage.CacheOption = BitmapCacheOption.OnLoad;
            InitialImage.EndInit();

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

                Logger.Debug($"Loaded {Frames.Count} frames", ID);
            }

            // Image Assets are exported with 300% zoom, so divide the size by 3
            Image.Width = InitialImage.Width / 3;
            Image.Height = InitialImage.Height / 3;
            Width = Image.Width;
            Height = Image.Height;

            Draw(InitialImage);
            return;

        error:
            Logger.Error("Couldn't load image asset", ID);

            BitmapImage errorImg = new();
            errorImg.BeginInit();
            errorImg.UriSource = new Uri($@"{AssetsManager.AssetsPath}\MMOClient\error.png", UriKind.RelativeOrAbsolute);
            errorImg.CacheOption = BitmapCacheOption.OnLoad;
            errorImg.EndInit();

            Image.Width = errorImg.Width;
            Image.Height = errorImg.Height;

            Draw(errorImg);
        }

        public void Draw(BitmapImage image)
        {
            Image.Source = image;
            LastDrawnImage = image;
        }

        public async void StartAnimation(bool loop)
        {
            Loop = loop;

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
