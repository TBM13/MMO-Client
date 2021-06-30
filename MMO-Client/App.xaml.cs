using System.Windows;
using MMO_Client.Common;
using MMO_Client.Screens;
using MMO_Client.Client.Assets;
using MMO_Client.Client.Net;
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Mines.Mobjects;

namespace MMO_Client
{
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs args) =>
            Setup();

        private void Setup()
        {
            _ = new Logger();
            _ = new AssetsManager();
            _ = new NetworkManager();
            _ = new GameScreen();

            NetworkManager.Instance.OnConnect += OnConnect;
            NetworkManager.Instance.Connect("juegosg1395.mundogaturro.com", 9899);
        }

        private void OnConnect(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            Logger.Info($"Connected", "Main");

            Mobject mobj = new();
            mobj.Strings["size"] = "5370589";
            mobj.Strings["hash"] = "V144XOejzkg2hK0W5sm02Emq3y9lTU60WGAb048TU4vVQlVE9E224xwGTnO55Igq";
            mobj.Strings["type"] = "login";
            mobj.Strings["check"] = "haha";
            mobj.Strings["username"] = "TEST001";
            NetworkManager.Instance.MinesSend(mobj);
        }
    }
}
