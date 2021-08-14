using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MMO_Client.Client.World.Rooms;

namespace MMO_Client.Screens
{
    public partial class GameScreen : UserControl
    {
        public static GameScreen Instance { get; private set; }

        public static double GameWidth { get => Instance.canvas.Width; }
        public static double GameHeight { get => Instance.canvas.Height; }

        private readonly ScaleTransform scaleTransform = new(1.3, 1.3);

        public GameScreen()
        {
            Instance = this;
            InitializeComponent();

            Viewbox.RenderTransform = scaleTransform;
        }

        public void Setup()
        {
            RoomManager.Instance.OnRoomCreated += OnRoomCreated;
        }

        private void OnRoomCreated()
        {
            if (canvas.Children.Count > 1)
                canvas.Children.RemoveAt(1);

            canvas.Children.Add(Room.CurrentRoom.Canvas);
        }

        public void ShowLoadScreen(string text)
        {
            LoadScreen.ResetProgressbar();
            LoadScreen.LabelText = text;
            LoadScreen.Visibility = Visibility.Visible;
        }

        public void HideLoadScreen() => 
            LoadScreen.Visibility = Visibility.Hidden;

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            double modifier = e.Delta > 0 ? 0.1 : -0.1;

            if (modifier < 0 && scaleTransform.ScaleX <= 0.3)
                return;

            scaleTransform.ScaleX += modifier;
            scaleTransform.ScaleY += modifier;
        }
    }
}
