﻿namespace MMO_Client.Client.World
{
    class Coord
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }

        public Coord(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}