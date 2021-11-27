using MMO_Client.Client.Net;
using System.Collections.Generic;

namespace MMO_Client.Client.Config
{
    internal class Settings
    {
        public static Settings Instance { get; private set; }

        public Events.Bool1Event OnSettingsLoaded { get; set; }
        public Dictionary<string, dynamic> Dictionary { get; } = new();

        private const string settingsUri = "https://cdn-ar.mundogaturro.com/juego/cfgs/settings.json";
        private const string gameplayUri = "https://cdn-ar.mundogaturro.com/juego/cfgs/gameplay.json";

        public Settings() =>
            Instance = this;

        public void LoadSettings()
        {
            Logger.Info("Loading Settings...");
            LoadFile(settingsUri);
        }

        public void LoadGameplaySettings()
        {
            Logger.Info("Loading Gameplay Settings...");
            LoadFile(gameplayUri);
        }

        private void LoadFile(string url)
        {
            LoadFile lf = new();
            lf.OnFileLoaded += (data) =>
            {
                Dictionary<string, dynamic> dic = JsonUtils.ParseJsonDictionary(data);
                foreach (KeyValuePair<string, dynamic> pair in dic)
                    Dictionary.Add(pair.Key, pair.Value);

                OnSettingsLoaded?.Invoke(true);
            };

            lf.OnError += (_) =>
                OnSettingsLoaded?.Invoke(false);

            _ = lf.LoadAsync(url);
        }
    }
}
