using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMO_Client.Common;
using MMO_Client.Client.Net;
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Requests.Room;

namespace MMO_Client.Client.World.Rooms
{
    class RoomManager
    {
        public static RoomManager Instance;
        private const string title = "Room Manager";

        public RoomManager()
        {
            Instance = this;
            Logger.Info("Room Manager Created", title);

            NetworkManager.Instance.OnRoomData += OnRoomData;
            NetworkManager.Instance.OnAvatarJoins += OnAvatarJoins;
        }

        public void LoadRoomData() => 
            NetworkManager.Instance.SendAction(new RoomDataRequest());

        private void OnRoomData(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            Logger.Debug(mEvent.ToString(), "");
        }

        private void OnAvatarJoins(MinesEvent mEvent)
        {
            NetworkManager.Instance.OnAvatarJoins -= OnAvatarJoins;
            LoadRoomData();
        }
    }
}
