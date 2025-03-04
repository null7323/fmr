using FMR.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace TexturedRenderer
{
    public class TexturePack
    {
        internal static bool GetPossiblyBooleanProperty(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => bool.Parse(element.ToString()),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                _ => throw new InvalidDataException("Bad Pack Description: Invalid type."),
            };
        }
        public TexturePack(ResourceManager assetManager, string textureName)
        {
            string assetFolder = assetManager.ResourcePath + '\\' + textureName + '\\';

            using JsonDocument packDescription = JsonDocument.Parse(File.ReadAllText(assetFolder + "Pack.json"));
            JsonElement root = packDescription.RootElement;

            TextureDescription = root.GetProperty("description").ToString();

            SameWidthNotes = GetPossiblyBooleanProperty(root.GetProperty("sameWidthNotes"));
            WhiteKey = root.GetProperty("whiteKey").ToString();
            BlackKey = root.GetProperty("blackKey").ToString();
            WhiteKeyPressed = root.GetProperty("whiteKeyPressed").ToString();
            BlackKeyPressed = root.GetProperty("blackKeyPressed").ToString();
            Bar = root.GetProperty("bar").ToString();
            KeyboardHeight = root.GetProperty("keyboardHeight").GetDouble();
            BarHeight = root.GetProperty("barHeight").GetDouble();
            BlackKeyHeight = root.GetProperty("blackKeyHeight").GetDouble();
        }

        public string TextureDescription { get; }
        public bool SameWidthNotes { get; }
        public string WhiteKey { get; }
        public string BlackKey { get; }
        public string WhiteKeyPressed { get; }
        public string BlackKeyPressed { get; }
        public string Bar { get; }
        public double KeyboardHeight { get; }
        public double BarHeight { get; }
        public double BlackKeyHeight { get; }
    }
    /// <summary>
    /// RenderControl.xaml 的交互逻辑
    /// </summary>
    public partial class RenderControl : UserControl
    {
        public RenderControl()
        {
            InitializeComponent();
        }

        private void reloadTextures_Click(object sender, RoutedEventArgs e)
        {

        }

        private void availableTextures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
