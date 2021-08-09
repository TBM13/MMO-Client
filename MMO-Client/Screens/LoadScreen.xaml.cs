using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MMO_Client.Screens
{
    public partial class LoadScreen : UserControl
    {
        internal Events.Event OnRetryClick { get; set; }

        public string LabelText { get => (string)LoadLabel.Content; set => LoadLabel.Content = value; }

        public LoadScreen() => 
            InitializeComponent();

        public void ResetProgressbar()
        {
            RetryButton.Visibility = Visibility.Hidden;
            LoadLabel.Visibility = Visibility.Visible;
            LoadProgressbar.Visibility = Visibility.Visible;
            LoadProgressbar.Value = 0;
            LoadProgressbar.IsIndeterminate = true;
            LoadProgressbar.Foreground = Utils.BrushFromHex("#FF06B025");
        }

        public void ShowError(string errorMsg, bool showRetryButton)
        {
            if (showRetryButton)
                RetryButton.Visibility = Visibility.Visible;

            LoadProgressbar.IsIndeterminate = false;
            LoadProgressbar.Foreground = Brushes.Red;
            LoadProgressbar.Value = 100;

            LoadLabel.Content = errorMsg;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            RetryButton.Visibility = Visibility.Hidden;
            OnRetryClick?.Invoke();
        }
    }
}
