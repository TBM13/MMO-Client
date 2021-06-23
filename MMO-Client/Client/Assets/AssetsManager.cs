using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Markup;
using System.Windows.Media;
using MMO_Client.Common;

namespace MMO_Client.Client.Assets
{
    class AssetsManager
    {
        public const string AssetsPath = @".\Assets";
        private const string title = "Assets Manager";

        private static AssetsManager instance;
        private readonly List<VectorAsset> vectorPool = new();
        private readonly List<ImageAsset> imagePool = new();

        public AssetsManager()
        {
            instance = this;

            if (!Directory.Exists(AssetsPath))
            {
                Directory.CreateDirectory(AssetsPath);
                Logger.Warn("Assets folder didn't exist, so we created it", title);
            }

            Logger.Info("Assets Manager Created", title);
        }

        private ImageAsset GetOrCreateImageAsset(string ID)
        {
            ImageAsset assetToClone = null;
            foreach (ImageAsset v in imagePool)
            {
                if (v.ID == ID)
                {
                    if (v.IsFree)
                    {
                        Logger.Debug($"Recycling free image asset {ID}", title);
                        v.Recycle();
                        return v;
                    }

                    assetToClone = v;
                }
            }

            ImageAsset newAsset = new()
            {
                IsFree = false,
                ID = ID
            };

            if (assetToClone != null)
            {
                Logger.Debug($"Cloning image asset {ID}", title);

                newAsset.Initialize(assetToClone.InitialImage, assetToClone.Frames);
                newAsset.FPS = assetToClone.FPS;
                newAsset.Loop = assetToClone.Loop;
            }
            else
            {
                Logger.Debug($"Creating image asset {ID}", title);

                string path = AssetsPath;
                string[] splittedID = ID.Split(".");
                for (int i = 0; i < splittedID.Length; i++)
                {
                    path += $@"\{splittedID[i]}";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        if (i == splittedID.Length - 1)
                        {
                            // TODO: Request image asset to server
                        }
                    }
                }

                newAsset.LoadFrames(path);
            }

            imagePool.Add(newAsset);
            return newAsset;
        }

        private VectorAsset GetOrCreateVectorAsset(string ID)
        {
            VectorAsset assetToClone = null;
            foreach(VectorAsset v in vectorPool)
            {
                if (v.ID == ID)
                {
                    if (v.IsFree)
                    {
                        Logger.Debug($"Recycling free vector asset {ID}", title);
                        v.Recycle();
                        return v;
                    }

                    assetToClone = v;
                }
            }

            VectorAsset newAsset = new()
            {
                IsFree = false,
                ID = ID
            };

            if (assetToClone != null)
            {
                Logger.Debug($"Cloning vector asset {ID}", title);

                newAsset.Initialize(assetToClone.InitialDrawing, assetToClone.Frames, assetToClone.MaxBounds);
                newAsset.FPS = assetToClone.FPS;
                newAsset.Loop = assetToClone.Loop;
            } 
            else
            {
                Logger.Debug($"Creating vector asset {ID}", title);

                string path = AssetsPath;
                string[] splittedID = ID.Split(".");
                foreach(string s in splittedID)
                {
                    path += $@"\{s}";

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                DrawingGroup drawing = Xaml2Drawing(path + @"\1.xaml");
                List<DrawingGroup> frames = null;

                if (drawing == null)
                {
                    Logger.Error($"Couldn't get the drawing of the asset \"{ID}\"", title);
                    drawing = Xaml2Drawing($@"{AssetsPath}\MMOClient\error.xaml");
                }
                else
                {
                    int framesCount = Directory.GetFiles(path).Length;
                    if (framesCount > 1)
                    {
                        frames = new List<DrawingGroup> { drawing };
                        for (int i = 1; i < framesCount; i++)
                        {
                            DrawingGroup d = Xaml2Drawing(path + $@"\{i + 1}.xaml", frames);
                            frames.Add(d);
                        }
                    }

                    Logger.Debug($"Loaded {frames.Count} frames for {ID}", title);
                }

                if (frames == null)
                    newAsset.Initialize(drawing);
                else
                    newAsset.Initialize(drawing, frames);
            }

            vectorPool.Add(newAsset);
            return newAsset;
        }

        public static DrawingGroup Xaml2Drawing(string xamlPath, List<DrawingGroup> frames = null)
        {
            if (!File.Exists(xamlPath))
            {
                if (!Zaml2Xaml(xamlPath.Replace(".xaml", ".zaml")))
                    return null;
            }

            using StreamReader streamReader = new(xamlPath);
            if (frames != null)
            {
                /*string line = streamReader.Peek();
                if (line.StartsWith(""))
                {

                }*/
            }

            return (DrawingGroup)XamlReader.Load(streamReader.BaseStream);
        }

        public static bool Zaml2Xaml(string zamlPath)
        {
            if (!File.Exists(zamlPath))
            {
                // TODO: Request SVG to server
                return false;
            }

            using FileStream originalFileStream = new(zamlPath, FileMode.Open, FileAccess.Read);

            string newFilePath = zamlPath.Replace(".zaml", ".xaml");
            using FileStream decompressedFileStream = File.Create(newFilePath);
            using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedFileStream);

            return true;
        }

        public static VectorAsset CreateVectorAsset(string ID) => 
            instance.GetOrCreateVectorAsset(ID);

        public static ImageAsset CreateImageAsset(string ID) =>
            instance.GetOrCreateImageAsset(ID);
    }
}
