using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace MMO_Client.Client.Net.Mines.IO
{
    internal class ByteArray
    {
        public List<byte> Bytes { get; } = new();
        public int ReadPosition { get; set; }
        public bool RemoveOnRead { get; set; }

        private readonly byte[] intFloatBuffer = new byte[4];
        // 8 KB
        private char[] fastReadStringBuffer = new char[8000];

        public ByteArray() { }

        public void WriteByte(byte b) =>
            Bytes.Add(b);

        public byte ReadByte()
        {
            if (RemoveOnRead)
            {
                byte b = Bytes[0];
                Bytes.RemoveAt(0);

                return b;
            }

            ReadPosition++;
            return Bytes[ReadPosition - 1];
        }

        public void WriteBytes(byte[] bytes, int offset, int length)
        {
            for (int i = offset; i < length; i++)
                WriteByte(bytes[i]);
        }

        public void WriteBytes(List<byte> bytes, int offset, int length)
        {
            for (int i = offset; i < length; i++)
                WriteByte(bytes[i]);
        }

        public byte[] ReadBytes(int length)
        {
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
                result[i] = ReadByte();

            return result;
        }

        public void WriteInt(int i)
        {
            BinaryPrimitives.WriteInt32BigEndian(intFloatBuffer, i);
            WriteBytes(intFloatBuffer, 0, intFloatBuffer.Length);
        }

        public int ReadInt()
        {
            for (int i = 0; i < 4; i++)
                intFloatBuffer[i] = ReadByte();

            return BinaryPrimitives.ReadInt32BigEndian(intFloatBuffer);
        }

        public void WriteBoolean(bool b) =>
            WriteByte((byte)(b ? 1 : 0));

        public bool ReadBoolean() =>
            ReadByte() == 1;

        public void WriteFloat(float f)
        {
            BinaryPrimitives.WriteSingleBigEndian(intFloatBuffer, f);
            WriteBytes(intFloatBuffer, 0, intFloatBuffer.Length);
        }

        public float ReadFloat()
        {
            for (int i = 0; i < intFloatBuffer.Length; i++)
                intFloatBuffer[i] = ReadByte();

            return BinaryPrimitives.ReadSingleBigEndian(intFloatBuffer);
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

            if (!RemoveOnRead)
                ReadPosition += length;

            return result;
        }

        /// <summary>
        /// Reads a string in a faster but unsafer way.
        /// Not recommended for important data as it can't parse special characters.
        /// </summary>
        public string FastReadString()
        {
            int length = ReadInt();

            if (length > fastReadStringBuffer.Length)
            {
                Array.Resize(ref fastReadStringBuffer, length);
                Logger.Debug($"Resized fastReadStringBuffer. String length: {length}", true, "ByteArray");
            }

            for (int i = 0; i < length; i++)
            {
                byte b = ReadByte();
                char c = b switch
                {
                    129 => 'Á',
                    137 => 'É',
                    141 => 'Í',
                    145 => 'Ñ',
                    147 => 'Ó',
                    154 => 'Ú',
                    194 or 195 => '\0',
                    _ => (char)b,
                };

                fastReadStringBuffer[i] = c;
            }

            return new string(fastReadStringBuffer, 0, length).Replace("\0", "");
        }

        public void Clear()
        {
            Bytes.Clear();
            ReadPosition = 0;
            RemoveOnRead = false;
        }
    }
}
