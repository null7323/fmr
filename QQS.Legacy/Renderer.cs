using FMR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QQS.Legacy;

namespace QQS.Legacy
{
    public static class RendererProperties
    {
        public const string BackgroundColor = "bgcolor";
        public const string NoteSpeed = "notespeed";
        public const string KeyHeightPercentage = "keyheight";
    }
    public class Renderer : IMidiRenderer
    {
        private bool rendering = false;
        private bool interrupted = false;
        private int width, height, fps;

        private RGBAColor[] trackColors;
        private MidiFile file;
        private RenderControl control;

        private RGBAColor background;
        private Canvas canvas;

        private double noteSpeed;
        private double division = 480.0;
        private double keyHeightPercentage;

        private long tempoCount;
        private long speedIndex;

        private double tick;
        private double speed;

        private int emptyFrameCount;

        private long[] endedNotes;

        public Renderer()
        {
            width = 1920;
            height = 1080;
            fps = 60;

            noteSpeed = 1.5;
            keyHeightPercentage = 0.15;

            background = new(0, 0, 0, 0);
            control = new RenderControl(this);
        }

        public int PixelSize => 4;

        public bool Rendering => rendering;

        public bool RenderEnded => !rendering;

        public bool RenderInterrupted => interrupted;

        public double MidiLoaderOffset => 0.0;

        public string Name => "QMidicore Quaver Stream Renderer (Legacy)";

        public string Description => "旧版的QMidicore Quaver Stream Renderer渲染器 (Flat风格). 使用软件渲染.";

        public ColorType SupportedColorType => ColorType.RGBA_Int;

        public Control ControlPanel => control;

        public bool HasWindow => false;

        private int[] keyX = new int[128];
        private int[] keyWidth = new int[128];

        public void BeginRender()
        {
            canvas = new Canvas(width, height, (uint)background);

            speedIndex = 0;
            tick = 0D;
            speed = division * 2.0 / fps;

            emptyFrameCount = 3 * fps;

            endedNotes = new long[128];
            unsafe
            {
                fixed (long* p = endedNotes)
                {
                    UnsafeMemory.Set(p, 0, 128 * sizeof(long));
                }
            }
            for (int i = 0; i != 128; ++i)
            {
                keyX[i] = (i / 12 * 126 + GenKeyX[i % 12]) * width / 1350;
            }
            for (int i = 0; i != 127; ++i)
            {
                var val = (i % 12) switch
                {
                    1 or 3 or 6 or 8 or 10 => width * 9 / 1350,
                    4 or 11 => keyX[i + 1] - keyX[i],
                    _ => keyX[i + 2] - keyX[i],
                };
                keyWidth[i] = val;
            }
            keyWidth[127] = width - keyX[127];

            rendering = true;
        }

        public void BindMidiFile(IMidiLoader midiFile)
        {
            if (midiFile is MidiFile file)
            {
                this.file = file;

                division = file.Division;
                tempoCount = file.Tempos.Count;
            }
        }

        public unsafe void CopyPixelsTo(IVideoExport videoExport)
        {
            videoExport.WriteFrame((byte*)canvas.pixels);
        }

        public void Dispose()
        {
            canvas.Dispose();
            GC.SuppressFinalize(this);
        }

        public void FreeRenderer()
        {
            rendering = false;
            canvas.Dispose();
        }

        public unsafe void RenderNextFrame()
        {
            if (!rendering)
            {
                return;
            }

            if (tick >= file.MidiTime && emptyFrameCount <= 0)
            {
                rendering = false;
                return;
            }
            canvas.Clear();

            double pixelPerBeat = 520.0 / division * noteSpeed;
            uint keyHeight = (uint)(keyHeightPercentage * canvas.height);

            int colorCount = trackColors.Length;

            double deltaTicks = (height - keyHeight) / pixelPerBeat;
            double tickUp = tick + deltaTicks;

            Note** noteBegins = stackalloc Note*[128];
            Note** end = stackalloc Note*[128];
            uint* keyColors = stackalloc uint[128];

            for (int i = 0; i < 128; ++i)
            {
                // 获取Note首地址
                if (file.Notes[i].Count != 0)
                {
                    noteBegins[i] = UnsafeMemory.GetActualAddressOf(ref file.Notes[i][0]);
                    end[i] = noteBegins[i] + file.Notes[i].Count;
                }
                else
                {
                    noteBegins[i] = end[i] = null;
                }
                // 设置默认琴键颜色
                keyColors[i] = (i % 12) switch
                {
                    1 or 3 or 6 or 8 or 10 => 0xFF000000,
                    _ => 0xFFFFFFFF,
                };
            }

            while (speedIndex < tempoCount && file.Tempos[speedIndex].Tick <= tick)
            {
                speed = 1e6 / file.Tempos[speedIndex].Value * division / fps;
                ++speedIndex;
            }

            Parallel.For(0, 75, (i) =>
            {
                i = DrawMap[i];
                if (noteBegins[i] == null)
                {
                    return;
                }
                long startIndex = endedNotes[i];
                Note* ptr = noteBegins[i] + startIndex;
                Note* pEnd = end[i];
                bool flag = false;

                uint y;
                uint length;
                int x = keyX[i];
                int width = keyWidth[i];

                while (ptr->Start < tickUp)
                {
                    if (ptr == pEnd)
                    {
                        return;
                    }
                    if (ptr->End >= tick)
                    {
                        uint trackColor = (uint)trackColors[ptr->Track % colorCount];
                        if (!flag)
                        {
                            flag = true;
                            startIndex = ptr - noteBegins[i];
                        }
                        if (ptr->Start < tick)
                        {
                            keyColors[i] = trackColor;

                            y = keyHeight;
                            length = (uint)((ptr->End - tick) * pixelPerBeat);
                        }
                        else
                        {
                            y = (uint)((ptr->Start - tick) * pixelPerBeat + keyHeight);
                            length = (uint)((ptr->End - ptr->Start) * pixelPerBeat);
                        }
                        if (y + length > height)
                        {
                            length = (uint)height - y;
                        }
                        canvas.FillRectangle(x + 1, (int)y, width - 1, (int)length, trackColor);
                    }
                    ++ptr;
                }
                endedNotes[i] = startIndex;
            });
            Parallel.For(75, 128, (i) =>
            {
                i = DrawMap[i];
                if (noteBegins[i] == null)
                {
                    return;
                }
                long startIndex = endedNotes[i];
                Note* ptr = noteBegins[i] + startIndex;
                Note* pEnd = end[i];
                bool flag = false;

                uint y;
                uint length;
                int x = keyX[i];
                int width = keyWidth[i];

                while (ptr->Start < tickUp)
                {
                    if (ptr == pEnd)
                    {
                        return;
                    }
                    if (ptr->End >= tick)
                    {
                        uint trackColor = (uint)trackColors[ptr->Track % colorCount];
                        if (!flag)
                        {
                            flag = true;
                            startIndex = ptr - noteBegins[i];
                        }
                        if (ptr->Start < tick)
                        {
                            keyColors[i] = trackColor;

                            y = keyHeight;
                            length = (uint)((ptr->End - tick) * pixelPerBeat);
                        }
                        else
                        {
                            y = (uint)((ptr->Start - tick) * pixelPerBeat + keyHeight);
                            length = (uint)((ptr->End - ptr->Start) * pixelPerBeat);
                        }
                        if (y + length > height)
                        {
                            length = (uint)height - y;
                        }
                        canvas.FillRectangle(x + 1, (int)y, width - 1, (int)length, trackColor);
                    }
                    ++ptr;
                }
                endedNotes[i] = startIndex;
            });
            DrawKeys(keyColors);
            tick += speed;

            if (tick >= file.MidiTime)
            {
                emptyFrameCount--;
            }
        }

