using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MMO_Client.Screens
{
    public partial class OutputScreen : UserControl
    {
        public OutputScreen() => 
            InitializeComponent();

        public void AddParagraph(string text, Brush foreColor = null, Brush backColor = null, FontWeight? fontWeight = null)
        {
            Paragraph paragraph = new();
            Run run = new(text)
            {
                Foreground = foreColor ?? Brushes.Black,
                Background = backColor ?? Brushes.White,
                FontWeight = fontWeight ?? FontWeights.Normal
            };

            paragraph.Inlines.Add(run);
            RTB.Document.Blocks.Add(paragraph);

            ScrollToEnd();
        }

        public void AppendText(string text, Brush foreColor = null, Brush backColor = null, FontWeight? fontWeight = null)
        {
            Paragraph paragraph = (Paragraph)RTB.Document.Blocks.LastBlock;
            Run run = new(text)
            {
                Foreground = foreColor ?? Brushes.Black,
                Background = backColor ?? Brushes.White,
                FontWeight = fontWeight ?? FontWeights.Normal
            };

            paragraph.Inlines.Add(run);
            RTB.Document.Blocks.Add(paragraph);

            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            if (RTB.VerticalOffset + RTB.ViewportHeight < RTB.ExtentHeight) return;
            RTB.ScrollToEnd();
        }

        #region Context Menu
        private void RTBContextMenu_Opened(object sender, RoutedEventArgs e) => 
            CopyMenuItem.IsEnabled = !RTB.Selection.IsEmpty;

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e) => 
            RTB.Copy();

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e) => 
            RTB.SelectAll();
        #endregion
    }
}
