using FMR.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR
{
    internal enum TaskState
    {
        Error = 0b1,
        Running = 0b10,
        Success = 0b100,
        Stopped = 0b1000
    }
    internal unsafe class RenderHost
    {
        internal double loaderOffset;
        internal double currentTime;
        internal long totalFramesRendered;
        internal int frameSizeInBytes;
        internal int fps;
        internal double nonNegativeOffset;
        internal ProgressReporter reporter;

        internal IMidiLoader midi;
        internal IMidiRenderer renderer;
        internal IVideoExport export;

        internal bool requireStop;
        internal int width;
        internal int height;

        public delegate void PixelsReceiver(byte* pixels, int pixelSize, int pixelCount);
        public delegate void ProgressReporter(double progress, double frameSpeed, double averageSpeed, long notesOnScreen, TaskState state, string message);

        public RenderHost(IMidiLoader midi, IMidiRenderer renderer, IVideoExport export, ColorManager manager, ProgressReporter reporter, RenderConfiguration config)
        {
            loaderOffset = renderer.MidiLoaderOffset;
            currentTime = Math.Min(0.0, -loaderOffset);
            nonNegativeOffset = Math.Max(0.0, loaderOffset);

            totalFramesRendered = 0;
            frameSizeInBytes = config.Width * config.Height * renderer.PixelSize;

            width = config.Width;
            height = config.Height;

            this.reporter = reporter;

            this.midi = midi;
            this.renderer = renderer;
            this.export = export;
            fps = config.FPS;

            requireStop = false;

            renderer.SetRenderConfiguration(config);
            renderer.BindMidiFile(midi);
            
            export.SetVideoFPS(config.FPS);
            export.SetFrameSize(config.Width, config.Height, renderer.PixelSize);

            switch (renderer.SupportedColorType)
            {
                case ColorType.RGBA_Int:
                    renderer.SetColors(manager.Colors);
                    break;
                case ColorType.RGBA_Float:
                    renderer.SetColors(manager.Vec4Colors);
                    break;
                case ColorType.RGBA_Both:
                    renderer.SetColors(manager.Colors);
                    break;
            }
            renderer.BeginRender();
            export.BeginWrite();

            if (renderer.OutputColorType == ColorType.RGBA_None || renderer.OutputColorType == ColorType.RGBA_Both)
            {
                ThrowHelper.Throw($"渲染器的输出颜色类型错误:\n只能为{ColorType.RGBA_Int}或{ColorType.RGBA_Float}");
            }
            if (export.SupportedColorType == ColorType.RGBA_None)
            {
                ThrowHelper.Throw($"视频输出的支持颜色类型错误:\n不能为{ColorType.RGBA_None}");
            }
        }

        public void Render()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            double secondsPerFrame = 1.0 / fps;
            totalFramesRendered = 0;
            try
            {
                while (!renderer.RenderEnded && (!requireStop))
                {
                    long frameBeginMilliseconds = stopwatch.ElapsedMilliseconds;

                    midi.ParseTo(currentTime + nonNegativeOffset);

                    renderer.RenderNextFrame();
                    renderer.CopyPixelsTo(export);

                    currentTime += secondsPerFrame;
                    ++totalFramesRendered;

                    long frameEndMilliseconds = stopwatch.ElapsedMilliseconds;

                    double frameSpeed = 1000.0 / (frameEndMilliseconds - frameBeginMilliseconds);
                    double avgSpeed = totalFramesRendered / (frameEndMilliseconds / 1000.0);
                    double estimatedProgress = currentTime / midi.Duration.TotalSeconds;

                    reporter(estimatedProgress, frameSpeed, avgSpeed, renderer.NotesOnScreen, TaskState.Running, string.Empty);
                }
            }
            catch (Exception ex)
            {
                reporter(0.0, 0.0, 0.0, 0, TaskState.Error, ex.Message);
                return;
            }
            try
            {
                renderer.FreeRenderer();
                export.EndWrite();
            }
            catch
            {

            }
            if (requireStop)
            {
                reporter(currentTime / midi.Duration.TotalSeconds, 0.0, totalFramesRendered / (stopwatch.ElapsedMilliseconds / 1000.0), 0, TaskState.Stopped, string.Empty);
            }
            else
            {
                reporter(1.0, 0.0, totalFramesRendered / (stopwatch.ElapsedMilliseconds / 1000.0), 0, TaskState.Success, string.Empty);
            }
            GC.Collect();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task RenderAsync()
        {
            return Task.Run(Render);
        }

        public void Stop()
        {
            requireStop = true;
        }
    }
}
