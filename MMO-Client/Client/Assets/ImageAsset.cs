using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MMO_Client.Client.Assets
{
    internal class ImageAsset : Asset
    {
        public static BitmapImage ErrorBitmapImage { get; set; }

        public Image Image { get; init; }

        public Dictionary<string, List<BitmapImage>> Frames { get; private set; }
        public int Framerate { get; set; } = 24;
        public bool Loop { get; set; }
        public bool PlayingAnimation { get; private set; }

        public bool IsBroken { get; private set; }

        public ImageAsset()
        {
            Image = new();

            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.HighQuality);
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.LowQuality);
            //RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
        }

        public void Clone(ImageAsset assetToClone)
        {
            Frames = assetToClone.Frames;

            Framerate = assetToClone.Framerate;
            Loop = assetToClone.Loop;
        }

        public void LoadAllFrames(Dictionary<string, int> animations = null)
        {
            if (Frames != null)
                return;

            string path = AssetsManager.GetAssetPath(ID);
            if (path.StartsWith("<<null>>"))
                goto error;

            int framesCount = Directory.GetFiles(path).Length;
            if (framesCount == 0)
                goto error;

            Frames = new();
            if (animations == null)
            {
                List<BitmapImage> list = new();

                for (int i = 1; i <= framesCount; i++)
                {
                    using StreamReader stream = new($@"{path}\{i}.png");
                    if (stream.BaseStream.Length <= 4)  // This means the file is referencing another frame
                    {
                        int frameToCopy = int.Parse(stream.ReadLine());
                        list.Add(list[frameToCopy - 1]);
                    }
                    else
                    {
                        BitmapImage frame = new();
                        frame.BeginInit();
                        frame.StreamSource = stream.BaseStream;
                        frame.CacheOption = BitmapCacheOption.OnLoad;
                        frame.EndInit();

                        list.Add(frame);
                    }
                }

                Frames[""] = list;
            }
            else
            {
                int pairIndex = 0;
                foreach(KeyValuePair<string, int> pair in animations)
                {
                    List<BitmapImage> list = new();
                    int i;

                    bool shouldAddNextFrame()
                    {
                        if (i > framesCount)
                            return false;

                        foreach (KeyValuePair<string, int> p in animations)
                        {
                            if (p.Key != pair.Key && i >= p.Value)
                                return false;
                        }

                        return true;
                    }

                    for (i = pair.Value; shouldAddNextFrame(); i++)
                    {
                        using StreamReader stream = new($@"{path}\{i}.png");
                        if (stream.BaseStream.Length <= 4)  // This means the file is referencing another frame
                        {
                            int frameToCopy = int.Parse(stream.ReadLine());
                            list.Add(list[frameToCopy - 1]);
                        }
                        else
                        {
                            BitmapImage frame = new();
                            frame.BeginInit();
                            frame.StreamSource = stream.BaseStream;
                            frame.CacheOption = BitmapCacheOption.OnLoad;
                            frame.EndInit();

                            list.Add(frame);
                        }
                    }

                    Frames[pair.Key] = list;
                    pairIndex++;
                }
            }

            return;

        error:
            IsBroken = true;
            Logger.Error("Couldn't load image asset", false, ID);

            Image.Width = ErrorBitmapImage.Width;
            Image.Height = ErrorBitmapImage.Height;
            Draw(ErrorBitmapImage);
        }

        public void DrawFrame(int frame)
        {
            if (Frames == null)
                return;

            int i = 1;
            foreach (KeyValuePair<string, List<BitmapImage>> pair in Frames)
            {
                for (int j = 0; j < pair.Value.Count; j++)
                {
                    if (i == frame)
                    {
                        if (Image.Source == null)
                        {
                            // Image Assets are exported with 300% zoom, so divide the size by 3
                            Image.Width = pair.Value[j].Width / 3;
                            Image.Height = pair.Value[j].Height / 3;
                        }

                        Draw(pair.Value[j]);
                        return;
                    }

                    i++;
                }
            }

            Logger.Error($"Invalid Frame {frame}", false, ID);
            Draw(ErrorBitmapImage);
        }

        private void Draw(BitmapImage image)
        {
            Image.Source = image;
        }

        /// <param name="name">Name of the animation. If the asset doesn't contain any animation names, set this to an empty string.</param>
        public async void PlayAnimation(string name, bool loop)
        {
            Loop = loop;

            if (PlayingAnimation)
            {
                Logger.Warn("Animation is already playing", false, ID);
                return;
            }

            if (Frames == null)
            {
                Logger.Error("No animation frames loaded", false, ID);
                return;
            }

            if (!Frames.ContainsKey(name) || Frames[name].Count <= 1)
            {
                Logger.Error($"Animation {name} doesn't contain any frames or only contains one", false, ID);
                return;
            }

            PlayingAnimation = true;

            List<BitmapImage> frames = Frames[name];

            int delayTime = 1000 / Framerate;
            int frame = 0;
            while (PlayingAnimation)
            {
                await Task.Delay(delayTime);
                frame++;
                if (frame == frames.Count)
                {
                    if (Loop)
                        frame = 0;
                    else
                    {
                        PlayingAnimation = false;
                        break;
                    }
                }

                Draw(frames[frame]);
            }
        }

        public void StopAnimation() =>
            PlayingAnimation = false;

        public override void Free()
        {
            IsFree = true;
            StopAnimation();
        }
    }
}
