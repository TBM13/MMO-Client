using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using MMO_Client.Client.Assets;

namespace MMO_Client.Screens.GameScreen
{
    public partial class GameScreen : Window
    {
        WebClient webClient = new();

        public GameScreen()
        {
            InitializeComponent();

            if (!Directory.Exists(@".\cache"))
                Directory.CreateDirectory(@".\cache");

            //webClient.DownloadFile(@"https://static.klarix.cf/1.zaml", @".\cache\1.zaml");

            //WpfDrawingSettings settings = new();
            //FileSvgConverter converter = new(settings);
            //converter.Convert(@".\cache\1.zaml");
            //Zaml2Xaml(@".\cache\1.zaml");

            /*List<DrawingGroup> frames = new();
            for (int i = 1; i < 42; i++)
            {
                StreamReader s = new($@"E:\Usuario SSD\Nueva carpeta (6)\sprites\DefineSprite_75\{i}.xaml");
                frames.Add((DrawingGroup)System.Windows.Markup.XamlReader.Load(s.BaseStream));
            }

            VectorAsset vA = new(frames[0], frames, true);
            grid.Children.Add(vA);
            vA.StartAnimation();*/
        }

        private void Zaml2Xaml(string zamlPath)
        {
            using FileStream originalFileStream = new(zamlPath, FileMode.Open, FileAccess.Read);

            string newFilePath = zamlPath.Replace(".zaml", ".xaml");
            using FileStream decompressedFileStream = File.Create(newFilePath);
            using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);

            decompressionStream.CopyTo(decompressedFileStream);
        }
    }
}
