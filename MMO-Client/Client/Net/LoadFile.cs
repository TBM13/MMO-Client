using System.Net.Http;

namespace MMO_Client.Client.Net
{
    internal class LoadFile
    {
        public Events.String1Event OnFileLoaded { get; set; }
        public Events.String1Event OnError { get; set; }

        private readonly HttpClient client = new();

        public async void Load(string url)
        {
            string data;
            try
            {
                data = await client.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                Logger.Error($"Couldn't download file '{url}' : {e.Message}");
                OnError?.Invoke(e.ToString());
                return;
            }

            OnFileLoaded?.Invoke(data);
        }
    }
}
