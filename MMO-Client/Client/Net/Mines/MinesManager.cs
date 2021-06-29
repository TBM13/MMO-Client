#define MinesDebug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MMO_Client.Common;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.Net.Mines.IO;

namespace MMO_Client.Client.Net.Mines
{
    class MinesManager
    {
        public static MinesManager Instance;
        private const string title = "Mines Manager";

        private readonly Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        private static ManualResetEvent sendDone = new(false);
        private static ManualResetEvent receiveDone = new(false);

        public MinesManager()
        {
            Instance = this;

            Logger.Info("Mines Manager Created", title);
        }

        public bool Connect(string host, int port)
        {
            try
            {
                Logger.Info($"Connecting to {host}:{port}", title);
                socket.Connect(host, port);

                return socket.Connected;
            }
            catch (Exception e)
            {
                Logger.Error($"Error while connecting to {host}:{port}", title);
                Logger.Error(e.ToString(), title);
                return false;
            }
        }

        public void Send(Mobject mobj)
        {
            MinesOutputStream mos = new();
            mos.WriteMobject(mobj);

#if MinesDebug
            string result = "";
            byte[] b = new byte[mos.Bytes.Count];
            for (int i = 0; i < mos.Bytes.Count; i++)
            {
                result += mos.Bytes[i].ToString() + ";";
                b[i] = mos.Bytes[i];
            }

            Logger.Debug($"Sending Mobject {mobj.ToString().Replace("\n", ",")}", title);
            Logger.Debug($"Bytes: {result}", title);
#endif

            byte[] bytesToSend = new byte[mos.Bytes.Count + 5];
            bytesToSend[0] = 3;

            byte[] lengthBytes = BitConverter.GetBytes(mos.Bytes.Count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes, 0, lengthBytes.Length);

            for (int i = 0; i < lengthBytes.Length; i++)
                bytesToSend[i + 1] = lengthBytes[i];

            for (int i = 5; i < bytesToSend.Length; i++)
                bytesToSend[i] = mos.Bytes[i - 5];

            byte[] receiveBuffer = new byte[256000]; //256 KB
            int bytesSent = socket.Send(bytesToSend);
            int bytesRec = socket.Receive(receiveBuffer);

            Logger.Debug($"Got {bytesRec} bytes", title);

            MinesInputStream mis = new(receiveBuffer);
            Logger.Debug(mis.ReadMobject().ToString(), title);
        }
    }
}
