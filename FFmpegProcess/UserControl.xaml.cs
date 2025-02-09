
using System.Windows;
using System.Windows.Controls;

namespace FMR.FFMpegProcess
{
    /// <summary>
    /// UserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ControlPanel : UserControl
    {
        internal FFmpegProc ffmpegProc;
        public ControlPanel(FFmpegProc proc)
        {
            ffmpegProc = proc;
            InitializeComponent();
        }

        private void qualityModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            int index = qualityModeBox.SelectedIndex;
            double qualityValue = 17.0;
            if (index == 0)
            {
                Resources["crfMode"] = true;
                ffmpegProc.SetVideoProperty(VideoProperty.QualityMode, VideoProperty.CRF);
                if (crfSelect is not null)
                {
                    qualityValue = (int)crfSelect.Value;
                }
                ffmpegProc.SetVideoProperty(VideoProperty.CRF, ((int)qualityValue).ToString());
            }
            else
            {
                qualityValue = 20000.0;
                if (bitrateSelect is not null)
                {
                    qualityValue = (double)bitrateSelect.Value;
                }
                Resources["crfMode"] = false;
                ffmpegProc.SetVideoProperty(VideoProperty.QualityMode, VideoProperty.Bitrate);
                ffmpegProc.SetVideoProperty(VideoProperty.Bitrate, qualityValue.ToString());
            }
        }

        private void bitrateSelect_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            ffmpegProc.SetVideoProperty(VideoProperty.Bitrate, args.NewValue.ToString());
        }

        private void crfSelect_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            ffmpegProc.SetVideoProperty(VideoProperty.CRF, args.NewValue.ToString());
        }

        private void encoderSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            int index = encoderSelect.SelectedIndex;
            switch (index)
            {
                case 0:
                    ffmpegProc.SetVideoProperty(VideoProperty.Encoder, VideoProperty.LibX264);
                    break;
                case 1:
                    ffmpegProc.SetVideoProperty(VideoProperty.Encoder, VideoProperty.LibX265);
                    break;
            }
        }

        private void presetSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            int index = presetSelect.SelectedIndex;
            switch (index)
            {
                case 0:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.UltraFast);
                    break;
                case 1:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.SuperFast);
                    break;
                case 2:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.VeryFast);
                    break;
                case 3:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.Faster);
                    break;
                case 4:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.Fast);
                    break;
                case 5:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.Medium);
                    break;
                case 6:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.Slow);
                    break;
                case 7:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.Slower);
                    break;
                case 8:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.VerySlow);
                    break;
                case 9:
                    ffmpegProc.SetVideoProperty(VideoProperty.Preset, VideoProperty.Placebo);
                    break;
                default:
                    break;
            }
        }
    }
}
