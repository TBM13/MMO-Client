using MMO_Client.Client.Config;
using MMO_Client.Screens;
using System.Windows;

namespace MMO_Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            // Setup Logger
            Logger.SetOutputScreen(LoggerOutput);

            // Setup Game Settings
            _ = new Settings();

            // Setup Login Event
            LoginScreen.OnLoginAttempt += OnLoginAttempt;
        }

        private void OnLoginAttempt(string username, string loginId, bool success)
        {
            if (!success)
                return;

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

            LoadScreen.ResetProgressbar();

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

            LoadScreen.ResetProgressbar();

            Settings.Instance.OnSettingsLoaded += (success) =>
            {
                Settings.Instance.OnSettingsLoaded = null;
                if (!success)
                {
                    LoadScreen.ShowError("Couldn't load Gameplay Settings", true);
                    return;
                }

                LoadScreen.Visibility = Visibility.Hidden;
            };

            Settings.Instance.LoadGameplaySettings();
        }
    }
}
