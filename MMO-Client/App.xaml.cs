using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MMO_Client.Common;
using MMO_Client.Screens;
using MMO_Client.Client.Assets;
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
            _ = new AssetsManager();

            _ = new MinesManager();

            if (!MinesManager.Instance.Connect("juegosg1395.mundogaturro.com", 9899))
            {
                Logger.Error("Couldn't connect to host.", "Main");
                return;
            }

            Logger.Info($"Connected", "Main");
        }
    }
}
