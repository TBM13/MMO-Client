using System;
using MMO_Client.Common;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.Net.Requests;

namespace MMO_Client.Client.Net
{
    internal class NetworkManager : Module
    {
        public static NetworkManager Instance;

        #region events
        public Events.Mines1Event OnConnect;
        public Events.Mines1Event OnLogin;
        public Events.Mines1Event OnLogout;

        public Events.Mines1Event OnBuddyAdded;
        public Events.Mines1Event OnBuddyRemoved;
        public Events.Mines1Event OnBuddyBlocked;
        public Events.Mines1Event OnBuddyUnblocked;
        public Events.Mines1Event OnBuddyStatusChanged;

        public Events.Mines1Event OnActionSent;
        public Events.Mines1Event OnUniqueActionSent;

        public Events.Mines1Event OnAvatarJoins;
        public Events.Mines1Event OnAvatarLeft;
        public Events.Mines1Event OnAvatarMove;
        public Events.Mines1Event OnAvatarData;
        public Events.Mines1Event OnAvatarsWhoAddedMeData;

        public Events.Mines1Event OnObjectLeft;
        public Events.Mines1Event OnObjectJoins;
        public Events.Mines1Event OnSceneObjectsData;

        public Events.Mines1Event OnChangeRoom;
        public Events.Mines1Event OnRoomData;

        public Events.Mines1Event OnMailRead;
        public Events.Mines1Event OnMailSent;
        public Events.Mines1Event OnMailDeleted;
        public Events.Mines1Event OnMailReceived;
        public Events.Mines1Event OnMailData;

        public Events.Mines1Event OnUserAddedToQueue;
        public Events.Mines1Event OnUserRemovedFromQueue;
        public Events.Mines1Event OnQueueCreated;
        public Events.Mines1Event OnQueueReady;
        public Events.Mines1Event OnQueueFailed;

        public Events.Mines1Event OnPlayerWarned;
        public Events.Mines1Event OnPlayerSuspended;

        public Events.Mines1Event OnAddedToInventory;
        public Events.Mines1Event OnRemovedFromInventory;
        public Events.Mines1Event OnInventoryData;

        public Events.Mines1Event OnWorldDataChanged;

        public Events.Mines1Event OnMessageReceived;

        public Events.Mines1Event OnCustomAttributesChanged;

        public Events.Mines1Event OnServerData;
        public Events.Mines1Event OnServerMessage;

        public Events.Mines1Event OnWhitelistData;
        #endregion

        private string preff;
        private int counter = 0;

        public override string Name { get; } = "Network Manager";

        public override void Initialize()
        {
            Instance = this;

            MinesServer.Instance.OnConnect += (MinesEvent ev) => OnConnect?.Invoke(ev);
            MinesServer.Instance.OnLogin += (MinesEvent ev) => OnLogin?.Invoke(ev);
            MinesServer.Instance.OnLogout += (MinesEvent ev) => OnLogout?.Invoke(ev);
            MinesServer.Instance.OnMessage += HandleMamboEvent;

            Logger.Info("Initialized", Name);

#if NetworkDebug
            Logger.Debug("Network Debug is enabled", Name);
#endif
#if NetworkDebugVerbose
            Logger.Debug("Verbose Network Debug is enabled", Name);
#endif
        }

        public override void Terminate() => 
            Logger.Info("Terminated", Name);

        public void Connect(string host, int port) =>
            MinesServer.Instance.Connect(host, port);

        public void SendMobject(Mobject mObj)
        {
#if NetworkDebug
            Logger.Debug($"Sending Action {mObj.Strings["request"]}", Name);
#endif

            OnActionSent?.Invoke(new MinesEvent(true, "<empty>", mObj));
            MinesServer.Instance.SendMobject(mObj);
        }

        public void SendAction(MamboRequest request)
        {
            Mobject mObj = request.ToMobject();
            mObj.Strings["request"] = request.Type;
            mObj.Strings["messageId"] = GenerateID();
            SendMobject(mObj);

            OnUniqueActionSent?.Invoke(new MinesEvent(true, "<empty>", mObj));
        }

        public void LoginWithID(string username, string hash) =>
            MinesServer.Instance.LoginWithID(username, hash);

        private string GenerateID()
        {
            preff ??= DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + "_";
            return preff + counter++.ToString();
        }

