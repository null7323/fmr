using FMR.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FMR.FFMpegProcess
{
#pragma warning disable CS8618
#pragma warning disable CS8625
    /// <summary>
    /// 表示用于<see cref="FFmpegProc.SetVideoProperty(string, string)"/>的属性.
    /// </summary>
    public static class VideoProperty
    {
#pragma warning disable CS1591
        public const string Encoder = "-vcodec";
        public const string Preset = "-preset";
        public const string CRF = "-crf";

        public const string Bitrate = "bitrate";
        public const string Additional = " ";
        public const string QualityMode = "qmode";
        
        public const string UltraFast = "ultrafast";
        public const string SuperFast = "superfast";
        public const string VeryFast = "veryfast";
        public const string Faster = "faster";
        public const string Fast = "fast";
        public const string Medium = "medium";
        public const string Slow = "slow";
        public const string Slower = "slower";
        public const string VerySlow = "veryslow";
        public const string Placebo = "placebo";

        public const string LibX264 = "libx264";
        public const string LibX265 = "libx265";
        public const string SDL2Preview = "sdl2 Preview";
        public const string SDLPreview = "sdl Preview";
        public const string YUV420p = "yuv420p";
#pragma warning restore CS1591
    }

    /// <summary>
    /// 表示默认的<see cref="IVideoExport"/>实现.
    /// </summary>
    /// <remarks>
    /// 该实现使用FFMpeg和管道完成.
    /// </remarks>
    public class FFmpegProc : IVideoExport
    {
        private string additionalFFArgs;
        private string exportPath;
        private string preset;
        private int width, height, fps;
        private Process ffProc;
        private bool procRunning;
        private bool useCRF;
        private string quality;
        private string videoEncoder;
        private string pixelFormat;
        private UserControl panel;


        /// <summary>
        /// 初始化<see cref="FFmpegProc"/>实例.
        /// </summary>
        /// <remarks>
        /// 默认情况下，视频使用crf质量表示，初始值为17；预设使用<see cref="VideoProperty.UltraFast"/>.
        /// </remarks>
        public FFmpegProc()
        {
            additionalFFArgs = string.Empty;
            ffProc = null;
            procRunning = false;
            exportPath = string.Empty;
            useCRF = true;
            quality = "17";
            preset = VideoProperty.UltraFast;
            videoEncoder = VideoProperty.LibX264;
            panel = new ControlPanel(this);
        }


        /// <inheritdoc/>
        public void BeginWrite()
        {
            string finalFFArg = $" -y -hide_banner -f rawvideo -pix_fmt rgba -s {width}x{height} -r {fps} -i - " +
                $"-vcodec {videoEncoder} -pix_fmt yuv420p -preset {preset} {additionalFFArgs} ";
            if (useCRF)
            {
                finalFFArg += $" -crf {quality} ";
            }
            else
            {
                finalFFArg += $" -b:v {quality}k -maxrate {quality}k -minrate {quality}k -bufsize {quality}k ";
            }
            finalFFArg += $"\"{exportPath}\"";
            ffProc = new Process()
            {
                StartInfo = new ProcessStartInfo("ffmpeg", finalFFArg)
                {
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };
            ffProc.Start();
            procRunning = !ffProc.HasExited;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (ffProc != null)
            {
                ffProc.Close();
                ffProc.Dispose();
            }
            ffProc = null;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void EndWrite()
        {
            if (procRunning || (!ffProc.HasExited))
            {
                ffProc.StandardInput.Close();
                ffProc.Close();
            }
            ffProc = null;
        }

        /// <inheritdoc/>
        public void SetExportPath(string path)
        {
            exportPath = path;
        }

        /// <inheritdoc/>
        public void SetVideoFPS(int fps)
        {
            this.fps = fps;
        }

        /// <inheritdoc/>
        public void SetVideoProperty(string propertyName, string value)
        {
            switch (propertyName)
            {
                case VideoProperty.Additional:
                    additionalFFArgs = value;
                    break;
                case VideoProperty.Preset:
                    preset = value;
                    break;
                case VideoProperty.CRF:
                    useCRF = true;
                    quality = value;
                    break;
                case VideoProperty.Bitrate:
                    useCRF = false;
                    quality = value;
                    break;
                case VideoProperty.Encoder:
                    videoEncoder = value;
                    break;
                case VideoProperty.QualityMode:
                    if (value != VideoProperty.Bitrate)
                    {
                        useCRF = true;
                    }
                    else
                    {
                        useCRF = false;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFrame(byte* pixels)
        {
            if (ffProc.HasExited)
            {
                procRunning = false;
            }
            if (!procRunning)
            {
                ThrowHelper.Throw("FFMpeg process not running. Check whether ffmpeg process isn't started, or ffmpeg crashed accidentally.");
            }
            ffProc.StandardInput.BaseStream.Write(new Span<byte>(pixels, width * height * 4));
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameSize(int width, int height, int pixelSize)
        {
            this.width = width;
            this.height = height;
            if (pixelSize != 4)
            {
                ThrowHelper.Throw("Unsupported pixel size.");
            } 
                
        }

        public string Name => "FFmpeg (Pipe)";
        public Control ControlPanel => panel;

        public ColorType SupportedColorType => ColorType.RGBA_Int;
    }
#pragma warning restore CS8618
#pragma warning restore CS8625
}
