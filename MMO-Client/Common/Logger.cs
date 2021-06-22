using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using MMO_Client.Screens;

namespace MMO_Client.Common
{
    public class Logger
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

        public static void Info(string text, [CallerMemberName]string methodName = "", bool bold = false) =>
            instance.Write(methodName, text, Brushes.Blue, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);

        public static void Warn(string text, [CallerMemberName] string methodName = "", bool bold = false) =>
            instance.Write(methodName, text, Brushes.Orange, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);

        public static void Error(string text, [CallerMemberName] string methodName = "", bool bold = false) =>
            instance.Write(methodName, text, Brushes.Red, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);

        public static void Fatal(string text, [CallerMemberName] string methodName = "") =>
            instance.Write(methodName, text, Brushes.White, Brushes.White, Brushes.Red, FontWeights.Bold);

        public static void Debug(string text, [CallerMemberName] string methodName = "", bool bold = false)
        {
#if DEBUG
            instance.Write(methodName, text, Brushes.LimeGreen, Brushes.Black, Brushes.White, bold ? FontWeights.Bold : FontWeights.Regular);
#endif
        }
    }
}
