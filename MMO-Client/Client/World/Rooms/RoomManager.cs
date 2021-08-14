using MMO_Client.Client.Net;
using MMO_Client.Client.Net.Mines;
using MMO_Client.Client.Net.Requests.Room;
using MMO_Client.Screens;

namespace MMO_Client.Client.World.Rooms
{
    internal class RoomManager
    {
        public static RoomManager Instance;
        public Events.Event OnRoomCreated;

        public RoomManager()
        {
            Instance = this;

            NetworkManager.Instance.OnRoomData += OnRoomData;
            NetworkManager.Instance.OnChangeRoom += OnChangeRoom;
            NetworkManager.Instance.OnAvatarJoins += OnAvatarJoins;
        }

        public void LoadRoomData()
        {
            GameScreen.Instance.ShowLoadScreen("Requesting Room Data...");
            NetworkManager.Instance.SendAction(new RoomDataRequest());
        }

        public void ChangeRoom(RoomLink link)
        {
            GameScreen.Instance.ShowLoadScreen("Requesting Room Change...");
            NetworkManager.Instance.SendAction(new ChangeRoomActionRequest(link));
        }

        private void OnAvatarJoins(MinesEvent mEvent)
        {
            NetworkManager.Instance.OnAvatarJoins -= OnAvatarJoins;
            LoadRoomData();
        }

        private void OnRoomData(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            Room.CurrentRoom?.Dispose();

            Room room = new();
            room.BuildFromMobject(mEvent.Mobject);
            OnRoomCreated?.Invoke();

            GameScreen.Instance.HideLoadScreen();
        }

        private void OnChangeRoom(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            LoadRoomData();
        }
    }
}
