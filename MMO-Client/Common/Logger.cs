using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using MMO_Client.Screens;

namespace MMO_Client.Common
{
    internal class Logger
    {
        private static Logger instance;
        private readonly OutputScreen outputScreen;

        public Logger()
        {
            instance = this;
            outputScreen = new("Logger");
            outputScreen.Show();

            outputScreen.AppendText($"[{DateTime.Now:HH:mm:ss}] [Logger] ", Brushes.Blue);
            outputScreen.AppendText("Logger Created");
        }

        public void Write(string text1, string text2, Brush infoColor, Brush textColor, Brush backColor, FontWeight fontWeight)
        {
            outputScreen.AddParagraph($"[{DateTime.Now:HH:mm:ss}] [{text1}] ", infoColor, backColor, fontWeight);
            outputScreen.AppendText(text2, textColor, backColor, fontWeight);
        }

        public static void Info(string text, [CallerFilePath] string title = "", bool bold = false)
        {
            if (title.Contains("\\"))
                title = GetCaller(title);

            instance.Write(title, text, Brushes.Blue, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);
        }

        public static void Warn(string text, [CallerFilePath] string title = "", bool bold = false)
        {
            if (title.Contains("\\"))
                title = GetCaller(title);

            instance.Write(title, text, Brushes.Orange, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);
        }

        public static void Error(string text, [CallerFilePath] string title = "", bool bold = false)
        {
            if (title.Contains("\\"))
                title = GetCaller(title);

            instance.Write(title, text, Brushes.Red, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);
        }

        public static void Fatal(string text, [CallerFilePath] string title = "")
        {
            if (title.Contains("\\"))
                title = GetCaller(title);

            instance.Write(title, text, Brushes.White, Brushes.White, Brushes.Red, FontWeights.Bold);
        }

        public static void Debug(string text, [CallerFilePath] string title = "", bool bold = false)
        {
#if DEBUG
            if (title.Contains("\\"))
                title = GetCaller(title);

            instance.Write(title, text, Brushes.LimeGreen, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);
#endif
        }

        private static string GetCaller(string path)
        {
            path = path.Remove(0, path.LastIndexOf("\\") + 1);
            int i = path.IndexOf(".");
            path = path.Remove(i, path.Length - i);

            return path;
        }
    }
}
