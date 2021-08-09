using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MMO_Client.Client.Net.Mines.IO;

namespace MMO_Client.Client.Net.Mines
{
    internal class MinesServer
    {
        public static MinesServer Instance;
        public Events.Mines1Event OnConnect;
        public Events.Mines1Event OnLogin;
        public Events.Mines1Event OnLogout;
        public Events.Mines1Event OnMessage;

        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool Connected { get => socket.Connected; }

        private readonly Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        private readonly List<byte[]> pendingReadBytes = new();
        private readonly ByteArray pendingBytes = new();
        private Message pendingMessage;
        private bool readingMsg;

        private readonly Thread msgProcessingThread;
        private readonly List<Message> pendingProcessingMessages = new();

        public MinesServer()
        {
            if (Instance == null)
                Instance = this;

            pendingBytes.RemoveOnRead = true;
            msgProcessingThread = new Thread(ProcessMessage);

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
                    Host = host;
                    Port = port;
                    OnConnect?.Invoke(new MinesEvent(true, null, null));

                    msgProcessingThread.Start();
                    ReadLoop();
                    return;
                }

                OnConnect?.Invoke(new MinesEvent(false, "0", null));
            }
            catch (Exception e)
            {
                Logger.Error($"Error while connecting to {host}:{port} : {e.Message}");
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
            Logger.Instance.Debug($"Sending Mobject {mobj.ToString().Replace("\n", ",")}", Name);

            string result = "";
            byte[] b = new byte[mos.Bytes.Count];
            for (int i = 0; i < mos.Bytes.Count; i++)
            {
                result += mos.Bytes[i].ToString() + ";";
                b[i] = mos.Bytes[i];
            }

            Logger.Instance.Debug($"Bytes: {result}", Name);
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

            Send(bytesToSend);
        }

        /// <summary>
        /// Sends the byte array to the server.
        /// </summary>
        /// <param name="bytesToSend"></param>
        public void Send(byte[] bytesToSend)
        {
            try
            {
                socket.BeginSend(bytesToSend, 0, bytesToSend.Length, 0, new AsyncCallback(SendCallback), null);
            }
            catch (SocketException e)
            {
                Logger.Error(e.ToString());
            }
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
                    int available = socket.Available;
                    byte[] buffer = new byte[available];

                    socket.BeginReceive(buffer, 0, available, 0, new AsyncCallback(ReceiveCallback), buffer);
                }

                await Task.Delay(50);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            int bytesRead = socket.EndReceive(ar);

            if (bytesRead > 0)
            {
#if NetworkDebugVerbose
                Logger.Instance.Debug($"{bytesRead} bytes read", Name);
#endif

                byte[] buffer = (byte[])ar.AsyncState;
                pendingReadBytes.Add(buffer);

                if (!readingMsg)
                    HandleSocketData();
            }
            else
                Logger.Fatal("ReceiveCallback: bytesRead is 0 !!!");
        }

        private void HandleSocketData()
        {
            readingMsg = true;

            for (int i = 0; i < pendingReadBytes.Count; i++)
            {
                pendingBytes.WriteBytes(pendingReadBytes[i], 0, pendingReadBytes[i].Length);
                pendingReadBytes.RemoveAt(i);
                i--;
            }

            if (pendingMessage == null)
            {
                int header = pendingBytes.ReadByte();
                if (header != Message.HEADER_TYPE)
                {
                    Logger.Fatal($"Unknown Header {(char)header} [{header}]");
                    readingMsg = false;
                    return;
                }

                pendingMessage = new Message();
            }

            if (pendingMessage.NeedsPayload)
            {
                if (pendingBytes.Bytes.Count < 4)
                {
                    readingMsg = false;
                    return;
                }

                pendingMessage.SetPayload(pendingBytes.ReadInt());
            }

            pendingMessage.Read(pendingBytes);
            if (pendingMessage.IsComplete())
            {
#if NetworkDebug
                Logger.Debug($"Msg parsed ({pendingMessage.Payload})");
#endif

                pendingProcessingMessages.Add(pendingMessage);
                pendingMessage = null;

                if (pendingBytes.Bytes.Count > 0)
                {
#if NetworkDebugVerbose
                    Logger.Instance.Debug($"Parsing extra {pendingBytes.Bytes.Count} bytes", Name);
#endif
                    HandleSocketData();
                    return;
                }
            }
#if NetworkDebug
            else
                Logger.Instance.Debug($"Msg isn't complete, waiting for more bytes ({pendingMessage.Length}/{pendingMessage.Payload})", Name);
#endif

            readingMsg = false;
        }

        private void ProcessMessage()
        {
            while (true)
            {
                if (pendingProcessingMessages.Count == 0)
                {
                    Thread.Sleep(70);
                    continue;
                }

                Mobject mObj = pendingProcessingMessages[0].ToMobject();
                switch (mObj.Strings["type"])
                {
                    case "ping":
                        break;
                    case "data":
                        bool success = true;

                        Mobject m = mObj.Mobjects["mobject"];
                        if (m.Booleans.ContainsKey("success"))
                            success = m.Booleans["success"];

                        OnMessage?.Invoke(new MinesEvent(success, null, m));
                        break;
                    case "login":
                        OnLogin?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings.ContainsKey("errorCode") ? mObj.Strings["errorCode"] : "<empty>", mObj.Mobjects["mobject"]));
                        break;
                    case "logout":
                        OnLogout?.Invoke(new MinesEvent(mObj.Booleans["result"], mObj.Strings.ContainsKey("errorCode") ? mObj.Strings["errorCode"] : "<empty>", mObj.Mobjects["mobject"]));
                        Logger.Debug("Logout!!!", true);
                        break;
                    default:
                        Logger.Error($"Unexpected Message Type \"{mObj.Strings["type"]}\"");
                        break;
                }

                pendingProcessingMessages.RemoveAt(0);
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
            mobj.Strings["size"] = "5371953";
            mobj.Strings["hash"] = hash;
            mobj.Strings["type"] = "login";
            mobj.Strings["check"] = "haha";
            mobj.Strings["username"] = username;

            Send(mobj);
        }
    }
}