        public void SetColors(RGBAColor[] colors)
        {
            trackColors = colors;
        }

        public void SetColors(Vec4Color[] colors)
        {
            ThrowHelper.ThrowNotImplemented();
        }

        public void SetRenderConfiguration(RenderConfiguration configuration)
        {
            width = configuration.Width;
            height = configuration.Height;
            fps = configuration.FPS;
        }

        public void SetRenderProperty(string propertyName, string value)
        {
            switch (propertyName)
            {
                case RendererProperties.BackgroundColor:
                    unsafe
                    {
                        if (uint.TryParse(value, out uint color))
                        {
                            background = *(RGBAColor*)&color;
                        }
                    }
                    break;
                case RendererProperties.NoteSpeed:
                    if (double.TryParse(value, out double speed))
                    {
                        noteSpeed = speed;
                    }
                    break;
                case RendererProperties.KeyHeightPercentage:
                    if (double.TryParse(value, out double keyHeightPercentage))
                    {
                        this.keyHeightPercentage = keyHeightPercentage;
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void DrawKeys(uint* keyColors)
        {
            int keyHeight = (int)(keyHeightPercentage * height);
            int bh = (int)(keyHeightPercentage * height * 66.0 / 100.0);

            int i, j;
            for (i = 0; i != 75; ++i) // 绘制所有白键. 参见 Global.DrawMap. Draws all white keys.
            {
                j = DrawMap[i];
                canvas.FillRectangle(keyX[j], 0, keyWidth[j], keyHeight, keyColors[j]);
                canvas.DrawRectangle(keyX[j], 0, keyWidth[j] + 1, keyHeight, 0xFF000000);
            }
            int diff = keyHeight - bh;
            for (; i != 128; ++i) // 绘制所有黑键. Draws all black keys.
            {
                j = DrawMap[i];
                canvas.FillRectangle(keyX[j], diff, keyWidth[j], bh, keyColors[j]); // 重新绘制黑键及其颜色. Draws a black key (See Global.DrawMap).
                canvas.DrawRectangle(keyX[j], diff, keyWidth[j] + 1, bh, 0xFF000000);
            }
        }

        public ColorType OutputColorType => ColorType.RGBA_Int;

        public static readonly short[] DrawMap = [
            0, 2, 4, 5, 7, 9, 11, 12, 14, 16, 17, 19, 21, 23, 24, 26, 28, 29,
            31, 33, 35, 36, 38, 40, 41, 43, 45, 47, 48, 50, 52, 53, 55, 57,
            59, 60, 62, 64, 65, 67, 69, 71, 72, 74, 76, 77, 79, 81, 83, 84,
            86, 88, 89, 91, 93, 95, 96, 98, 100, 101, 103, 105, 107, 108,
            110, 112, 113, 115, 117, 119, 120, 122, 124, 125, 127, 1, 3,
            6, 8, 10, 13, 15, 18, 20, 22, 25, 27, 30, 32, 34, 37, 39, 42, 44,
            46, 49, 51, 54, 56, 58, 61, 63, 66, 68, 70, 73, 75, 78, 80, 82,
            85, 87, 90, 92, 94, 97, 99, 102, 104, 106, 109, 111, 114, 116,
            118, 121, 123, 126
        ];

        public static readonly short[] GenKeyX = [
            0, 12, 18, 33, 36, 54, 66, 72, 85, 90, 105, 108
        ];
    }
}
