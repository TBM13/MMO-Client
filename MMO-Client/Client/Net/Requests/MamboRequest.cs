using MMO_Client.Client.Net.Mines;

namespace MMO_Client.Client.Net.Requests
{
    internal class MamboRequest
    {
        public string Type { get; private set; }

        public MamboRequest(string type) =>
            Type = type;

        public Mobject ToMobject() =>
            Build();

        public virtual Mobject Build()
        {
            Logger.Error("Not implemented!", false, Type);
            return null;
        }
    }
}
