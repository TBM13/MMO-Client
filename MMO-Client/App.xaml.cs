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
using MMO_Client.Client.Net.Mines.Mobjects;
using MMO_Client.Client.Net.Mines.Event;

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

            Mobject mobj = new();
            mobj.Strings["size"] = "5370132";
            mobj.Strings["hash"] = "V8euWoGRbNXaDR1qFK9jIxf4L6618wOUS1aDqUY6ZvYYs283io6fPvj73q8z4h16";
            mobj.Strings["type"] = "login";
            mobj.Strings["check"] = "haha";
            mobj.Strings["username"] = "TEST001";
            MinesManager.Instance.Send(mobj);
        }
    }
}
