using System.Collections.Generic;

namespace MMO_Client.Client.World.Path
{
    internal class Path
    {
        public Coord OriginalDestination { get; private set; }
        private List<Coord> Coords { get; init; }

        public Path(List<Coord> coords, Coord originalDestination = null)
        {
            Coords = coords;
            OriginalDestination = originalDestination;
        }

        public Coord Coord { get => Coords.Count > 0 ? Coords[0] : null; }
        public Coord NextCoord { get => Coords.Count > 1 ? Coords[1] : null; }
        public Coord Destination { get => Coords[^1]; }

        public void CropAt(Coord coord)
        {
            if (HasValue(coord))
                Coords.RemoveRange(0, Coords.IndexOf(coord) + 1);
        }

        public bool HasValue(Coord coord) =>
            Coords.Contains(coord);

        public void Step() =>
            Coords.RemoveAt(0);
    }
}
