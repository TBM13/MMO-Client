using MMO_Client.Common.Logger;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO_Client.Client.Net.Api.ClientApi
{
    class ClientApi
    {
        static readonly RestClient client = new RestClient("https://api.mmo-client.ml");
        
        private static void HandleError(JObject data)
        {
            string ErrorStr = data.GetValue("error").ToString();
            if (ErrorStr != "null") 
            {
                Logger.Error(ErrorStr, "Api-Response");
            }
        }
        public static JToken RequestApi(string type, Dictionary<string, string> Params)
        {
            var request = new RestRequest(type);
            foreach (KeyValuePair<string, string> entry in Params)
            {
                request.AddParameter(entry.Key, entry.Value);
            }
            request.AddHeader("ClientVersion", "0.1");
            string rStr = client.Post(request).Content;

            JObject r = JObject.Parse(rStr);

            HandleError(r);
            return r["response"];
        }
    }
}
