using MMO_Client.Client.Net.Mines.Mobjects;

namespace MMO_Client.Client.Net.Mines.IO
{
    class MinesInputStream : ByteArray
    {
        public MinesInputStream() { }

        public MobjectData ReadMobjectData()
        {
            MobjectData mData;

            char dataType = (char)ReadByte();
            switch(dataType)
            {
                case 'b':
                    mData = new(ReadString(), ReadBoolean(), MobjectDataType.BOOLEAN);
                    break;
                case 'i':
                    mData = new(ReadString(), ReadInt(), MobjectDataType.INTEGER);
                    break;
                case 's':
                    mData = new(ReadString(), ReadString(), MobjectDataType.STRING);
                    break;
                case 'f':
                    mData = new(ReadString(), ReadFloat(), MobjectDataType.FLOAT);
                    break;
                case 'm':
                    mData = new(ReadString(), ReadMobject(), MobjectDataType.MOBJECT);
                    break;

                case 'B':
                    string key = ReadString();
                    int length = ReadInt();

                    bool[] array = new bool[length];
                    for (int i = 0; i < length; i++)
                        array[i] = ReadBoolean();

                    mData = new(key, array, MobjectDataType.BOOLEAN_ARRAY);
                    break;
                case 'I':
                    string key2 = ReadString();
                    int length2 = ReadInt();

                    int[] array2 = new int[length2];
                    for (int i = 0; i < length2; i++)
                        array2[i] = ReadInt();

                    mData = new(key2, array2, MobjectDataType.INTEGER_ARRAY);
                    break;
                case 'S':
                    string key3 = ReadString();
                    int length3 = ReadInt();

                    string[] array3 = new string[length3];
                    for (int i = 0; i < length3; i++)
                        array3[i] = ReadString();

                    mData = new(key3, array3, MobjectDataType.STRING_ARRAY);
                    break;
                case 'F':
                    string key4 = ReadString();
                    int length4 = ReadInt();

                    float[] array4 = new float[length4];
                    for (int i = 0; i < length4; i++)
                        array4[i] = ReadFloat();

                    mData = new(key4, array4, MobjectDataType.FLOAT_ARRAY);
                    break;
                case 'M':
                    string key5 = ReadString();
                    int length5 = ReadInt();

                    Mobject[] array5 = new Mobject[length5];
                    for (int i = 0; i < length5; i++)
                    {
                        DiscardHeader();
                        array5[i] = ReadMobject();
                    }

                    mData = new(key5, array5, MobjectDataType.MOBJECT_ARRAY);
                    break;

                default:
                    Logger.Error($"Invalid MobjectData header while reading: {dataType} [{(byte)dataType}]", true);
                    return new("", "", MobjectDataType.STRING);
            }

            return mData;
        }

        public Mobject ReadMobject()
        {
            Mobject mObj = new();

            int length = ReadInt();
            for (int i = 0; i < length; i++)
                mObj.AddData(ReadMobjectData());

            return mObj;
        }

        public void DiscardHeader() =>
            ReadPosition++;
    }
}
