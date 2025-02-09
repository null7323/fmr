using FMR.Core.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace NoteCounter
{
    /// <summary>
    /// RenderControl.xaml 的交互逻辑
    /// </summary>
    public partial class RenderControl : UserControl
    {
        public const string DefaultFormat =
"""
Note Count: {notePassed}/{totalNoteCount}
Division: {division}
Polyphony: {polyphony}
NPS: {nps}
Time: {currentTime}/{duration}
BPM: {bpm}
""";
        internal int newFontSize;
        internal Renderer renderer;
        internal List<string> availableFontNames;

        public RenderControl(Renderer renderer)
        {
            this.renderer = renderer;
            InitializeComponent();

            textFormat.Text = DefaultFormat;

            fonts.Items.Clear();
            availableFontNames = [];

            foreach (System.Windows.Media.FontFamily systemFont in Fonts.SystemFontFamilies)
            {
                fonts.Items.Add(new ComboBoxItem()
                {
                    Content = systemFont.Source,
                    FontFamily = systemFont
                });
                availableFontNames.Add(systemFont.Source);
            }

            int index = availableFontNames.IndexOf("Arial");
            fonts.SelectedIndex = index;
        }

        private void fontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            if (sender is null)
            {
                return;
            }
            newFontSize = (int)args.NewValue;
            renderer.fontSize = newFontSize;
        }

        private void fontStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return; 
            }
            switch (fontStyle.SelectedIndex)
            {
                case 0:
                    renderer.fontStyle = System.Drawing.FontStyle.Regular;
                    break;
                case 1:
                    renderer.fontStyle = System.Drawing.FontStyle.Bold;
                    break;
                case 2:
                    renderer.fontStyle = System.Drawing.FontStyle.Italic;
                    break;
                case 3:
                    renderer.fontStyle = System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic;
                    break;
                case 4:
                    renderer.fontStyle = System.Drawing.FontStyle.Underline;
                    break;
                case 5:
                    renderer.fontStyle = System.Drawing.FontStyle.Strikeout;
                    break;
            }
        }

        private void textFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            string text = textFormat.Text;
            renderer.outputFormat = text;
        }

        private void fonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            renderer.fontName = availableFontNames[fonts.SelectedIndex];
        }

        private void numberSeparator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return; 
            }
            if (numberSeparator.SelectedIndex == 0)
            {
                renderer.generalTextFormat = string.Empty;
                renderer.bpmTextFormat = "0.00";
            }
            else
            {
                renderer.generalTextFormat = "N0";
                renderer.bpmTextFormat = "N2";
            }
        }
    }
}