        private void HandleMamboEvent(MinesEvent mEvent)
        {
            string type = mEvent.Mobject.Strings["type"];

            if (!mEvent.Success)
                Logger.Warn($"Received a failing \"{type}\". Error Code: {mEvent.ErrorCode}", Name);
#if NetworkDebug
            else
                Logger.Debug($"Received a successful \"{type}\" ({((bool)(mEvent.Mobject?.Strings.ContainsKey("messageId")) ? mEvent.Mobject.Strings["messageId"] : "no Mobject")})", Name);
#endif

            switch (type)
            {
                case "BuddyStatusChanged":
                    OnBuddyStatusChanged?.Invoke(mEvent);
                    break;
                case "AvatarJoinsRoom":
                    OnAvatarJoins?.Invoke(mEvent);
                    break;
                case "ObjectLeavesRoom":
                    OnObjectLeft?.Invoke(mEvent);
                    break;
                case "SceneObjectsListDataResponse":
                    OnSceneObjectsData?.Invoke(mEvent);
                    break;
                case "ChangeRoomActionResponse":
                    OnChangeRoom?.Invoke(mEvent);
                    break;
                case "RoomDataResponse":
                    OnRoomData?.Invoke(mEvent);
                    break;
                case "MarkMailAsReadActionResponse":
                    OnMailRead?.Invoke(mEvent);
                    break;
                case "AddBuddyActionResponse":
                    OnBuddyAdded?.Invoke(mEvent);
                    break;
                case "MailMessageActionResponse":
                    OnMailSent?.Invoke(mEvent);
                    break;
                case "WorldDataChanged":
                    OnWorldDataChanged?.Invoke(mEvent);
                    break;
                case "AvatarLeftRoom":
                    OnAvatarLeft?.Invoke(mEvent);
                    break;
                case "RemoveBuddyActionResponse":
                    OnBuddyRemoved?.Invoke(mEvent);
                    break;
                case "UserJoinsToQueue":
                    OnUserAddedToQueue?.Invoke(mEvent);
                    break;
                case "MailsDataResponse":
                    OnMailData?.Invoke(mEvent);
                    break;
                case "BlockBuddyActionResponse":
                    OnBuddyBlocked?.Invoke(mEvent);
                    break;
                case "ServerDataResponse":
                    OnServerData?.Invoke(mEvent);
                    break;
                case "MessageReceived":
                    OnMessageReceived?.Invoke(mEvent);
                    break;
                case "CustomAttributesChanged":
                    OnCustomAttributesChanged?.Invoke(mEvent);
                    break;
                case "WarnPlayer":
                    OnPlayerWarned?.Invoke(mEvent);
                    break;
                case "GameLaunched":
                    OnQueueReady?.Invoke(mEvent);
                    break;
                case "RemovedFromInventory":
                    OnRemovedFromInventory?.Invoke(mEvent);
                    break;
                case "GameLaunchFailed":
                    OnQueueFailed?.Invoke(mEvent);
                    break;
                case "AvatarsWhoAddedMeDataResponse":
                    OnAvatarsWhoAddedMeData?.Invoke(mEvent);
                    break;
                case "InventoryDataResponse":
                    OnInventoryData?.Invoke(mEvent);
                    break;
                case "AvatarDataResponse":
                    OnAvatarData?.Invoke(mEvent);
                    break;
                case "UnblockBuddyActionResponse":
                    OnBuddyUnblocked?.Invoke(mEvent);
                    break;
                case "SuspendPlayer":
                    OnPlayerSuspended?.Invoke(mEvent);
                    break;
                case "ObjectJoinsRoom":
                    OnObjectJoins?.Invoke(mEvent);
                    break;
                case "ServerMessage":
                    OnServerMessage?.Invoke(mEvent);
                    break;
                case "UserLeavesFromQueue":
                    OnUserRemovedFromQueue?.Invoke(mEvent);
                    break;
                case "MailReceived":
                    OnMailReceived?.Invoke(mEvent);
                    break;
                case "DeleteMailActionResponse":
                    OnMailDeleted?.Invoke(mEvent);
                    break;
                case "WhiteListDataResponse":
                    OnWhitelistData?.Invoke(mEvent);
                    break;
                case "MoveActionResponse":
                    OnAvatarMove?.Invoke(mEvent);
                    break;
                case "JoinQueueActionResponse":
                    OnQueueCreated?.Invoke(mEvent);
                    break;
                case "AddedToInventory":
                    OnAddedToInventory?.Invoke(mEvent);
                    break;
                default:
                    Logger.Error($"Unhandled Mambo Event {type}", Name);
                    break;
            }
        }
    }
}
