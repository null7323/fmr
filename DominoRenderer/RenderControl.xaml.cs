using FMR.Core.UI;
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

namespace DominoRenderer
{
    /// <summary>
    /// RenderControl.xaml 的交互逻辑
    /// </summary>
    public partial class RenderControl : UserControl
    {
        Renderer renderer;
        public RenderControl(Renderer renderer)
        {
            this.renderer = renderer;
            InitializeComponent();
        }

        private void keyboardWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            renderer.keyboardWidth = (double)args.NewValue / 100.0;
        }

        private void beatsOnScreen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            renderer.beatsPerScreen = (double)args.NewValue;
        }

        private void previewWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            renderer.previewWidth = (int)args.NewValue;
        }

        private void previewHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            int height = (int)args.NewValue;
            renderer.previewHeight = (int)args.NewValue;
        }

        private void overrideColorSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            renderer.overrideColorSettings = overrideColorSettings.SelectedIndex switch
            {
                0 => false,
                1 => true,
                _ => false,
            };
        }

        private void shuffleDominoColors_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }

            // 确定随机顺序
            {
                List<int> orders = [0, 1, 2, 3, 4, 5, 6, 7];
                Random random = new();
                int i = 0;
                while (orders.Count > 0)
                {
                    int k = random.Next(0, orders.Count);
                    renderer.overrideColorOrder[i++] = orders[k];
                    orders.RemoveAt(k);
                }
            }
        }

        private void restoreDominoColorOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            renderer.overrideColorOrder = [0, 1, 2, 3, 4, 5, 6, 7];
        }
    }
}
