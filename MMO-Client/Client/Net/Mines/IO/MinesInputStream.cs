namespace MMO_Client.Client.Net.Mines.IO
{
    internal class MinesInputStream : ByteArray
    {
        public MinesInputStream() { }

        public MobjectData ReadMobjectData()
        {
            MobjectData mData;

            char dataType = (char)ReadByte();
            switch (dataType)
            {
                case 'b':
                    mData = new MobjectData(FastReadString(), ReadBoolean(), MobjectDataType.BOOLEAN);
                    break;
                case 'i':
                    mData = new MobjectData(FastReadString(), ReadInt(), MobjectDataType.INTEGER);
                    break;
                case 's':
                    mData = new MobjectData(FastReadString(), FastReadString(), MobjectDataType.STRING);
                    break;
                case 'f':
                    mData = new MobjectData(FastReadString(), ReadFloat(), MobjectDataType.FLOAT);
                    break;
                case 'm':
                    mData = new MobjectData(FastReadString(), ReadMobject(), MobjectDataType.MOBJECT);
                    break;

                case 'B':
                    string key = FastReadString();
                    int length = ReadInt();

                    bool[] array = new bool[length];
                    for (int i = 0; i < length; i++)
                        array[i] = ReadBoolean();

                    mData = new MobjectData(key, array, MobjectDataType.BOOLEAN_ARRAY);
                    break;
                case 'I':
                    string key2 = FastReadString();
                    int length2 = ReadInt();

                    int[] array2 = new int[length2];
                    for (int i = 0; i < length2; i++)
                        array2[i] = ReadInt();

                    mData = new MobjectData(key2, array2, MobjectDataType.INTEGER_ARRAY);
                    break;
                case 'S':
                    string key3 = FastReadString();
                    int length3 = ReadInt();

                    string[] array3 = new string[length3];
                    for (int i = 0; i < length3; i++)
                        array3[i] = FastReadString();

                    mData = new MobjectData(key3, array3, MobjectDataType.STRING_ARRAY);
                    break;
                case 'F':
                    string key4 = FastReadString();
                    int length4 = ReadInt();

                    float[] array4 = new float[length4];
                    for (int i = 0; i < length4; i++)
                        array4[i] = ReadFloat();

                    mData = new MobjectData(key4, array4, MobjectDataType.FLOAT_ARRAY);
                    break;
                case 'M':
                    string key5 = FastReadString();
                    int length5 = ReadInt();

                    Mobject[] array5 = new Mobject[length5];
                    for (int i = 0; i < length5; i++)
                    {
                        DiscardHeader();
                        array5[i] = ReadMobject();
                    }

                    mData = new MobjectData(key5, array5, MobjectDataType.MOBJECT_ARRAY);
                    break;

                default:
                    Logger.Fatal($"Invalid MobjectData header while reading: {dataType} [{(byte)dataType}]");
                    return new MobjectData("", "", MobjectDataType.STRING);
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
