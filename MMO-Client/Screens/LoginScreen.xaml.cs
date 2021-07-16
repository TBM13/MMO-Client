using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using MMO_Client.Common;

namespace MMO_Client.Screens
{
    public partial class LoginScreen : Window
    {
        internal event Events.String2Event OnLoginIdReceivedEvent;

        private const string LogTitle = "Login Screen";

        public LoginScreen() => 
            InitializeComponent();

        private void PwdPasswordbox_PasswordChanged(object sender, RoutedEventArgs e) =>
            LoginButton.IsEnabled = UsernameTextbox.Text.Length >= 4 && PwdPasswordbox.Password.Length >= 4;

        private void UsernameTextbox_TextChanged(object sender, TextChangedEventArgs e) =>
            LoginButton.IsEnabled = UsernameTextbox.Text.Length >= 4 && PwdPasswordbox.Password.Length >= 4;

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            GetCsrfAndSessionToken();
        }

        private async void GetCsrfAndSessionToken()
        {
            Logger.Info("Attempting to get CSRF Token and Login Session cookies...", LogTitle);

            Uri loginUri = new("https://login.mundogaturro.com/");

            CookieContainer cookieContainer = new();
            using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            using HttpClient client = new(handler);

            HttpResponseMessage response = await client.GetAsync(loginUri);
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Unexpected status code {response.StatusCode}. Check your internet connection and try again", LogTitle);
                return;
            }

            string csrfToken = await response.Content.ReadAsStringAsync();
            csrfToken = csrfToken.Remove(0, csrfToken.IndexOf("csrf-token\" content=\"", StringComparison.InvariantCulture) + 21);
            csrfToken = csrfToken.Remove(csrfToken.IndexOf("\"", StringComparison.InvariantCulture));

            if (csrfToken.Length != 40)
            {
                Logger.Error($"We were expecting the CSRF Token to have 40 characters, but it has {csrfToken.Length}", LogTitle);
                Logger.Debug(csrfToken, LogTitle);
                return;
            }

            string loginSession = null;
            IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(loginUri);
            foreach (Cookie cookie in responseCookies)
            {
                if (cookie.Name == "login_session")
                {
                    loginSession = cookie.Value;
                    break;
                }
            }

            if (loginSession == null)
            {
                Logger.Error("Couldn't get Login Session cookie", LogTitle);
                return;
            }

            GetGaturroToken(UsernameTextbox.Text, PwdPasswordbox.Password, csrfToken, loginSession);
        }

        private async void GetGaturroToken(string username, string password, string csrfToken, string loginSession)
        {
            Logger.Info("Attempting to get gaturro token cookie...", LogTitle);

            Uri loginUri = new("http://desktop.mundogaturro.com/auth");

            CookieContainer cookieContainer = new();
            using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            using HttpClient client = new(handler);

            cookieContainer.Add(loginUri, new Cookie("login_session", loginSession));

            Dictionary<string, string> postBody = new()
            {
                { "_token", csrfToken },
                { "username", username },
                { "password", password }
            };

            FormUrlEncodedContent content = new(postBody);
            HttpResponseMessage response = await client.PostAsync(loginUri, content);
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 419)
                {
                    Logger.Error("Status code 419. This probably means a header/cookie/body-data is wrong/missing.", LogTitle);
                    return;
                }

                Logger.Error($"Unexpected status code {response.StatusCode}. Check your internet connection and try again", LogTitle);
                return;
            }

            string gaturroToken = null;
            string xsrfToken = null;

            IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(loginUri);
            foreach (Cookie cookie in responseCookies)
            {
                switch (cookie.Name)
                {
                    case "gaturrotoken":
                        gaturroToken = cookie.Value;
                        break;
                    case "XSRF-TOKEN":
                        xsrfToken = cookie.Value;
                        break;
                    default:
                        break;
                }
            }

            if (gaturroToken != null)
            {
                if (xsrfToken == null)
                {
                    Logger.Error("Couldn't get XSRF-TOKEN cookie", LogTitle);
                    return;
                }

                GetLoginID(gaturroToken, xsrfToken);
                return;
            }

            Logger.Error("Wrong username and/or password", LogTitle);
        }

        private async void GetLoginID(string gaturroToken, string xsrfToken)
        {
            Logger.Info("Attempting to get login ID...", LogTitle);

            Uri mmoUri = new("https://mmo.mundogaturro.com/");

            CookieContainer cookieContainer = new();
            using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            using HttpClient client = new(handler);

            cookieContainer.Add(mmoUri, new Cookie("gaturrotoken", gaturroToken));
            cookieContainer.Add(mmoUri, new Cookie("GEOIP_COUNTRY_CODE", "ar"));
            cookieContainer.Add(mmoUri, new Cookie("XSRF-TOKEN", xsrfToken));

            HttpResponseMessage response = await client.GetAsync(mmoUri);
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Unexpected status code {response.StatusCode}. Check your internet connection and try again", LogTitle);
                return;
            }

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                Logger.Error("Redirect detected. This shouldn't have happened", LogTitle);
                return;
            }

            string loginId = await response.Content.ReadAsStringAsync();
            loginId = loginId.Remove(0, loginId.IndexOf("loginId=", StringComparison.InvariantCulture) + 8);
            loginId = loginId.Remove(loginId.IndexOf("&", StringComparison.InvariantCulture));

            if (loginId.Length != 64)
            {
                Logger.Error($"We were expecting the login ID to have 64 characters, but it has {loginId.Length}", LogTitle);
                Logger.Debug(loginId, LogTitle);
                return;
            }

            Logger.Info("Login Successful", LogTitle);

            OnLoginIdReceivedEvent?.Invoke(UsernameTextbox.Text, loginId);
            Close();
        }
    }
}
