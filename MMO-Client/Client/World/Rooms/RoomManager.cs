using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMO_Client.Client.Net;
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Requests.Room;

namespace MMO_Client.Client.World.Rooms
{
    class RoomManager
    {
        public static RoomManager Instance;
        public Events.Event OnRoomCreated;

        public RoomManager()
        {
            Instance = this;

            NetworkManager.Instance.OnRoomData += OnRoomData;
            NetworkManager.Instance.OnAvatarJoins += OnAvatarJoins;

            Logger.Info("Initialized");
        }

        public void LoadRoomData() => 
            NetworkManager.Instance.SendAction(new RoomDataRequest());

        private void OnAvatarJoins(MinesEvent mEvent)
        {
            NetworkManager.Instance.OnAvatarJoins -= OnAvatarJoins;
            LoadRoomData();
        }

        private void OnRoomData(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            _ = new Room(mEvent.Mobject);
            OnRoomCreated?.Invoke();
        }
    }
}
