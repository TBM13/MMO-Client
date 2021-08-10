namespace MMO_Client.Client.World
{
    internal class Coord
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }

        private int[] array;

        public Coord(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Coord(int[] array)
        {
            X = array[0];
            Y = array[1];
            Z = array[2];
        }

        public int[] ToArray()
        {
            array ??= new int[3];
            array[0] = X;
            array[1] = Y;
            array[2] = Z;

            return array;
        }
    }
}
