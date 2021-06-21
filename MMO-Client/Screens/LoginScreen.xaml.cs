using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using MMO_Client.Common; // A
using MMO_Client.Common.Events;

namespace MMO_Client.Screens
{
    public partial class LoginScreen : Window
    {
        public event Events.String2Event OnLoginEvent;
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        public LoginScreen() {
            AllocConsole();
            InitializeComponent();
        }
        
        private void PwdPasswordbox_PasswordChanged(object sender, RoutedEventArgs e) => 
            LoginButton.IsEnabled = UsernameTextbox.Text.Length >= 4 && PwdPasswordbox.Password.Length >= 4;

        private void UsernameTextbox_TextChanged(object sender, TextChangedEventArgs e) => 
            LoginButton.IsEnabled = UsernameTextbox.Text.Length >= 4 && PwdPasswordbox.Password.Length >= 4;

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Debug("Test login debug " + UsernameTextbox.Text);
            OnLoginEvent?.Invoke(UsernameTextbox.Text, PwdPasswordbox.Password);
            Close();
        }
    }
}
