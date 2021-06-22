using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MMO_Client.Client.Net;
using MMO_Client.Common;
using MMO_Client.Screens;
using MMO_Client.Client.Assets;

namespace MMO_Client
{
    public partial class App : Application
    {
        private NetworkManager Net = new();
        private LoginService LoginManager = new();

        void App_Startup(object sender, StartupEventArgs args) =>
            Setup();

        private void Setup()
        {
            _ = new Logger();
            _ = new AssetsManager();

            Logger.Info("Loading Login Screen", "Main");
            LoginScreen loginScreen = new();
            loginScreen.OnLoginEvent += InitiateLogin;
            loginScreen.Show();

            InitEvents();
        }

        private void InitEvents()
        {
            LoginManager.OnLoginSuccess += SetupGameScreen;
            //LoginManager.OnLoginFail += ; //Put "invalid credentials" in LoginScreen
        }

        private void SetupGameScreen(string token)
        {
            //Open GameScreen
            Net.Connect("host", 123);
        }
        private void InitiateLogin(string username, string password)
        {
            LoginManager.LoginByUserPass(username, password);
        }
    }
}
