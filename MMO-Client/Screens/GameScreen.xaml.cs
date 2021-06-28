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
using MMO_Client.Client.World.Rooms;
using Newtonsoft.Json.Linq;

namespace MMO_Client.Screens
{
    public partial class GameScreen : Window
    {
        private static GameScreen instance;

        public static double GameWidth { get => instance.canvas.Width; }
        public static double GameHeight { get => instance.canvas.Height; }

        /// <summary>
        /// The objects look too small in comparison to MG, so we have to multiply their size.
        /// </summary>
        public const double SizeMultiplier = 1.3;

        public GameScreen()
        {
            instance = this;
            InitializeComponent();

            /*List<string> content = new();
            for (int i = 1; i < 664; i++)
            {
                using StreamReader stream = new($@"E:\Usuario SSD\Nueva carpeta (6)\sprites\DefineSprite_2430_assets.MonkeyMC_assets.MonkeyMC\{i}.png");
                string c = stream.ReadToEnd();

                bool reused = false;
                for (int j = 0; j < content.Count; j++)
                {
                    if (content[j] == c)
                    {
                        reused = true;
                        stream.Close();
                        content.Add("asd");
                        File.WriteAllText($@"E:\Usuario SSD\Nueva carpeta (6)\sprites\DefineSprite_2430_assets.MonkeyMC_assets.MonkeyMC\{i}.png", $"{j + 1}");
                        break;
                    }
                }

                if (reused) continue;

                content.Add(c);
            }*/



            Room room = new(JObject.Parse(File.ReadAllText(@"E:\Usuario SSD\Nueva carpeta (6)\ROSEDAL2.json")));
            canvas.Children.Add(room.Canvas);

            /*ImageAsset asset = AssetsManager.CreateImageAsset("test.monoPng300");
            canvas.Children.Add(asset.Image);*/
        }
    }
}
