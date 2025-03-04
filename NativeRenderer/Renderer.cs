using FMR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NativeRenderer
{
    public static class NativeRendererLoader
    {
        internal static string[] dllNames;

    }
    public unsafe struct NativeRenderer
    {
        public delegate* unmanaged<void> RenderNextFrame;

        public delegate* unmanaged<bool> IsRendering;

        public delegate* unmanaged<int> SupportedColorType;

        public delegate* unmanaged<byte*> GetRenderedFrame;

        public delegate* unmanaged<void> FreeRenderer;

        public delegate* unmanaged<void*, int, int, void> SetColors;

        public delegate* unmanaged<int, int, int, void> SetRenderConfiguration;

        public delegate* unmanaged<int> GetNumberOfAvailableOptions;
    }

    public class Renderer : IMidiRenderer
    {
        public bool Rendering => throw new NotImplementedException();

        public bool RenderEnded => throw new NotImplementedException();

        public bool RenderInterrupted => throw new NotImplementedException();

        public double MidiLoaderOffset => throw new NotImplementedException();

        public string Description => "以 P/Invoke 方式调用本机代码渲染.";

        public ColorType SupportedColorType => throw new NotImplementedException();

        public Control ControlPanel => control;

        public bool HasWindow => throw new NotImplementedException();

        public long NotesOnScreen => throw new NotImplementedException();

        public string Name => "P/Invoke";

        public void BeginRender()
        {
            throw new NotImplementedException();
        }

        public void BindMidiFile(IMidiLoader midiFile)
        {
            throw new NotImplementedException();
        }

        public void CopyPixelsTo(IVideoExport videoExport)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void FreeRenderer()
        {
            throw new NotImplementedException();
        }

        public void RenderNextFrame()
        {
            throw new NotImplementedException();
        }

        public void SetColors(RGBAColor[] colors)
        {
            throw new NotImplementedException();
        }

        public void SetColors(Vec4Color[] colors)
        {
            throw new NotImplementedException();
        }

        public void SetRenderConfiguration(RenderConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public void SetRenderProperty(string propertyName, string value)
        {
            throw new NotImplementedException();
        }

        public Renderer()
        {
            control = new(this);
        }

        internal RenderControl control;
    }
}
