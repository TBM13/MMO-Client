using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.Net.Mines.IO;
using MMO_Client.Client.Net.Mines.Event;

namespace MMO_Client.Client.Net.Mines
{
    /// <summary>
    /// The Mines Server Module is responsible for the client-server communication.
    /// </summary>
    internal class MinesServer
    {
        public static MinesServer Instance;

        public Events.Mines1Event OnConnect;
        public Events.Mines1Event OnLogout;
        public Events.Mines1Event OnMessage;

        private readonly Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        private Message pendingMessage;

        public MinesServer()
        {
            Instance = this;

            Logger.Info("Initialized");
        }

        /// <summary>
        /// Connects the socket to the specified host and starts the read loop if the connection was successful.
        /// </summary>
        public void Connect(string host, int port)
        {
            try
            {
                Logger.Info($"Connecting to {host}:{port}");
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
                Logger.Error($"Error while connecting to {host}:{port}");
                Logger.Error(e.ToString());

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

#if NetworkDebugVerbose
            Logger.Debug($"Sending Mobject {mobj.ToString().Replace("\n", ",")}", Name);

            string result = "";
            byte[] b = new byte[mos.Bytes.Count];
            for (int i = 0; i < mos.Bytes.Count; i++)
            {
                result += mos.Bytes[i].ToString() + ";";
                b[i] = mos.Bytes[i];
            }

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
#if NetworkDebugVerbose
                Logger.Debug($"{bytesRead} bytes read", Name);
#endif

                byte[] buffer = (byte[])ar.AsyncState;
                HandleSocketData(buffer);
            }
            else
                Logger.Warn("ReceiveCallback: bytesRead is 0 !!!", true);
        }

        private void HandleSocketData(byte[] data)
        {
            ByteArray byteArray = new();
            byteArray.WriteBytes(data, 0, data.Length);

            if (pendingMessage == null)
            {
                int header = byteArray.ReadByte();
                if (header != Message.HEADER_TYPE)
                {
                    Logger.Error($"Unknown Header {(char)header} [{header}]", true);
                    return;
                }

                pendingMessage = new();
            }

            if (pendingMessage.NeedsPayload)
            {
                if (data.Length < 4)
                    return;

                pendingMessage.SetPayload(byteArray.ReadInt());
            }

            pendingMessage.Read(byteArray);
            if (pendingMessage.IsComplete())
            {
                ProcessMessage(pendingMessage);
                pendingMessage = null;
            }
#if NetworkDebugVerbose
            else
                Logger.Debug($"Message isn't complete, waiting for more bytes ({pendingMessage.Length}/{pendingMessage.Payload})", Name);
#endif
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

                    if (mObj.Mobjects["mobject"].Booleans.ContainsKey("success"))
                    {
                        success = mObj.Mobjects["mobject"].Booleans["success"];
                        if (!success)
                        {
                            Logger.Debug("Does the mobject contain any error code?");
                        }
                    }

                    OnMessage?.Invoke(new MinesEvent(success, errorCode, mObj.Mobjects["mobject"]));
                    break;
                case "login":
                    //OnLogin?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings["errorCode"], mObj.Mobjects["mobject"]));
                    Logger.Debug("Login!!!", true);
                    break;
                case "logout":
                    OnLogout?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings["errorCode"], mObj.Mobjects["mobject"]));
                    Logger.Debug("Logout!!!", true);
                    break;
                default:
                    Logger.Error($"Unknown Message Type \"{mObj.Strings["type"]}\"", true);
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
            mobj.Strings["size"] = "5367773";
            mobj.Strings["hash"] = hash;
            mobj.Strings["type"] = "login";
            mobj.Strings["check"] = "haha";
            mobj.Strings["username"] = username;

            Send(mobj);
        }
    }
}
