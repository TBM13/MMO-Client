using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public AssetsManager()
        {
            instance = this;

            if (!Directory.Exists(AssetsPath))
                Directory.CreateDirectory(AssetsPath);

            Logger.Info("Assets Manager Created", title);
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
                        v.IsFree = false;
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

                newAsset.Drawing = assetToClone.Drawing;
                newAsset.Frames = assetToClone.Frames;
                newAsset.MaxBounds = assetToClone.MaxBounds;
                newAsset.FPS = assetToClone.FPS;
                newAsset.Loop = assetToClone.Loop;

                newAsset.Draw(newAsset.Drawing);
            } 
            else
            {
                Logger.Debug($"Creating vector asset {ID}", title);

                string path = AssetsPath;
                string[] splittedID = ID.Split(".");
                foreach(string s in splittedID)
                {
                    path += @"\s";

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                path += ".xaml";
                DrawingGroup drawing = Xaml2Drawing(path);
                if (drawing == null)
                {
                    Logger.Error($"Couldn't get the drawing of {ID}", title);
                    drawing = Xaml2Drawing("pack://application:,,,/Client/Assets/error.xaml");
                }
                
                newAsset.Initialize(drawing);
            }

            vectorPool.Add(newAsset);
            return newAsset;
        }

        public static DrawingGroup Xaml2Drawing(string xamlPath)
        {
            if (!File.Exists(xamlPath))
            {
                if (!Zaml2Xaml(xamlPath.Replace(".xaml", ".zaml")))
                    return null;
            }

            Logger.Debug("Converting XAML to Drawing...", title);
            using StreamReader streamReader = new(xamlPath);
            return (DrawingGroup)XamlReader.Load(streamReader.BaseStream);
        }

        public static bool Zaml2Xaml(string zamlPath)
        {
            if (!File.Exists(zamlPath))
            {
                // TODO: Request SVG to server
                return false;
            }

            Logger.Debug("Converting ZAML to XAML...", title);
            using FileStream originalFileStream = new(zamlPath, FileMode.Open, FileAccess.Read);

            string newFilePath = zamlPath.Replace(".zaml", ".xaml");
            using FileStream decompressedFileStream = File.Create(newFilePath);
            using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedFileStream);

            return true;
        }

        public static VectorAsset CreateVectorAsset(string ID) => 
            instance.GetOrCreateVectorAsset(ID);
    }
}
