namespace MMO_Client.Client.World
{
    internal static class Direction
    {
        public const int North = 1;
        public const int South = 2;
        public const int West = 4;
        public const int East = 8;
        public const int Up = 16;
        public const int Down = 32;

        public static int Calculate(Coord coord1, Coord coord2)
        {
            int result = 0;

            if (coord1.X < coord2.X)
                result |= East;
            else if (coord1.X > coord2.X)
                result |= West;

            if (coord1.Y < coord2.Y)
                result |= South;
            else if (coord1.Y > coord2.Y)
                result |= North;

            if (coord1.Z < coord2.Z)
                result |= Up;
            else if (coord1.Z > coord2.Z)
                result |= Down;

            return result;
        }
    }
}
