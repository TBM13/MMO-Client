namespace MMO_Client.Client.Net.Requests.Room
{
    class RoomDataRequest : MamboRequest
    {
        public RoomDataRequest(string type = "RoomDataRequest") =>
            Type = type;
    }
}
