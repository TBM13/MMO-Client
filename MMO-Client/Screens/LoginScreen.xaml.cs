using System.Windows;
using System.Windows.Controls;
using MMO_Client.Common.Events;

namespace MMO_Client.Screens.LoginScreen
{
    public partial class LoginScreen : Window
    {
        public event Events.String2Event OnLoginEvent;

        public LoginScreen() => 
            InitializeComponent();

        private void PwdPasswordbox_PasswordChanged(object sender, RoutedEventArgs e) => 
            LoginButton.IsEnabled = UsernameTextbox.Text.Length >= 4 && PwdPasswordbox.Password.Length >= 4;

        private void UsernameTextbox_TextChanged(object sender, TextChangedEventArgs e) => 
            LoginButton.IsEnabled = UsernameTextbox.Text.Length >= 4 && PwdPasswordbox.Password.Length >= 4;

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            OnLoginEvent?.Invoke(UsernameTextbox.Text, PwdPasswordbox.Password);
            //Close();
        }
    }
}
