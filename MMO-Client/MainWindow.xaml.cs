using MMO_Client.Client.Assets;
using MMO_Client.Client.Config;
using MMO_Client.Client.Net;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.World.Rooms;
using MMO_Client.Screens;
using System.Threading.Tasks;
using System.Windows;

namespace MMO_Client
{
    public partial class MainWindow : Window
    {
        // Temp solution to store username
        public static string Username { get; private set; }
        
        private string username;
        private string loginId;
        
        public MainWindow()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            // Setup Logger
            Logger.SetOutputScreen(LoggerOutput);

            // Handle Unobserved Task Exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Setup Game Settings
            _ = new Settings();

            // Setup Login Event
            LoginScreen.OnLoginAttempt += OnLoginAttempt;
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) =>
            Logger.Error($"Unhandled Task Exception\n{e.Exception.InnerException}", false, "Main");

        private void OnLoginAttempt(string username, string loginId, bool success)
        {
            if (!success)
                return;

            Username = username;
            this.username = username;
            this.loginId = loginId;

            LoginScreen.OnLoginAttempt -= OnLoginAttempt;
            LoginScreen.Visibility = Visibility.Hidden;

            LoadScreen.Visibility = Visibility.Visible;
            LoadGameSettings();
        }

        private void LoadGameSettings()
        {
            LoadScreen.ResetProgressbar();
            LoadScreen.LabelText = "Loading Game Settings";

            LoadScreen.OnRetryClick = null;
            LoadScreen.OnRetryClick += LoadGameSettings;

            Settings.Instance.OnSettingsLoaded += (success) =>
            {
                Settings.Instance.OnSettingsLoaded = null;
                if (!success)
                {
                    LoadScreen.ShowError("Couldn't load Game Settings", true);
                    return;
                }

                LoadGameplaySettings();
            };

            Settings.Instance.LoadSettings();
        }

        private void LoadGameplaySettings()
        {
            LoadScreen.ResetProgressbar();
            LoadScreen.LabelText = "Loading Gameplay Settings";

            LoadScreen.OnRetryClick = null;
            LoadScreen.OnRetryClick += LoadGameplaySettings;

            Settings.Instance.OnSettingsLoaded += (success) =>
            {
                Settings.Instance.OnSettingsLoaded = null;
                if (!success)
                {
                    LoadScreen.ShowError("Couldn't load Gameplay Settings", true);
                    return;
                }

                LoadScreen.Visibility = Visibility.Hidden;
                SetupGame();
            };

            Settings.Instance.LoadGameplaySettings();
        }

        private void SetupGame()
        {
            // Assets
            _ = new AssetsManager();

            // Network
            _ = new MinesServer();
            _ = new NetworkManager();

            // World
            _ = new RoomManager();

            ShowServersList();
        }

        private void ShowServersList()
        {
            ServersScreen.OnServerSelected += OnServerSelected;
            ServersScreen.Visibility = Visibility.Visible;
            ServersScreen.LoadServers();
        }

        private void OnServerSelected(string host, string port)
        {
            ServersScreen.OnServerSelected -= OnServerSelected;
            ServersScreen.Visibility = Visibility.Hidden;

            LoadScreen.Visibility = Visibility.Visible;
            Connect(host, port);
        }

        private void Connect(string host, string port)
        {
            LoadScreen.ResetProgressbar();
            LoadScreen.LabelText = $"Connecting to {host}:{port}";

            void retry() =>
                ShowServersList();

            LoadScreen.OnRetryClick = null;
            LoadScreen.OnRetryClick += retry;

            MinesServer.Instance.OnConnect += OnMinesConnect;
           _ = MinesServer.Instance.ConnectAsync(host, int.Parse(port) + 1);
        }

        private void OnMinesConnect(MinesEvent mEvent)
        {
            MinesServer.Instance.OnConnect -= OnMinesConnect;

            if (!mEvent.Success)
            {
                LoadScreen.ShowError("Couldn't connect to server", true);
                return;
            }

            LoadScreen.Visibility = Visibility.Hidden;
            GameScreen.Visibility = Visibility.Visible;
            GameScreen.Setup();

            MinesServer.Instance.LoginWithID(username, loginId);
        }
    }
}
