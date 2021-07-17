namespace MMO_Client.Client.Net.Mines.Mobjects
{
    class MobjectData
    {
        public string Key { get; init; }
        public dynamic Value { get; init; }
        public int DataType { get; init; }

        public MobjectData(string key, dynamic value, int type)
        {
            Key = key;
            Value = value;
            DataType = type;
        }

        public override string ToString()
            => $"<{Key}:{DataType}={GetValueAsString()}>";

        /// <summary>
        /// Returns Value converted to a string. 
        /// If Value is an array, returns all the array values converted to a string.
        /// </summary>
        public string GetValueAsString(int depth = 0)
        {
            switch(DataType)
            {
                case MobjectDataType.BOOLEAN_ARRAY:
                case MobjectDataType.FLOAT_ARRAY:
                case MobjectDataType.INTEGER_ARRAY:
                case MobjectDataType.STRING_ARRAY:
                    string result = "";
                    foreach (object obj in Value)
                    {
                        string value = obj.ToString();
                        if (obj is float)
                            value = value.Replace(',', '.');
                        else if (obj is bool)
                            value = value.ToLower();

                        result += value + ",";
                    }

                    if (result.EndsWith(","))
                        result = result.Remove(result.Length - 1, 1);

                    return result;
                case MobjectDataType.MOBJECT_ARRAY:
                    string result2 = "";
                    foreach (Mobject obj in Value)
                    {
                        string value = obj.ToString(depth + 1);
                        result2 += value;
                    }

                    return result2;
                case MobjectDataType.MOBJECT:
                    string v2 = Value.ToString(depth + 1);
                    return v2;
                default:
                    string v = Value.ToString();
                    if (Value is float)
                        v = v.Replace(',', '.');
                    else if (Value is bool)
                        v = v.ToLower();

                    return v;
            }
        }
    }
}
