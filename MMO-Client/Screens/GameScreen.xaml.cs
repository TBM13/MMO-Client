using System.Windows;
using System.Windows.Controls;
using MMO_Client.Client.World.Rooms;

namespace MMO_Client.Screens
{
    public partial class GameScreen : UserControl
    {
        public static GameScreen Instance { get; private set; }

        public static double GameWidth { get => Instance.canvas.Width; }
        public static double GameHeight { get => Instance.canvas.Height; }

        /// <summary>
        /// The objects look too small in comparison to MG, so we have to multiply their size.
        /// </summary>
        public const double SizeMultiplier = 1.3;

        public GameScreen()
        {
            Instance = this;
            InitializeComponent();
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
    }
}
