using System;
using System.Windows;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.Net.Requests;
using MMO_Client.Client.Net.Security;

namespace MMO_Client.Client.Net
{
    internal class NetworkManager
    {
        public static NetworkManager Instance;

        public SecurityMethod SecurityMethod { get; private set; }

        #region events
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
        private int counter;

        public NetworkManager()
        {
            Instance = this;

            MinesServer.Instance.OnLogin += OnMineslogin;
            MinesServer.Instance.OnMessage += InvokeHandleMamboEvent;

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
            Logger.Debug($"Sending action {mobj.Strings["request"]}");
#endif

            mobj.Strings["request"] = request.Type;
            mobj.Strings["messageId"] = GenerateID();
            SendMobject(mobj);
        }

        public void SendMobject(Mobject mobj)
        {
            MinesServer.Instance.SendMobject(mobj);
        }

        private string GenerateID()
        {
            preff ??= Utils.GetUnixTime().ToString() + "_";
            return preff + counter++.ToString();
        }

        // We want to execute HandleMamboEvent in the main thread
        private void InvokeHandleMamboEvent(MinesEvent mEvent) =>
            Application.Current.Dispatcher.Invoke(new Action(() => { HandleMamboEvent(mEvent); }));

        private void HandleMamboEvent(MinesEvent mEvent)
        {
            Mobject mobj = mEvent.Mobject;
            string type = mobj.Strings["type"];

            if (!mEvent.Success)
            {
                string errorMsg = "";
                if (mobj.Strings.ContainsKey("errorMessage"))
                    errorMsg = mobj.Strings["errorMessage"];
                else
                    Logger.Debug("Does the mobject contain any error code?");

                Logger.Warn($"Received a failing {type} : {errorMsg}");
                return;
            }
#if NetworkDebug
            else
                Logger.Debug($"Received a successful \"{type}\" ({((bool)(mEvent.Mobject?.Strings.ContainsKey("messageId")) ? mEvent.Mobject.Strings["messageId"] : "no Mobject")})");
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
                    Logger.Error($"Unhandled Mambo Event {type}");
                    break;
            }
        }
    }
}
