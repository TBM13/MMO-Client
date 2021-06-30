using System;
using MMO_Client.Common;
using MMO_Client.Client.Net.Mines.IO;
using MMO_Client.Client.Net.Mines.Mobjects;

namespace MMO_Client.Client.Net.Mines
{
    class Message
    {
        public const int HEADER_TYPE = 3;

        private readonly MinesInputStream mis = new();
        private int payload = -1;

        public Message() { }

        public bool NeedsPayload { get => payload == -1; }

        public void Read(ByteArray byteArray)
        {
            int i = payload - mis.Bytes.Count;
            int length = Math.Max(0, Math.Min(byteArray.Bytes.Count - byteArray.ReadPosition, i));

            if (length == 0) // From AS3 Socket Documentation: The default value of 0 causes all available data to be read.
            {
                length = byteArray.Bytes.Count - byteArray.ReadPosition;
                Logger.Debug("Length is 0!", "Mines Message", true);
            }

            mis.WriteBytes(byteArray.ReadBytes(length), mis.Bytes.Count, length);
        }

        public Mobject ToMobject()
        {
            mis.ReadPosition = 0;
            return mis.ReadMobject();
        }

        public bool IsComplete()
        {
            if (payload < mis.Bytes.Count)
                Logger.Error($"Payload is wrong! payload: {payload}, MIS Length: {mis.Bytes.Count}", "Mines Message", true);

            return payload == mis.Bytes.Count;
        }

        public void SetPayload(int payload) => 
            this.payload = payload;
    }
}
