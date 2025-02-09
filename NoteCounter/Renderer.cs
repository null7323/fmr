using FMR.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NoteCounter
{
    internal struct FrameData
    {
        public int PolyphonyChanged;
        public long NotePassed;
        public FrameData()
        {
            NotePassed = 0;
            PolyphonyChanged = 0;
        }
    }
    public unsafe class Renderer : IMidiRenderer
    {
        internal bool rendering = false;
        internal int width, height, fps;
        internal Queue<FrameData> frameQueue = new();
        internal SortedDictionary<uint, TickInfo> midiFrames = [];
        internal Tempo[] tempos;

        internal double[] midiFrameTime = [];
        internal uint[] frameTicks = [];
        internal long frameCount;

        internal double currentTime;
        internal long polyphony;
        internal double maxRenderTime;
        internal long totalNotePassed;
        internal long totalNoteCount;
        internal long notePassedLastSecond;
        internal double bpm;
        internal ushort division;

        internal string generalTextFormat;
        internal string bpmTextFormat;
        internal string outputFormat;

        internal Font font;
        internal FontStyle fontStyle;
        internal string fontName = "Arial";
        internal int fontSize;
        internal Bitmap renderTarget;
        internal Graphics graphics;

        internal RenderControl control;
        internal uint frameIndex = 0;

        public int PixelSize => 4;

        public bool Rendering => rendering;

        public bool RenderEnded => !rendering;

        public bool RenderInterrupted => !rendering;

        public double MidiLoaderOffset => 0.0;

        public string Description => "Midi 计数渲染器";

        public ColorType SupportedColorType => ColorType.RGBA_None;

        public ColorType OutputColorType => ColorType.RGBA_Int;

        public Control ControlPanel => control;

        public bool HasWindow => false;

        public string Name => "Note Counter";

        public void BeginRender()
        {
            var values = midiFrames.Values;
            midiFrameTime = new double[values.Count];
            frameTicks = new uint[values.Count];
            {
                int i = 0;
                foreach (TickInfo info in values)
                {
                    midiFrameTime[i++] = info.ActualTime;
                }
                midiFrames.Keys.CopyTo(frameTicks, 0);
            }
            
            currentTime = 0.0;
            frameIndex = 0;
            polyphony = 0;
            totalNotePassed = 0;
            notePassedLastSecond = 0;

            font = new Font(fontName, fontSize, fontStyle);

            frameQueue = new();
            frameQueue.Enqueue(new FrameData()); // keeps queue not empty

            rendering = true;

            renderTarget = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            graphics = Graphics.FromImage(renderTarget);
        }

        public void BindMidiFile(IMidiLoader midiFile)
        {
            if (midiFile is MidiFile file)
            {
                midiFrames = file.FrameInfo;
                maxRenderTime = file.Duration.TotalMicroseconds;
                frameCount = file.FrameInfo.Count;

                tempos = new Tempo[file.Tempos.Count];
                unsafe
                {
                    fixed (Tempo* p = tempos)
                    {
                        file.Tempos.CopyTo(p);
                    }
                }
            }
            
            division = midiFile.Division;
            totalNoteCount = midiFile.NoteCount;
        }

        public unsafe void CopyPixelsTo(IVideoExport videoExport)
        {
            BitmapData data = renderTarget.LockBits(new Rectangle(0, 0, renderTarget.Width, renderTarget.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            IntPtr pFirst = data.Scan0;
            videoExport.WriteFrame((byte*)pFirst);
            renderTarget.UnlockBits(data);
        }

        public void Dispose()
        {
            font.Dispose();
            graphics?.Dispose();
            renderTarget?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void FreeRenderer()
        {
            frameQueue.Clear();
            midiFrameTime = [];
            font.Dispose();
            graphics?.Dispose();
            renderTarget?.Dispose();
            rendering = false;
        }

        public void RenderNextFrame()
        {
            if (currentTime > maxRenderTime + MicrosecondsPerSecond)
            {
                rendering = false;
                return;
            }

            FrameData data = new();
            while (frameIndex < frameCount && midiFrameTime[frameIndex] <= currentTime)
            {
                uint key = frameTicks[frameIndex];
                TickInfo info = midiFrames[key];
                data.NotePassed += info.NotePassedChanged;
                data.PolyphonyChanged += info.PolyphonyChanged;
                frameIndex++;
            }
            polyphony += data.PolyphonyChanged;
            totalNotePassed += data.NotePassed;

            polyphony = Math.Max(0, polyphony);

            frameQueue.Enqueue(data);
            if (frameQueue.Count > fps)
            {
                frameQueue.Dequeue();
            }

            notePassedLastSecond = 0;
            foreach (FrameData frame in frameQueue)
            {
                notePassedLastSecond += frame.NotePassed;
            }

            foreach (Tempo t in tempos)
            {
                if (t.ActualTime > currentTime)
                {
                    break;
                }
                bpm = 60000000.0 / t.Value;
            }

            string outputText = GenerateText();

            graphics.FillRectangle(Black, new Rectangle
            {
                X = 0,
                Y = 0,
                Width = renderTarget.Width,
                Height = renderTarget.Height
            });
            graphics.DrawString(outputText, font, White, new PointF(0.0f, 0.0f));

            currentTime += MicrosecondsPerSecond / fps;
        }

        public void SetColors(RGBAColor[] colors)
        {
            return;
        }

        public void SetColors(Vec4Color[] colors)
        {
            return;
        }

        public void SetRenderConfiguration(RenderConfiguration configuration)
        {
            width = configuration.Width;
            height = configuration.Height;
            fps = configuration.FPS;
        }

        public void SetRenderProperty(string propertyName, string value)
        {
            return;
        }

        protected string GenerateText()
        {
            string notePassed = totalNotePassed.ToString(generalTextFormat);
            string totalNoteCount = this.totalNoteCount.ToString(generalTextFormat);
            string nps = notePassedLastSecond.ToString(generalTextFormat);
            string polyphony = this.polyphony.ToString(generalTextFormat);
            string division = this.division.ToString(generalTextFormat);
            string currentTime = GetTimeSpanExpression(this.currentTime);
            string duration = GetTimeSpanExpression(maxRenderTime);
            string bpm = Math.Round(this.bpm, 3).ToString(bpmTextFormat);
            return outputFormat.Replace("{notePassed}", notePassed).Replace("{totalNoteCount}", totalNoteCount)
                .Replace("{nps}", nps).Replace("{polyphony}", polyphony).Replace("{currentTime}", currentTime)
                .Replace("{duration}", duration).Replace("{bpm}", bpm).Replace("{ppq}", division)
                .Replace("{division}", division);
        }

        protected static string GetTimeSpanExpression(double microseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMicroseconds(microseconds);
            return $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
        }

        private const double MicrosecondsPerSecond = 1000000.0;

        public Renderer()
        {
            fontStyle = FontStyle.Regular;
            generalTextFormat = string.Empty;
            bpmTextFormat = "0.00";
            tempos = [];
            outputFormat = string.Empty;
            control = new(this);
        }

        private static readonly SolidBrush White = new(Color.White);
        private static readonly SolidBrush Black = new(Color.Black);
    }
}
