using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Markup;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;
using System;

namespace MMO_Client.Client.Assets
{
    /// <summary>
    /// The Assets Manager is responsible for loading and caching assets.
    /// </summary>
    internal class AssetsManager
    {
        public static AssetsManager Instance;
        public const string AssetsPath = @".\Assets";

        private readonly List<VectorAsset> vectorPool = new();
        private readonly List<ImageAsset> imagePool = new();

        public AssetsManager()
        {
            Instance = this;

            if (!Directory.Exists(AssetsPath))
            {
                Directory.CreateDirectory(AssetsPath);
                Logger.Warn("Assets folder didn't exist, so we created it");
            }

            BitmapImage errorImg = new();
            errorImg.BeginInit();
            errorImg.UriSource = new Uri($@"{AssetsPath}\MMOClient\error.png", UriKind.RelativeOrAbsolute);
            errorImg.CacheOption = BitmapCacheOption.OnLoad;
            errorImg.EndInit();
            ImageAsset.ErrorBitmapImage = errorImg;
        }

        public static string GetAssetPath(string ID)
        {
            string path = AssetsPath;

            string[] splittedID = ID.Split(".");
            for (int i = 0; i < splittedID.Length; i++)
                path += $@"\{splittedID[i]}";

            if (!Directory.Exists(path))
                return "<<null>>" + path;

            return path;
        }

        public static dynamic GetAssetProperties(string ID)
        {
            string path = AssetsPath;
            string[] splittedID = ID.Split(".");
            for (int i = 0; i < splittedID.Length - 1; i++)
                path += $@"\{splittedID[i]}";

            path += $@"\{splittedID[^1]}.json";

            if (!File.Exists(path))
                return null;

            return JObject.Parse(File.ReadAllText(path));
        }

        #region Image Asset
        public ImageAsset GetOrCreateImageAsset(string ID)
        {
            ImageAsset assetToClone = null;
            for (int i = 0; i < imagePool.Count; i++)
            {
                ImageAsset asset = imagePool[i];

                if (asset.IsBroken)
                {
                    Logger.Debug($"Removing broken image asset {ID} from pool");
                    imagePool.RemoveAt(i);

                    i--;
                    continue;
                }

                if (asset.ID == ID)
                {
                    if (asset.IsFree)
                    {
                        Logger.Debug($"Recycling free image asset {ID}");

                        asset.Unfree();
                        return asset;
                    }

                    assetToClone = asset;
                }
            }

            ImageAsset newAsset = new() { ID = ID };

            if (assetToClone != null)
            {
                Logger.Debug($"Cloning image asset {ID}");

                newAsset.Clone(assetToClone);
            }
            else
            {
                Logger.Debug($"Creating image asset {ID}");

                string path = GetAssetPath(ID);
                if (path.StartsWith("<<null>>"))
                {
                    path = path.Remove(0, 8);
                    Directory.CreateDirectory(path);
                    // TODO: Download image asset from server
                }
            }

            imagePool.Add(newAsset);
            return newAsset;
        }
        #endregion

        #region Vector Asset
        public VectorAsset GetOrCreateVectorAsset(string ID)
        {
            VectorAsset assetToClone = null;
            foreach(VectorAsset v in vectorPool)
            {
                if (v.ID == ID)
                {
                    if (v.IsFree)
                    {
                        Logger.Debug($"Recycling free vector asset {ID}");

                        v.Recycle();
                        return v;
                    }

                    assetToClone = v;
                }
            }

            VectorAsset newAsset = new() { ID = ID };

            if (assetToClone != null)
            {
                Logger.Debug($"Cloning vector asset {ID}");

                newAsset.Initialize(assetToClone.InitialDrawing, assetToClone.Frames, assetToClone.MaxBounds);
                newAsset.FPS = assetToClone.FPS;
                newAsset.Loop = assetToClone.Loop;
            } 
            else
            {
                Logger.Debug($"Creating vector asset {ID}");

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
                    Logger.Error($"Couldn't get the drawing of the asset \"{ID}\"");
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

                    Logger.Debug($"Loaded {frames.Count} frames for {ID}");
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
        #endregion
    }
}
