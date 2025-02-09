using System;
using System.Collections.Generic;
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

namespace QQS.Legacy
{
    /// <summary>
    /// RenderControl.xaml 的交互逻辑
    /// </summary>
    public partial class RenderControl : UserControl
    {
        internal Renderer renderer;
        public RenderControl(Renderer renderer)
        {
            this.renderer = renderer;
            InitializeComponent();

            previewBackgroundColor.Background = new SolidColorBrush(new Color
            {
                R = 0,
                G = 0,
                B = 0,
                A = 255
            });
        }

        private void noteSpeedSelect_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            renderer.SetRenderProperty(RendererProperties.NoteSpeed, args.NewValue.ToString());
        }

        private void keyHeightSelect_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            renderer.SetRenderProperty(RendererProperties.KeyHeightPercentage, (args.NewValue / 100).ToString());
        }

        private void setBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            string coltxt = bgColor.Text;
            if (coltxt.Length != 6)
            {
                _ = MessageBox.Show("当前的颜色代码不符合规范.\n一个颜色代码应当由6位16进制表示的数字组成.", "无法设置颜色");
                return;
            }
            try
            {
                byte r = Convert.ToByte(coltxt[..2], 16);
                byte g = Convert.ToByte(coltxt.Substring(2, 2), 16);
                byte b = Convert.ToByte(coltxt.Substring(4, 2), 16);
                uint col = 0xFF000000U | r | (uint)(g << 8) | (uint)(b << 16);
                renderer.SetRenderProperty(RendererProperties.BackgroundColor, col.ToString());
                previewBackgroundColor.Background = new SolidColorBrush(new Color()
                {
                    R = r,
                    G = g,
                    B = b,
                    A = 0xff
                });
            }
            catch
            {
                _ = MessageBox.Show("错误: 无法解析颜色代码.\n请检查输入的颜色代码是否正确.", "无法设置颜色");
            }
        }
    }
}
