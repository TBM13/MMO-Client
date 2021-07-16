using MMO_Client.Client.Net.Mines.Mobjects;

namespace MMO_Client.Client.Net.Requests
{
    class MamboRequest
    {
        public string Type { get; set; }

        public MamboRequest(string type = null) => 
            Type = type;

        public Mobject ToMobject()
        {
            Mobject mObj = new();
            mObj = Build(mObj);

            return mObj;
        }

        public virtual Mobject Build(Mobject mObj) => 
            mObj;
    }
}
