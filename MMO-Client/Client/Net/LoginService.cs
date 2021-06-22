using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMO_Client.Common;
using MMO_Client.Client.Net.Api;

namespace MMO_Client.Client.Net
{
    class LoginService
    {
        public event Events.String1Event OnLoginSuccess;
        public event Events.Event OnLoginFail;
        public void LoginByUserPass(string username, string password)
        {
            Dictionary<string, string> Params = new Dictionary<string, string>()
            {
                {"username", username},
                {"password", password}
            };
            JToken loginData = ClientApi.RequestApi("login", Params);
            if (loginData.ToString() == "null")
            {
                OnLoginFail?.Invoke();
                return;
            }
            string Token = loginData["user"]["token"].ToString();
            Logger.Info("Login in as " + username + " with token: " + Token);
            OnLoginSuccess?.Invoke(Token);
        }
    }
}