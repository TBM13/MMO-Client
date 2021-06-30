using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMO_Client.Common;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Mines.Mobjects;

namespace MMO_Client.Client.Net
{
    class NetworkManager
    {
        public static NetworkManager Instance;
        private const string title = "Network Manager";

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

        private readonly MinesServer mines;

        public NetworkManager()
        {
            Instance = this;

            mines = new();
            mines.OnConnect += (MinesEvent ev) => OnConnect?.Invoke(ev);
            mines.OnLogin += (MinesEvent ev) => OnLogin?.Invoke(ev);
            mines.OnLogout += (MinesEvent ev) => OnLogout?.Invoke(ev);
            mines.OnMessage += HandleMamboEvent;

            Logger.Info("Network Manager Created", title);

#if NetworkDebug
            Logger.Debug("Network Debug is enabled", title);
#endif
        }

        public void Connect(string host, int port) =>
            mines.Connect(host, port);

        public void SendMobject(Mobject mObj)
        {
#if NetworkDebug
            Logger.Debug($"Sending Action {mObj.Strings["request"]}", title);
#endif

            OnActionSent?.Invoke(new MinesEvent(true, "<empty>", mObj));
            mines.SendMobject(mObj);
        }

        public void MinesSend(Mobject mObj) => 
            mines.Send(mObj);

        private void HandleMamboEvent(MinesEvent mEvent)
        {
            string type = mEvent.Mobject.Strings["type"];

            if (!mEvent.Success)
                Logger.Warn($"Received a failing \"{type}\". Error Code: {mEvent.ErrorCode}", title);
#if NetworkDebug
            else
                Logger.Debug($"Received a successful \"{type}\" ({((bool)(mEvent.Mobject?.Strings.ContainsKey("messageId")) ? mEvent.Mobject.Strings["messageId"] : "no Mobject")})", title);
#endif

            switch(type)
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
                case "UniqueActionSent":
                    OnUniqueActionSent?.Invoke(mEvent);
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
                    Logger.Error($"Unhandled Mambo Event {type}", title);
                    break;
            }
        }
    }
}
