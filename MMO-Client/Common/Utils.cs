using System;
using System.Windows.Media;

namespace MMO_Client
{
    internal static class Utils
    {
        private readonly static BrushConverter brushConverter = new();

        public static long GetUnixTime() =>
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static Brush BrushFromHex(string hex) =>
            (Brush)brushConverter.ConvertFromString(hex);
    }
}