using MMO_Client.Client.Net;
using System.Collections.Generic;
using System.Text.Json;

namespace MMO_Client.Client.Config
{
    internal static class Settings
    {
        public static Dictionary<string, dynamic> Dictionary { get; } = new();

        private const string settingsUri = "https://cdn-ar.mundogaturro.com/juego/cfgs/settings.json";
        private const string gameplayUri = "https://cdn-ar.mundogaturro.com/juego/cfgs/gameplay.json";

        public static void LoadSettings()
        {
            Logger.Info("Loading Settings...");
            LoadFile(settingsUri);
        }

        public static void LoadGameplaySettings()
        {
            Logger.Info("Loading Gameplay Settings...");
            LoadFile(gameplayUri);
        }

        private static void LoadFile(string url)
        {
            LoadFile lf = new();
            lf.OnFileLoaded += (data) =>
            {
                Dictionary<string, dynamic> dic = ParseJson(data);
                foreach (KeyValuePair<string, dynamic> pair in dic)
                    Dictionary.Add(pair.Key, pair.Value);
            };

            lf.Load(url);
        }

        private static Dictionary<string, dynamic> ParseJson(string data)
        {
            JsonSerializerOptions options = new()
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            Dictionary<string, dynamic> dic = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(data, options);
            foreach (KeyValuePair<string, dynamic> pair in dic)
                dic[pair.Key] = ParseJsonElement(pair.Value);

            return dic;
        }

        private static dynamic ParseJsonElement(JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.String:
                    return elem.GetString();
                case JsonValueKind.Number:
                    if (elem.TryGetInt32(out int i))
                        return i;

                    return elem.GetDouble();
                case JsonValueKind.Object:
                    return ParseJson(elem.GetRawText());
                case JsonValueKind.Array:
                    dynamic[] array = new dynamic[elem.GetArrayLength()];
                    int i2 = 0;
                    foreach(JsonElement e in elem.EnumerateArray())
                    {
                        array[i2] = ParseJsonElement(e);
                        i2++;
                    }

                    return array;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Undefined:
                    return null;
                default:
                    Logger.Error($"Unsupported Json Element '{elem.ValueKind}'. Ignoring!");
                    return null;
            }
        }
    }
}
