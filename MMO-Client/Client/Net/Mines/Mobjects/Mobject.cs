using System.Collections.Generic;

namespace MMO_Client.Client.Net.Mines
{
    internal class Mobject
    {
        public Dictionary<string, int> Integers { get; } = new();
        public Dictionary<string, float> Floats { get; } = new();
        public Dictionary<string, string> Strings { get; } = new();
        public Dictionary<string, bool> Booleans { get; } = new();
        public Dictionary<string, Mobject> Mobjects { get; } = new();

        public Dictionary<string, int[]> IntegerArrays { get; } = new();
        public Dictionary<string, float[]> FloatArrays { get; } = new();
        public Dictionary<string, string[]> StringArrays { get; } = new();
        public Dictionary<string, bool[]> BooleanArrays { get; } = new();
        public Dictionary<string, Mobject[]> MobjectArrays { get; } = new();

        public List<MobjectData> Iterator()
        {
            List<MobjectData> list = new();

            foreach (dynamic pair in Integers)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.INTEGER));
            foreach (dynamic pair in Booleans)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.BOOLEAN));
            foreach (dynamic pair in Floats)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.FLOAT));
            foreach (dynamic pair in Strings)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.STRING));
            foreach (dynamic pair in Mobjects)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.MOBJECT));

            foreach (dynamic pair in IntegerArrays)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.INTEGER_ARRAY));
            foreach (dynamic pair in BooleanArrays)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.BOOLEAN_ARRAY));
            foreach (dynamic pair in FloatArrays)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.FLOAT_ARRAY));
            foreach (dynamic pair in StringArrays)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.STRING_ARRAY));
            foreach (dynamic pair in MobjectArrays)
                list.Add(new MobjectData(pair.Key, pair.Value, MobjectDataType.MOBJECT_ARRAY));

            return list;
        }

        public override string ToString() =>
            ToString(0);

        public string ToString(int depth = 0)
        {
            string result = "";

            int i = 0;
            string newLine = "\n";
            while (i < depth)
            {
                newLine += "   ";
                i++;
            }

            foreach (MobjectData mData in Iterator())
            {
                if (mData.DataType is MobjectDataType.MOBJECT or MobjectDataType.MOBJECT_ARRAY)
                    result += $"[{mData.Key}] {$"{mData.Value.GetType()}->{newLine + "   "}{mData.GetValueAsString(depth)}"}\n";
                else
                    result += $"[{mData.Key}] {$"{mData.Value.GetType()}-> {mData.GetValueAsString(depth)}"}\n";
            }

            if (depth > 0)
                result = result.Replace("\n", newLine);

            return result;
        }

        public void AddData(MobjectData mData)
        {
            switch (mData.DataType)
            {
                case MobjectDataType.INTEGER:
                    Integers[mData.Key] = (int)mData.Value;
                    break;
                case MobjectDataType.FLOAT:
                    Floats[mData.Key] = (float)mData.Value;
                    break;
                case MobjectDataType.STRING:
                    Strings[mData.Key] = (string)mData.Value;
                    break;
                case MobjectDataType.BOOLEAN:
                    Booleans[mData.Key] = (bool)mData.Value;
                    break;
                case MobjectDataType.MOBJECT:
                    Mobjects[mData.Key] = (Mobject)mData.Value;
                    break;
                case MobjectDataType.INTEGER_ARRAY:
                    IntegerArrays[mData.Key] = (int[])mData.Value;
                    break;
                case MobjectDataType.FLOAT_ARRAY:
                    FloatArrays[mData.Key] = (float[])mData.Value;
                    break;
                case MobjectDataType.STRING_ARRAY:
                    StringArrays[mData.Key] = (string[])mData.Value;
                    break;
                case MobjectDataType.BOOLEAN_ARRAY:
                    BooleanArrays[mData.Key] = (bool[])mData.Value;
                    break;
                case MobjectDataType.MOBJECT_ARRAY:
                    MobjectArrays[mData.Key] = (Mobject[])mData.Value;
                    break;
                default:
                    Logger.Error($"Unable to add data! Unknown DataType {mData.DataType}");
                    break;
            }
        }

        /*public static Mobject StringToMobject(string data)
        {
            Mobject mobj = new Mobject();

            int skipAmount = 0;
            foreach(string s in data.Split('\n'))
            {
                if (skipAmount > 0)
                {
                    skipAmount--;
                    continue;
                }

                if (string.IsNullOrEmpty(s))
                    continue;

                string[] splittedS = s.Split(new string[] { ";;;" }, StringSplitOptions.None);

                string key = splittedS[0];
                int dataType = int.Parse(splittedS[1]);
                string value = splittedS[2];

                switch (dataType)
                {
                    case MobjectDataType.STRING:
                        mobj.Strings[key] = value;
                        break;
                    case MobjectDataType.BOOLEAN:
                        mobj.Booleans[key] = bool.Parse(value);
                        break;
                    case MobjectDataType.INTEGER:
                        mobj.Integers[key] = int.Parse(value);
                        break;
                    case MobjectDataType.FLOAT:
                        mobj.Floats[key] = float.Parse(value);
                        break;
                    case MobjectDataType.MOBJECT:
                        string mobjData = data.Remove(0, data.IndexOf($"{key};;;{dataType};;;") + key.Length + splittedS[1].Length + 6);
                        string[] mobjData2 = mobjData.Split(new string[] { "\n\n" }, StringSplitOptions.None);

                        if (mobjData.Length == 0) // This means the mobject is empty
                        {
                            mobj.Mobjects[key] = new Mobject();
                            break;
                        }

                        int mobjsAmount = 0;
                        foreach (string d in mobjData.Split('\n'))
                        {
                            if (d.Contains($";;;{MobjectDataType.MOBJECT};;;"))
                                mobjsAmount++;
                        }

                        skipAmount = mobjData2[mobjsAmount].Split('\n').Length;
                        mobj.Mobjects[key] = StringToMobject(mobjData2[0]);
                        break;
                    case MobjectDataType.STRING_ARRAY:
                        string[] values = value.Split(',');
                        mobj.StringArrays[key] = values;
                        break;
                    case MobjectDataType.BOOLEAN_ARRAY:
                        string[] values2 = value.Split(',');

                        bool[] boolArray = new bool[values2.Length];
                        for (int i = 0; i < values2.Length; i++)
                            boolArray[i] = bool.Parse(values2[i]);

                        mobj.BooleanArrays[key] = boolArray;
                        break;
                    case MobjectDataType.INTEGER_ARRAY:
                        string[] values3 = value.Split(',');

                        int[] intArray = new int[values3.Length];
                        for (int i = 0; i < values3.Length; i++)
                            intArray[i] = int.Parse(values3[i]);

                        mobj.IntegerArrays[key] = intArray;
                        break;
                    case MobjectDataType.FLOAT_ARRAY:
                        string[] values4 = value.Split(',');

                        float[] floatArray = new float[values4.Length];
                        for (int i = 0; i < values4.Length; i++)
                            floatArray[i] = float.Parse(values4[i]);

                        mobj.FloatArrays[key] = floatArray;
                        break;
                    case MobjectDataType.MOBJECT_ARRAY:
                        
                        break;
                    default:
                        Logger.Instance.Fatal($"StringToMobject-: Unknown data type {dataType}", "Mobject");
                        break;
                }
            }

            return mobj;
        }*/
    }
}
