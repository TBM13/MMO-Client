using System.Text;

namespace MMO_Client.Client.Net.Mines
{
    internal class MobjectData
    {
        public string Key { get; init; }
        public dynamic Value { get; init; }
        public MobjectDataType DataType { get; init; }

        public MobjectData(string key, dynamic value, MobjectDataType type)
        {
            Key = key;
            Value = value;
            DataType = type;
        }

        public override string ToString() =>
            $"<{Key}:{DataType}={GetValueAsString()}>";

        /// <summary>
        /// Returns Value converted to a string. 
        /// If Value is an array, returns all the array values converted to a string.
        /// </summary>
        public string GetValueAsString(int depth = 0)
        {
            switch (DataType)
            {
                case MobjectDataType.BOOLEAN_ARRAY:
                case MobjectDataType.FLOAT_ARRAY:
                case MobjectDataType.INTEGER_ARRAY:
                case MobjectDataType.STRING_ARRAY:
                    StringBuilder sb = new();

                    foreach (object obj in Value)
                    {
                        string value = obj.ToString();
                        if (obj is float)
                            value = value.Replace(',', '.');
                        else if (obj is bool)
                            value = value.ToLowerInvariant();

                        sb.Append(value);
                        sb.Append(',');
                    }

                    if (sb[^1] == ',')
                        sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                case MobjectDataType.MOBJECT_ARRAY:
                    StringBuilder sb2 = new();

                    foreach (Mobject obj in Value)
                    {
                        string value = obj.ToString(depth + 1);
                        sb2.Append(value);
                    }

                    return sb2.ToString();
                case MobjectDataType.MOBJECT:
                    string v2 = Value.ToString(depth + 1);
                    return v2;
                default:
                    string v = Value.ToString();
                    if (Value is float)
                        v = v.Replace(',', '.');
                    else if (Value is bool)
                        v = v.ToLowerInvariant();

                    return v;
            }
        }
    }
}
