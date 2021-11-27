using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MMO_Client.Screens
{
    public partial class LoginScreen : UserControl
    {
        internal event Events.String2BoolEvent OnLoginAttempt;

        public LoginScreen()
        {
            InitializeComponent();
            PopulateSavedCredentialsList();
        }

        private void Box_TextChanged(object sender, RoutedEventArgs e)
        {
            bool validCredentials = ValidateCredentials();
            LoginButton.IsEnabled = validCredentials;
            SaveButton.IsEnabled = validCredentials;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            OnLoginAttempt += OnLoginEvent;
            _ = LoginAsync();
        }

        private void OnLoginEvent(string username, string loginId, bool success)
        {
            OnLoginAttempt -= OnLoginEvent;
            LoginProgressbar.Visibility = Visibility.Hidden;
            SavedCredentialsList.IsEnabled = true;
        }

        private bool ValidateCredentials()
        {
            if (UsernameBox.Text.Length < 4)
                return false;
            if (PwdBox.Password.Length < 4)
                return false;

            if (UsernameBox.Text.Length > 16)
                return false;
            if (PwdBox.Password.Length >= 16)
                return false;

            return true;
        }

        private async Task LoginAsync()
        {
            LoginProgressbar.Value = 0;
            LoginProgressbar.Visibility = Visibility.Visible;
            SavedCredentialsList.IsEnabled = false;

            Logger.Debug("Attempting to get CSRF Token and Login Session cookies...");
            Uri loginUri = new("https://login.mundogaturro.com/");

            CookieContainer cookieContainer = new();
            using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            using HttpClient client = new(handler);

            HttpResponseMessage response = await client.GetAsync(loginUri);
            LoginProgressbar.Value = 20;

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Unexpected status code {response.StatusCode}. Check your internet connection and try again");
                OnLoginAttempt?.Invoke(UsernameBox.Text.ToUpperInvariant(), "", false);
                return;
            }

            string csrfToken = await response.Content.ReadAsStringAsync();
            csrfToken = csrfToken.Remove(0, csrfToken.IndexOf("csrf-token\" content=\"", StringComparison.InvariantCulture) + 21);
            csrfToken = csrfToken.Remove(csrfToken.IndexOf("\"", StringComparison.InvariantCulture));
            LoginProgressbar.Value = 40;

            if (csrfToken.Length != 40)
            {
                Logger.Error($"We were expecting the CSRF Token to have 40 characters, but it has {csrfToken.Length}");
                Logger.Debug(csrfToken);
                OnLoginAttempt?.Invoke(UsernameBox.Text.ToUpperInvariant(), "", false);
                return;
            }

            string? loginSession = null;
            foreach (Cookie cookie in cookieContainer.GetCookies(loginUri))
            {
                if (cookie.Name == "login_session")
                {
                    loginSession = cookie.Value;
                    break;
                }
            }

            if (loginSession == null)
            {
                Logger.Error("Couldn't get Login Session cookie");
                OnLoginAttempt?.Invoke(UsernameBox.Text.ToUpperInvariant(), "", false);
                return;
            }

            await GetGaturroTokenAsync(UsernameBox.Text, PwdBox.Password, csrfToken, loginSession);
        }

        private async Task GetGaturroTokenAsync(string username, string password, string csrfToken, string loginSession)
        {
            Logger.Debug("Attempting to get gaturro token cookie...");

            Uri loginUri = new("http://desktop.mundogaturro.com/auth");

            CookieContainer cookieContainer = new();
            using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            using HttpClient client = new(handler);

            cookieContainer.Add(loginUri, new Cookie("login_session", loginSession));

#pragma warning disable CS8714
            Dictionary<string?, string?> postBody = new()
#pragma warning restore CS8714
            {
                { "_token", csrfToken },
                { "username", username },
                { "password", password }
            };

            FormUrlEncodedContent content = new(postBody);
            HttpResponseMessage response = await client.PostAsync(loginUri, content);
            LoginProgressbar.Value = 60;

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 419)
                {
                    Logger.Error("Status code 419. This probably means a header/cookie/body-data is wrong/missing.");
                    OnLoginAttempt?.Invoke(username.ToUpperInvariant(), "", false);
                    return;
                }

                Logger.Error($"Unexpected status code {response.StatusCode}. Check your internet connection and try again");
                OnLoginAttempt?.Invoke(username.ToUpperInvariant(), "", false);
                return;
            }

            string? gaturroToken = null;
            string? xsrfToken = null;

            foreach (Cookie cookie in cookieContainer.GetCookies(loginUri))
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
                    Logger.Error("Couldn't get XSRF-TOKEN cookie");
                    OnLoginAttempt?.Invoke(username.ToUpperInvariant(), "", false);
                    return;
                }

                await GetLoginIDAsync(gaturroToken, xsrfToken, username);
                return;
            }

            Logger.Error("Wrong username and/or password");
            OnLoginAttempt?.Invoke(username.ToUpperInvariant(), "", false);
        }

        private async Task GetLoginIDAsync(string gaturroToken, string xsrfToken, string username)
        {
            Logger.Debug("Attempting to get login ID...");

            Uri mmoUri = new("https://mmo.mundogaturro.com/");

            CookieContainer cookieContainer = new();
            using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            using HttpClient client = new(handler);

            cookieContainer.Add(mmoUri, new Cookie("gaturrotoken", gaturroToken));
            cookieContainer.Add(mmoUri, new Cookie("GEOIP_COUNTRY_CODE", "ar"));
            cookieContainer.Add(mmoUri, new Cookie("XSRF-TOKEN", xsrfToken));

            HttpResponseMessage response = await client.GetAsync(mmoUri);
            LoginProgressbar.Value = 80;

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Unexpected status code {response.StatusCode}. Check your internet connection and try again");
                OnLoginAttempt?.Invoke(username.ToUpperInvariant(), "", false);
                return;
            }

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                Logger.Error("Redirect detected. This shouldn't have happened");
                OnLoginAttempt?.Invoke(username.ToUpperInvariant(), "", false);
                return;
            }

            string content = await response.Content.ReadAsStringAsync();

            string loginId = content;
            loginId = loginId.Remove(0, loginId.IndexOf("loginId=", StringComparison.InvariantCulture) + 8);
            loginId = loginId.Remove(loginId.IndexOf("&", StringComparison.InvariantCulture));

            if (loginId.Length != 64)
            {
                Logger.Error($"We were expecting the login ID to have 64 characters, but it has {loginId.Length}");

                if (content.Contains("https://descargas.mundogaturro.com/mundogaturro_installer_la.exe"))
                {
                    Logger.Info("Retrying...");
                    await GetLoginIDAsync(gaturroToken, xsrfToken, username);
                }

                return;
            }

            LoginProgressbar.Value = 100;
            Logger.Info("Login Successful");

            OnLoginAttempt?.Invoke(username.ToUpperInvariant(), loginId, true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PwdBox.Password;

            if (username.Contains(";") || password.Contains(";"))
            {
                Logger.Error("The username and password can't contain the character ';'");
                return;
            }

            int i = SavedCredentialsList.SelectedIndex;
            if (i < 1)
            {
                AppSettings.Default.SavedCredentials.Add($"{username};{password}");

                AddSavedCrendetialsItem(username);
            }
            else
            {
                AppSettings.Default.SavedCredentials[i - 1] = $"{username};{password}";
                ((ListViewItem)SavedCredentialsList.Items[i]).Content = username;
            }

            AppSettings.Default.Save();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int i = SavedCredentialsList.SelectedIndex;

            AppSettings.Default.SavedCredentials.RemoveAt(i - 1);
            AppSettings.Default.Save();

            SavedCredentialsList.Items.RemoveAt(i);
            DeleteButton.IsEnabled = false;
        }

        private void SavedCredentialsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = SavedCredentialsList.SelectedIndex;
            if (i < 1)
            {
                DeleteButton.IsEnabled = false;
                return;
            }

            string[] data = AppSettings.Default.SavedCredentials[i - 1].Split(';');
            UsernameBox.Text = data[0];
            PwdBox.Password = data[1];

            DeleteButton.IsEnabled = true;
        }

        private void PopulateSavedCredentialsList()
        {
            foreach (string s in AppSettings.Default.SavedCredentials)
                AddSavedCrendetialsItem(s.Split(';')[0]);
        }

        private void AddSavedCrendetialsItem(string username)
        {
            ListViewItem item = new()
            {
                Content = username
            };

            item.MouseDoubleClick += (sender, e) =>
            {
                OnLoginAttempt += OnLoginEvent;
                _ = LoginAsync();
            };

            SavedCredentialsList.Items.Add(item);
        }
    }
}
