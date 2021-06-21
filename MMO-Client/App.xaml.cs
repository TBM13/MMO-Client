using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MMO_Client.Common.Logger;
using MMO_Client.Screens.LoginScreen;

namespace MMO_Client
{
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs args) => 
            Setup();

        private void Setup()
        {
            new Logger();

            Logger.Info("Loading Login Screen", "Main");
            LoginScreen loginScreen = new();
            loginScreen.OnLoginEvent += InitiateLogin;
            loginScreen.Show();
        }

        private void InitiateLogin(string user, string pass)
        {

        }
    }
}
