using System;
using System.Collections.Generic;
using System.Text;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Common;

namespace MMO_Client.Client.Net.Mines.IO
{
    class MinesOutputStream
    {
        public const int HEADER_TYPE_PING = 1;
		public const int HEADER_TYPE_LOGIN = 2;
		public const int HEADER_TYPE_MOBJECT = 3;

        public List<byte> Bytes { get; private set; } = new();

        public MinesOutputStream() { }

        public void WriteByte(byte b) =>
            Bytes.Add(b);

        public void WriteBytes(byte[] bytes, int offset = 0, int length = 0)
        {
            for (int i = offset; i < length; i++)
                WriteByte(bytes[i]);
        }

        public void WriteBytes(List<byte> bytes, int offset = 0, int length = 0)
        {
            for (int i = offset; i < length; i++)
                WriteByte(bytes[i]);
        }

        public void WriteInt(int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b, 0, b.Length);

            WriteBytes(b, 0, b.Length);
        }

        public void WriteBoolean(bool b) =>
            WriteByte((byte)(b ? 1 : 0));

        public void WriteFloat(float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b, 0, b.Length);

            for (int i = 0; i < b.Length; i++)
                WriteByte(b[i]);
        }

        public void WriteString(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);

            WriteInt(b.Length);
            WriteBytes(b, 0, b.Length);
        }

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
