#define MinesDebug

using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using MMO_Client.Common;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.Net.Mines.IO;
using MMO_Client.Client.Net.Mines.Event;

namespace MMO_Client.Client.Net.Mines
{
    class MinesManager
    {
        public static MinesManager Instance;

        public delegate void Mines1Event(MinesEvent arg1);
        public Mines1Event OnLogin;
        public Mines1Event OnLogout;
        public Mines1Event OnMessage;

        private const string title = "Mines Manager";
        private readonly Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        public MinesManager()
        {
            Instance = this;

            Logger.Info("Mines Manager Created", title);
        }

        /// <summary>
        /// Connects the socket to the specified host and starts the read loop if the connection was successful.
        /// </summary>
        /// <returns>Returns true if the connection was successful.</returns>
        public bool Connect(string host, int port)
        {
            try
            {
                Logger.Info($"Connecting to {host}:{port}", title);
                socket.Connect(host, port);

                if (socket.Connected)
                {
                    ReadLoop();
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Error($"Error while connecting to {host}:{port}", title);
                Logger.Error(e.ToString(), title);
                return false;
            }
        }

        /// <summary>
        /// Converts the Mobject to a byte array and sends it.
        /// </summary>
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
            bytesToSend[0] = Message.HEADER_TYPE; // Write header type

            byte[] lengthBytes = BitConverter.GetBytes(mos.Bytes.Count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes, 0, lengthBytes.Length);

            for (int i = 0; i < lengthBytes.Length; i++) // Write mos length
                bytesToSend[i + 1] = lengthBytes[i];

            for (int i = 5; i < bytesToSend.Length; i++) // Write mos bytes
                bytesToSend[i] = mos.Bytes[i - 5];

            socket.BeginSend(bytesToSend, 0, bytesToSend.Length, 0, new AsyncCallback(SendCallback), null);
        }

        private void SendCallback(IAsyncResult ar) => 
            socket.EndSend(ar);

        /// <summary>
        /// Continually checks if there is available data for reading, and reads it.
        /// </summary>
        private async void ReadLoop()
        {
            while (true)
            {
                if (socket.Available > 0)
                {
                    byte[] buffer = new byte[socket.Available];
                    socket.BeginReceive(buffer, 0, socket.Available, 0, new AsyncCallback(ReceiveCallback), buffer);
                }

                await Task.Delay(5);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            int bytesRead = socket.EndReceive(ar);

            if (bytesRead > 0)
            {
                Logger.Debug($"{bytesRead} bytes read", title);

                byte[] buffer = (byte[])ar.AsyncState;
                HandleSocketData(buffer);
            }
            else
                Logger.Warn("ReceiveCallback: bytesRead is 0 !!!", title, true);
        }

        private void HandleSocketData(byte[] data)
        {
            ByteArray byteArray = new();
            byteArray.WriteBytes(data, 0, data.Length);

            int header = byteArray.ReadByte();
            if (header != Message.HEADER_TYPE)
            {
                Logger.Error($"Unknown Header {(char)header} [{header}]", title, true);
                return;
            }

            Message msg = new();
            msg.SetPayload(byteArray.ReadInt());

            msg.Read(byteArray);

            if (msg.IsComplete())
                ProcessMessage(msg);
            else
                Logger.Error("Message isn't complete!!!", title, true);
        }

        private void ProcessMessage(Message msg)
        {
            Mobject mObj = msg.ToMobject();

            switch(mObj.Strings["type"])
            {
                case "ping":
                    break;
                case "data":
                    OnMessage?.Invoke(new MinesEvent(true, null, mObj.Mobjects["mobject"]));
                    break;
                case "login":
                    OnLogin?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings["errorCode"], mObj.Mobjects["mobject"]));
                    Logger.Debug("Login!!!", title, true);
                    break;
                case "logout":
                    OnLogout?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings["errorCode"], mObj.Mobjects["mobject"]));
                    Logger.Debug("Logout!!!", title, true);
                    break;
                default:
                    Logger.Error($"Unknown Message Type \"{mObj.Strings["type"]}\"", title, true);
                    break;
            }
        }
    }
}
