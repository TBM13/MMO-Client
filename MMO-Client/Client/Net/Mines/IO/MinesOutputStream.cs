using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Common;

namespace MMO_Client.Client.Net.Mines.IO
{
    class MinesOutputStream : ByteArray
    {
        public const int HEADER_TYPE_PING = 1;
		public const int HEADER_TYPE_LOGIN = 2;
		public const int HEADER_TYPE_MOBJECT = 3;

        public MinesOutputStream() { }

        public void WritePing(string s)
        {
            WriteByte(HEADER_TYPE_PING);
            WriteString(s);
        }

        private void WriteHeader(MinesOutputStream mos, char s, MobjectData mData)
        {
            mos.WriteByte((byte)s);
            mos.WriteString(mData.Key);
        }

        private void WriteArrayHeader(MinesOutputStream mos, char s, MobjectData mData)
        {
            WriteHeader(mos, s, mData);
            mos.WriteInt(mData.Value.Length);
        }

        public void WriteMobject(Mobject mObj)
        {
            MinesOutputStream mos = new();

            int i = 0;
            foreach (MobjectData mData in mObj.Iterator())
            {
                mos.WriteMobjectData(mData);
                i++;
            }

            WriteInt(i);
            WriteBytes(mos.Bytes, 0, mos.Bytes.Count);
        }

        public void WriteMobjectData(MobjectData mData)
        {
            MinesOutputStream mos = new();

            switch (mData.DataType)
            {
                case MobjectDataType.INTEGER:
                    WriteHeader(mos, 'i', mData);
                    mos.WriteInt((int)mData.Value);

                    break;
                case MobjectDataType.FLOAT:
                    WriteHeader(mos, 'f', mData);
                    mos.WriteFloat((float)mData.Value);

                    break;
                case MobjectDataType.STRING:
                    WriteHeader(mos, 's', mData);
                    mos.WriteString((string)mData.Value);

                    break;
                case MobjectDataType.BOOLEAN:
                    WriteHeader(mos, 'b', mData);
                    mos.WriteBoolean((bool)mData.Value);

                    break;
                case MobjectDataType.MOBJECT:
                    WriteHeader(mos, 'm', mData);
                    mos.WriteMobject((Mobject)mData.Value);

                    break;
                case MobjectDataType.INTEGER_ARRAY:
                    WriteArrayHeader(mos, 'I', mData);
                    foreach (object obj in mData.Value)
                        mos.WriteInt((int)obj);

                    break;
                case MobjectDataType.FLOAT_ARRAY:
                    WriteArrayHeader(mos, 'F', mData);
                    foreach (object obj in mData.Value)
                        mos.WriteFloat((float)obj);

                    break;
                case MobjectDataType.STRING_ARRAY:
                    WriteArrayHeader(mos, 'S', mData);
                    foreach (object obj in mData.Value)
                        mos.WriteString((string)obj);

                    break;
                case MobjectDataType.BOOLEAN_ARRAY:
                    WriteArrayHeader(mos, 'B', mData);
                    foreach(object obj in mData.Value)
                        mos.WriteBoolean((bool)obj);

                    break;
                case MobjectDataType.MOBJECT_ARRAY:
                    WriteArrayHeader(mos, 'M', mData);
                    foreach (object obj in mData.Value)
                    {
                        mos.WriteByte((byte)'m');
                        mos.WriteMobject((Mobject)obj);
                    }

                    break;
                default:
                    Logger.Fatal($"Unable to write Mobject data! Unknown DataType {mData.DataType}", "Mines Output Stream");
                    break;
            }

            WriteBytes(mos.Bytes, 0, mos.Bytes.Count);
        }
    }
}
