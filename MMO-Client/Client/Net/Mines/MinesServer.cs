using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading.Tasks;
using MMO_Client.Client.Net.Mines.IO;

namespace MMO_Client.Client.Net.Mines
{
    internal class MinesServer
    {
        public static MinesServer Instance { get; private set; }

        public Events.Mines1Event OnConnect { get; set; }
        public Events.Mines1Event OnLogin { get; set; }
        public Events.Mines1Event OnLogout { get; set; }
        public Events.Mines1Event OnMessage { get; set; }

        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool Connected => socket.Connected;

        private readonly Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        private readonly ByteArray pendingBytes = new() { RemoveOnRead = true };
        private readonly Message pendingMessage = new();
        private bool readingMsg, workingOnMsg;

        public MinesServer()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Connects the socket to the specified host and starts the read loop if the connection was successful.
        /// </summary>
        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                Logger.Info($"Connecting to {host}:{port}");
                await socket.ConnectAsync(host, port);

                if (socket.Connected)
                {
                    Logger.Info("Connected!");

                    Host = host;
                    Port = port;
                    OnConnect?.Invoke(new MinesEvent(true, null, null));

                    _ = ReadLoopAsync();
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

            int length = mos.Bytes.Count + 5;

            byte[] bytesToSend = ArrayPool<byte>.Shared.Rent(length);
            bytesToSend[0] = Message.HEADER_TYPE; // Write header type

            BinaryPrimitives.WriteInt32BigEndian(new Span<byte>(bytesToSend, 1, 4), mos.Bytes.Count); // Write Length

            for (int i = 5; i < length; i++) // Write bytes
                bytesToSend[i] = mos.Bytes[i - 5];

            Send(bytesToSend, length);
        }

        /// <summary>
        /// Sends the byte array to the server.
        /// </summary>
        private void Send(byte[] bytesToSend, int size)
        {
            try
            {
                _ = socket.BeginSend(bytesToSend, 0, size, 0, new AsyncCallback(SendCallback), bytesToSend);
            }
            catch (SocketException e)
            {
                Logger.Error(e.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            socket.EndSend(ar);

            ArrayPool<byte>.Shared.Return((byte[])ar.AsyncState);
        }

        /// <summary>
        /// Continually checks if there is available data for reading, and reads it.
        /// </summary>
        private async Task ReadLoopAsync()
        {
            while (true)
            {
                if (socket.Available > 0)
                {
                    int available = socket.Available;
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(available);

                    _ = socket.BeginReceive(buffer, 0, available, 0, new AsyncCallback(ReceiveCallback), buffer);
                }

                await Task.Delay(50);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            int bytesRead = socket.EndReceive(ar);

#if NetworkDebugVerbose
            Logger.Debug($"{bytesRead} bytes read", Name);
#endif

            byte[] buffer = (byte[])ar.AsyncState;
            pendingBytes.WriteBytes(buffer, 0, bytesRead);

            ArrayPool<byte>.Shared.Return(buffer);

            if (!readingMsg)
                HandleSocketData();
        }

        private void HandleSocketData()
        {
            readingMsg = true;

            if (!workingOnMsg)
            {
                int header = pendingBytes.ReadByte();
                if (header != Message.HEADER_TYPE)
                {
                    Logger.Fatal($"Unknown Header {(char)header} [{header}]");
                    readingMsg = false;
                    return;
                }

                pendingMessage.Clear();
                workingOnMsg = true;
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
#if NetworkDebugVerbose
                Logger.Debug($"Msg parsed ({pendingMessage.Payload})");
#endif

                ProcessMessage(pendingMessage);
                workingOnMsg = false;

                if (pendingBytes.Bytes.Count > 0)
                {
#if NetworkDebugVerbose
                    Logger.Debug($"Parsing extra {pendingBytes.Bytes.Count} bytes", Name);
#endif
                    HandleSocketData();
                    return;
                }
            }
#if NetworkDebugVerbose
            else
                Logger.Debug($"Msg isn't complete, waiting for more bytes ({pendingMessage.Length}/{pendingMessage.Payload})");
#endif

            readingMsg = false;
        }

        private void ProcessMessage(Message msg)
        {
            Mobject mObj = msg.ToMobject();
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
            mobj.Strings["size"] = "14417855";
            mobj.Strings["hash"] = hash;
            mobj.Strings["type"] = "login";
            mobj.Strings["check"] = "haha";
            mobj.Strings["username"] = username;

            Send(mobj);
        }
    }
}
