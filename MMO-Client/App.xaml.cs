using System;
using System.Windows;
using System.Windows.Threading;
using MMO_Client.Screens;
using MMO_Client.Client.Assets;
using MMO_Client.Client.Net;
using MMO_Client.Client.Net.Mines.Event;
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.World.Rooms;
using MMO_Client.Client.Net.Mines;

namespace MMO_Client
{
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs args) => 
            Setup();

        private void Setup()
        {
            _ = new Logger();
        }
    }
}
