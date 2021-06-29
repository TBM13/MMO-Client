﻿using System.Collections.Generic;
using MMO_Client.Common;

namespace MMO_Client.Client.Net.Mines.Mobjects
{
    class Mobject
    {
        public Dictionary<string, int> Integers { get; private set; } = new();
        public Dictionary<string, float> Floats { get; private set; } = new();
        public Dictionary<string, string> Strings { get; private set; } = new();
        public Dictionary<string, bool> Booleans { get; private set; } = new();
        public Dictionary<string, Mobject> Mobjects { get; private set; } = new();

        public Dictionary<string, int[]> IntegerArrays { get; private set; } = new();
        public Dictionary<string, float[]> FloatArrays { get; private set; } = new();
        public Dictionary<string, string[]> StringArrays { get; private set; } = new();
        public Dictionary<string, bool[]> BooleanArrays { get; private set; } = new();
        public Dictionary<string, Mobject[]> MobjectArrays { get; private set; } = new();

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

        public override string ToString()
        {
            string result = "";

            foreach(MobjectData mData in Iterator())
                result += $"[{mData.Key}] {mData.GetValueAsString()}\n";

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
                    Logger.Fatal($"Unable to add data! Unknown DataType {mData.DataType}", "Mobject");
                    break;
            }
        }
    }
}
