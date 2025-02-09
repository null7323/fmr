using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FMR.Core;
using FMR.Core.GLRender;

namespace TexturedRenderer
{
    public class Renderer : IMidiRenderer
    {
        public int PixelSize => 4;

        public bool Rendering => rendering;

        public bool RenderEnded => !rendering;

        public bool RenderInterrupted => !rendering;

        public double MidiLoaderOffset => 0.0;

        public string Description => "表示支持自定义纹理的渲染器.";

        public ColorType SupportedColorType => ColorType.RGBA_Int;

        public ColorType OutputColorType => ColorType.RGBA_Int;

        public Control ControlPanel => throw new NotImplementedException();

        public bool HasWindow => true;

        public string Name => "Textured Renderer";

        public void BeginRender()
        {
            
            context = new Context(previewWidth, previewHeight);
            
        }

        public void BindMidiFile(IMidiLoader midiFile)
        {
            if (midiFile is MidiFile file)
            {
                this.midiFile = file;
            }
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

        internal bool rendering;
        internal int width, height;
        internal int previewWidth, previewHeight;

        internal MidiFile midiFile;
        internal Context context;
    }
}
