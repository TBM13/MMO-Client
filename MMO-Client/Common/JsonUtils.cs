using System.Collections.Generic;
using System.Text.Json;

namespace MMO_Client
{
    internal static class JsonUtils
    {
        public static dynamic[] ParseJsonArray(string data)
        {
            JsonSerializerOptions options = new()
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            dynamic[] array = JsonSerializer.Deserialize<dynamic[]>(data, options);

            for (int i = 0; i < array.Length; i++)
                array[i] = ParseJsonElement(array[i]);

            return array;
        }
        
        public static Dictionary<string, dynamic> ParseJsonDictionary(string data)
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

        public static dynamic ParseJsonElement(JsonElement elem)
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
                    return ParseJsonDictionary(elem.GetRawText());
                case JsonValueKind.Array:
                    dynamic[] array = new dynamic[elem.GetArrayLength()];
                    int i2 = 0;
                    foreach (JsonElement e in elem.EnumerateArray())
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
