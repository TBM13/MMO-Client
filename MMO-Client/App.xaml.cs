﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MMO_Client.Client.Net.LoginService;
using MMO_Client.Client.Net.NetworkManager;
using MMO_Client.Common.Logger;
using MMO_Client.Screens.LoginScreen;

namespace MMO_Client
{
    public partial class App : Application
    {
        private NetworkManager Net = new NetworkManager();
        private LoginService LoginManager = new LoginService();
        void App_Startup(object sender, StartupEventArgs args) =>
            Setup();

        private void Setup()
        {
            new Logger();

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
