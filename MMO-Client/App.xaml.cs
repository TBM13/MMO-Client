using System.Windows;
using MMO_Client.Common;
using MMO_Client.Screens;
using MMO_Client.Client.Assets;
using MMO_Client.Client.Net;

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
        }
    }
}
