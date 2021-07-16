using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using MMO_Client.Common;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.Net.Mines.IO;
using MMO_Client.Client.Net.Mines.Event;

namespace MMO_Client.Client.Net.Mines
{
    /// <summary>
    /// The Mines Server Module is responsible for the client-server communication.
    /// </summary>
    internal class MinesServer : Module
    {
        public static MinesServer Instance;

        public Events.Mines1Event OnConnect;
        public Events.Mines1Event OnLogin;
        public Events.Mines1Event OnLogout;
        public Events.Mines1Event OnMessage;

        private readonly Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        public override string Name { get; } = "Mines";

        public override void Initialize()
        {
            Instance = this;

            Logger.Info("Initialized", Name);
        }

        public override void Terminate()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Logger.Info("Terminated", Name);
        }

        /// <summary>
        /// Connects the socket to the specified host and starts the read loop if the connection was successful.
        /// </summary>
        public void Connect(string host, int port)
        {
            try
            {
                Logger.Info($"Connecting to {host}:{port}", Name);
                socket.Connect(host, port);

                if (socket.Connected)
                {
                    OnConnect?.Invoke(new MinesEvent(true, null, null));

                    ReadLoop();
                    return;
                }

                OnConnect?.Invoke(new MinesEvent(false, "0", null));
            }
            catch (Exception e)
            {
                Logger.Error($"Error while connecting to {host}:{port}", Name);
                Logger.Error(e.ToString(), Name);

                OnConnect?.Invoke(new MinesEvent(false, "0", null));
            }
        }

        /// <summary>
        /// Converts the Mobject to a byte array and sends it.
        /// </summary>
        public void Send(Mobject mobj)
        {
            MinesOutputStream mos = new();
            mos.WriteMobject(mobj);

#if NetworkDebug
            string result = "";
            byte[] b = new byte[mos.Bytes.Count];
            for (int i = 0; i < mos.Bytes.Count; i++)
            {
                result += mos.Bytes[i].ToString() + ";";
                b[i] = mos.Bytes[i];
            }

            Logger.Debug($"Sending Mobject {mobj.ToString().Replace("\n", ",")}", Name);
            Logger.Debug($"Bytes: {result}", Name);
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
                Logger.Debug($"{bytesRead} bytes read", Name);

                byte[] buffer = (byte[])ar.AsyncState;
                HandleSocketData(buffer);
            }
            else
                Logger.Warn("ReceiveCallback: bytesRead is 0 !!!", Name, true);
        }

        private void HandleSocketData(byte[] data)
        {
            ByteArray byteArray = new();
            byteArray.WriteBytes(data, 0, data.Length);

            int header = byteArray.ReadByte();
            if (header != Message.HEADER_TYPE)
            {
                Logger.Error($"Unknown Header {(char)header} [{header}]", Name, true);
                return;
            }

            Message msg = new();
            msg.SetPayload(byteArray.ReadInt());

            msg.Read(byteArray);

            if (msg.IsComplete())
                ProcessMessage(msg);
            else
                Logger.Error("Message isn't complete!!!", Name, true);
        }

        private void ProcessMessage(Message msg)
        {
            Mobject mObj = msg.ToMobject();

            switch(mObj.Strings["type"])
            {
                case "ping":
                    break;
                case "data":
                    bool success = true;
                    string errorCode = "<empty>";

                    if (mObj.Booleans.ContainsKey("result"))
                    {
                        success = mObj.Booleans["result"];
                        if (!success)
                            errorCode = mObj.Strings.ContainsKey("errorCode") ? mObj.Strings["errorCode"] : mObj.Strings["errorMessage"];
                    }

                    OnMessage?.Invoke(new MinesEvent(success, errorCode, mObj.Mobjects["mobject"]));
                    break;
                case "login":
                    OnLogin?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings["errorCode"], mObj.Mobjects["mobject"]));
                    Logger.Debug("Login!!!", Name, true);
                    break;
                case "logout":
                    OnLogout?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings["errorCode"], mObj.Mobjects["mobject"]));
                    Logger.Debug("Logout!!!", Name, true);
                    break;
                default:
                    Logger.Error($"Unknown Message Type \"{mObj.Strings["type"]}\"", Name, true);
                    break;
            }
        }

        public void SendMobject(Mobject mObj)
        {
            Mobject newMobj = new();
            newMobj.Strings["type"] = "data";
            newMobj.Mobjects["mobject"] = mObj;

            Send(newMobj);
        }

        public void LoginWithID(string username, string hash)
        {
            Mobject mobj = new();
            mobj.Strings["size"] = "5370589";
            mobj.Strings["hash"] = hash;
            mobj.Strings["type"] = "login";
            mobj.Strings["check"] = "haha";
            mobj.Strings["username"] = username;

            Send(mobj);
        }
    }
}
