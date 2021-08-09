using MMO_Client.Client.Config;
using MMO_Client.Client.Net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MMO_Client.Screens
{
    public partial class ServersScreen : UserControl
    {
        internal Events.String2Event OnServerSelected { get; set; }
        
        private record Server(string Host, string Port, double Usage, string Name);

        private Server[] servers;

        private readonly Brush serverButtonBrushForeground = Utils.BrushFromHex("#F9D611");
        private readonly Brush serverButtonBrushBackground = Utils.BrushFromHex("#BDDEFF");

        public ServersScreen()
        {
            InitializeComponent();

            LoadScreen.OnRetryClick += LoadServers;
        }

        public void LoadServers()
        {
            LoadScreen.LabelText = "Loading Servers...";
            List.Items.Clear();

            LoadScreen.Visibility = Visibility.Visible;
            LoadScreen.ResetProgressbar();
            
            LoadFile lf = new();
            lf.OnFileLoaded += OnServersFileLoaded;

            lf.OnError += (_) =>
                LoadScreen.ShowError("Error while loading servers", true);

            lf.Load(Settings.Instance.Dictionary["connection"]["directory"] + "&" + Utils.GetUnixTime().ToString());
        }

        private void OnServersFileLoaded(string data)
        {
            LoadScreen.Visibility = Visibility.Hidden;

            dynamic[] serversArray = JsonUtils.ParseJsonArray(data);
            servers = new Server[serversArray.Length];

            LoadScreen.Visibility = Visibility.Hidden;

            for (int i = 0; i < serversArray.Length; i++)
            {
                dynamic server = serversArray[i];
                servers[i] = new Server(server["host"], server["port"], Math.Round(server["usage"] * 100, 2), server["name"].ToUpperInvariant());
                AddServerButton(servers[i]);
            }
        }

        private void AddServerButton(Server server)
        {
            Grid grid = new()
            {
                Height = 32
            };

            ProgressBar progressBar = new()
            {
                Value = server.Usage,
                Foreground = serverButtonBrushForeground,
                Background = serverButtonBrushBackground
            };

            Label serverName = new()
            {
                Content = server.Name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };

            Label serverUsage = new()
            {
                Content = $"{server.Usage}%",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            grid.Children.Add(progressBar);
            grid.Children.Add(serverName);
            grid.Children.Add(serverUsage);

            grid.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                    OnServerSelected?.Invoke(server.Host, server.Port);
            };

            List.Items.Add(grid);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) =>
            LoadServers();
    }
}