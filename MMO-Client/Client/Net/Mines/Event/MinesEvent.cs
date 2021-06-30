using MMO_Client.Client.Net.Mines.Mobjects;

namespace MMO_Client.Client.Net.Mines.Event
{
    internal class MinesEvent
    {
        public bool Success { get; init; }
        public string ErrorCode { get; init; }
        public Mobject Mobject { get; init; }

        public MinesEvent(bool success, string errorCode, Mobject mObj)
        {
            Success = success;
            ErrorCode = errorCode;
            Mobject = mObj;
        }
    }
}
