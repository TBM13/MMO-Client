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
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Mines;

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
            Show();
        }
    }
}
