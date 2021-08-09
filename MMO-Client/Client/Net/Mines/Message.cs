using System;
using System.Collections.Generic;
using MMO_Client.Client.Net.Mines.IO;

namespace MMO_Client.Client.Net.Mines
{
    internal class Message
    {
        public const int HEADER_TYPE = 3;

        public int Payload { get; private set; } = -1;
        public bool NeedsPayload { get => Payload == -1; }
        public int Length { get => mis.Bytes.Count; }
        public List<byte> Bytes { get => mis.Bytes; }

        private readonly MinesInputStream mis = new();

        public Message() { }

        public void Read(ByteArray byteArray)
        {
            int i = Payload - mis.Bytes.Count;
            int length = Math.Max(0, Math.Min(byteArray.Bytes.Count - byteArray.ReadPosition, i));

#if DEBUG
            if (length == 0) // From AS3 Socket Documentation: The default value of 0 causes all available data to be read.
            {
                //length = byteArray.Bytes.Count - byteArray.ReadPosition;
                Logger.Fatal("Length is 0!");
            }
#endif

            mis.WriteBytes(byteArray.ReadBytes(length), 0, length);
        }

        public Mobject ToMobject()
        {
            mis.ReadPosition = 0;
            return mis.ReadMobject();
        }

        public bool IsComplete()
        {
            if (Payload < mis.Bytes.Count)
                Logger.Fatal($"Payload is wrong! payload: {Payload}, MIS Length: {mis.Bytes.Count}");

            return Payload == mis.Bytes.Count;
        }

        public void SetPayload(int payload) =>
            Payload = payload;
    }
}
