using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Client.Client.Net.Mines.IO
{
    class ByteArray
    {
        public List<byte> Bytes { get; private set; } = new();
        public int ReadPosition { get; set; } = 0;

        public void WriteByte(byte b) =>
            Bytes.Add(b);

        public byte ReadByte()
        {
            ReadPosition++;
            return Bytes[ReadPosition - 1];
        }

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

        public List<byte> ReadBytes(int length)
        {
            List<byte> result = new();

            for (int i = 0; i < length; i++)
                result.Add(ReadByte());

            return result;
        }

        public void WriteInt(int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b, 0, b.Length);

            WriteBytes(b, 0, b.Length);
        }

        public int ReadInt()
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = ReadByte();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public void WriteBoolean(bool b) =>
            WriteByte((byte)(b ? 1 : 0));

        public bool ReadBoolean()
        {
            byte b = ReadByte();
            return b == 1;
        }

        public void WriteFloat(float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b, 0, b.Length);

            for (int i = 0; i < b.Length; i++)
                WriteByte(b[i]);
        }

        public float ReadFloat()
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = ReadByte();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToSingle(bytes, 0);
        }

        public void WriteString(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);

            WriteInt(b.Length);
            WriteBytes(b, 0, b.Length);
        }

        public string ReadString()
        {
            int length = ReadInt();
            string result = Encoding.ASCII.GetString(Bytes.ToArray(), ReadPosition, length);

            ReadPosition += length;
            return result;
        }
    }
}
