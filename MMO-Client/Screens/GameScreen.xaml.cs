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

            RoomManager.Instance.OnRoomCreated += OnRoomCreated;
        }

        private void OnRoomCreated()
        {
            canvas.Children.Add(Room.CurrentRoom.Canvas);
        }
    }
}
