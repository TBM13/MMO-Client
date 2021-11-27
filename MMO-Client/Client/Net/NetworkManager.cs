using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.Net.Requests;
using MMO_Client.Client.Net.Security;

namespace MMO_Client.Client.Net
{
    internal partial class NetworkManager
    {
        public static NetworkManager Instance;

        public SecurityMethod SecurityMethod { get; private set; }

        private string preff;
        private int counter;

        public NetworkManager()
        {
            Instance = this;

            MinesServer.Instance.OnLogin += OnMineslogin;
            MinesServer.Instance.OnMessage += HandleMamboEvent;

#if NetworkDebug
            Logger.Debug("Network Debug is enabled");
#endif
#if NetworkDebugVerbose
            Logger.Debug("Verbose Network Debug is enabled", Name);
#endif
        }

        private void OnMineslogin(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            MinesServer.Instance.OnLogin -= OnMineslogin;
            SecurityMethod = new(mEvent.Mobject.Strings["key"]);

            Logger.Info("Server confirmed successful login");
        }

        public void SendAction(MamboRequest request)
        {
            Mobject mobj = request.ToMobject();
            if (mobj == null)
            {
                Logger.Warn("Can't send a null action");
                return;
            }

#if NetworkDebug
            Logger.Debug($"Sending action {request.Type}");
#endif

            mobj.Strings["request"] = request.Type;
            mobj.Strings["messageId"] = GenerateID();
            SendMobject(mobj);
        }

        public void SendMobject(Mobject mobj) => 
            MinesServer.Instance.SendMobject(mobj);

        private string GenerateID()
        {
            preff ??= Utils.GetUnixTime().ToString() + "_";
            return preff + counter++.ToString();
        }
    }
}
