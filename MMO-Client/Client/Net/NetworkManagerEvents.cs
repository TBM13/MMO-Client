using MMO_Client.Client.Net.Mines;

namespace MMO_Client.Client.Net
{
    internal partial class NetworkManager
    {
        public Events.Mines1Event OnBuddyAdded { get; set; }
        public Events.Mines1Event OnBuddyRemoved { get; set; }
        public Events.Mines1Event OnBuddyBlocked { get; set; }
        public Events.Mines1Event OnBuddyUnblocked { get; set; }
        public Events.Mines1Event OnBuddyStatusChanged { get; set; }

        public Events.Mines1Event OnActionSent { get; set; }
        public Events.Mines1Event OnUniqueActionSent { get; set; }

        public Events.Mines1Event OnAvatarJoins { get; set; }
        public Events.Mines1Event OnAvatarLeft { get; set; }
        public Events.Mines1Event OnAvatarMove { get; set; }
        public Events.Mines1Event OnAvatarData { get; set; }
        public Events.Mines1Event OnAvatarsWhoAddedMeData { get; set; }

        public Events.Mines1Event OnObjectLeft { get; set; }
        public Events.Mines1Event OnObjectJoins { get; set; }
        public Events.Mines1Event OnSceneObjectsData { get; set; }

        public Events.Mines1Event OnChangeRoom { get; set; }
        public Events.Mines1Event OnRoomData { get; set; }

        public Events.Mines1Event OnMailRead { get; set; }
        public Events.Mines1Event OnMailSent { get; set; }
        public Events.Mines1Event OnMailDeleted { get; set; }
        public Events.Mines1Event OnMailReceived { get; set; }
        public Events.Mines1Event OnMailData { get; set; }

        public Events.Mines1Event OnUserAddedToQueue { get; set; }
        public Events.Mines1Event OnUserRemovedFromQueue { get; set; }
        public Events.Mines1Event OnQueueCreated { get; set; }
        public Events.Mines1Event OnQueueReady { get; set; }
        public Events.Mines1Event OnQueueFailed { get; set; }

        public Events.Mines1Event OnPlayerWarned { get; set; }
        public Events.Mines1Event OnPlayerSuspended { get; set; }

        public Events.Mines1Event OnAddedToInventory { get; set; }
        public Events.Mines1Event OnRemovedFromInventory { get; set; }
        public Events.Mines1Event OnInventoryData { get; set; }

        public Events.Mines1Event OnWorldDataChanged { get; set; }

        public Events.Mines1Event OnMessageReceived { get; set; }

        public Events.Mines1Event OnCustomAttributesChanged { get; set; }

        public Events.Mines1Event OnServerData { get; set; }
        public Events.Mines1Event OnServerMessage { get; set; }

        public Events.Mines1Event OnWhitelistData { get; set; }

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
