using System;
using System.Windows;
using System.Windows.Threading;
using MMO_Client.Common;
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
        private readonly Module[] criticalModules = new Module[]
        {
            new AssetsManager(),
            new MinesServer(),
            new NetworkManager()
        };

        private readonly Module[] restartableModules = new Module[]
        {
            new RoomManager()
        };

        void App_Startup(object sender, StartupEventArgs args)
        {
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;

            Setup();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            bool isCritical = false;
            string className = e.Exception.TargetSite.ReflectedType.Name;
            foreach (Module module in criticalModules)
            {
                if (module.GetType().Name == className)
                {
                    isCritical = true;
                    break;
                }
            }

            if (!isCritical)
                Logger.Error("Unhandled Exception: " + e.Exception.Message, className);
            else
                Logger.Fatal("Unhandled Exception: " + e.Exception.Message, className);

            Logger.Debug(e.Exception.StackTrace, className);
            e.Handled = true;
        }

        private void Setup()
        {
            _ = new Logger();

            foreach (Module module in criticalModules)
                module.Initialize();

            LoginScreen loginScreen = new();
            loginScreen.Show();
            loginScreen.OnLoginIdReceivedEvent += LoginScreen_OnLoginReceivedEvent;
        }

        string username;
        string loginHash;

        private void LoginScreen_OnLoginReceivedEvent(string username, string hash)
        {
            this.username = username;
            loginHash = hash;

            NetworkManager.Instance.OnConnect += OnConnect;
            NetworkManager.Instance.Connect("juegosg1395.mundogaturro.com", 9899);
        }

        private void OnConnect(MinesEvent mEvent)
        {
            if (!mEvent.Success)
                return;

            Logger.Info($"Connected", "Main");

            foreach (Module module in restartableModules)
                module.Initialize();

            _ = new GameScreen();
            NetworkManager.Instance.LoginWithID(username, loginHash);
        }
    }
}
