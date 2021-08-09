using MMO_Client.Client.Net.Mines;

namespace MMO_Client.Client.Net.Requests
{
    internal class SecureMamboRequest : MamboRequest
    {
        public SecureMamboRequest(string type) : base(type) { }

        public static void ApplyValidationDigest(Mobject mobj)
        {
            string[] digest = NetworkManager.Instance.SecurityMethod.CreateValidationDigest(NetworkManager.Instance.SecurityMethod.SecurityRequestKey);
            mobj.Strings["digestNum"] = digest[0];
            mobj.Strings["digestHash"] = digest[1];
        }
    }
}
