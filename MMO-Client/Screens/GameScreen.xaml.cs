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
using MMO_Client.Client.Assets.Controls;

namespace MMO_Client.Screens
{
    public partial class GameScreen : Window
    {
        public GameScreen()
        {
            InitializeComponent();

            /* WpfDrawingSettings settings = new();
             DirectorySvgConverter converter = new(settings);
             converter.Convert(new DirectoryInfo(@"E:\Usuario SSD\Nueva carpeta (6)\sprites\DefineSprite_2430_assets.MonkeyMC_assets.MonkeyMC"), new DirectoryInfo(@".\Assets\Test\"));
            */

            /*System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            VectorAsset lastAsset = null;

            for (int i = 0; i < 1; i++)
            {
                lastAsset = AssetsManager.CreateVectorAsset("test.1");
                grid.Children.Add(lastAsset.Viewbox);
            }

            sw.Stop(); // 1300 ms*/

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            VectorAsset asset = AssetsManager.CreateVectorAsset("test.1");
            grid.Children.Add(asset.Viewbox);
            sw.Stop();
            asset.Loop = true;
            asset.StartAnimation();

            //webClient.DownloadFile(@"https://static.klarix.cf/1.zaml", @".\cache\1.zaml");
        }
    }
}
