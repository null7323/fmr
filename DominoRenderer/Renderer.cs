using FMR.Core;
using FMR.Core.Collections;
using FMR.Core.SDLRender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DominoRenderer
{
    internal struct BeatSeparator
    {
        public bool IsBarSeparator;
        public uint Tick;
    }
    public unsafe sealed class Renderer : IMidiRenderer
    {
        internal const int keyBufferRectCount = 16384 * 3 / 2;
        public void BeginRender()
        {
            context = new Context("Domino Renderer", previewWidth, previewHeight);
            renderTarget = Texture.CreateTarget(context, width, height);
            keyboard = Texture.CreateFrom(context, assetManager.GetPathOf("Piano_10.png"));

            frameBuffer = Surface.Create(width, height);
            finalCompositeBuffer = Surface.Create(width, height);

            context.SetBlendMode(BlendMode.None);

            tickLeft = 0.0;
            tickRight = division * beatsPerScreen;
            currentTime = 0.0;
            currentTick = 0.0;

            noteIndices = new long[128];
            Array.Fill(noteIndices, 0);

            vertexIndices = UnsafeMemory.Allocate<int>(keyBufferRectCount * 6);
            for (int i = 0; i < keyBufferRectCount; i++)
            {
                vertexIndices[i * 6] = i * 4 + 0;
                vertexIndices[i * 6 + 1] = i * 4 + 1;
                vertexIndices[i * 6 + 2] = i * 4 + 2;
                vertexIndices[i * 6 + 3] = i * 4 + 1;
                vertexIndices[i * 6 + 4] = i * 4 + 2;
                vertexIndices[i * 6 + 5] = i * 4 + 3;
            }


            keyVertexBuffer = new RectVertexBuffer[128];
            for (int i = 0; i != 128; i++)
            {
                keyVertexBuffer[i] = new RectVertexBuffer(keyBufferRectCount * 4, vertexIndices);
            }

            rendering = true;
            frameDrawn = false;
        }

        public void BindMidiFile(IMidiLoader midiFile)
        {
            if (midiFile is MidiFile file)
            {
                division = file.Division;

                #region Time Signature and Bar Division
                beatSeparators.Clear();
                {
                    long signatureIter = file.TimeSignatures.Count - 1;
                    long i = 0;
                    double ticksPerBeat;
                    int beatCount;
                    int beatsPerBar;
                    while (i < signatureIter)
                    {
                        ref TimeSignature first = ref file.TimeSignatures[i];
                        ref TimeSignature next = ref file.TimeSignatures[i + 1];

                        double startTick = first.Tick;
                        double endTick = next.Tick;

                        ticksPerBeat = division * 4.0 / first.Denominator;

                        beatsPerBar = first.Numerator;
                        beatCount = 0;
                        while (startTick < endTick)
                        {
                            bool isBarDivider = (beatCount % beatsPerBar == 0);

                            beatSeparators.Add(new BeatSeparator
                            {
                                IsBarSeparator = isBarDivider,
                                Tick = (uint)startTick
                            });

                            ++beatCount;
                            startTick += ticksPerBeat;
                        }
                        ++i;
                    }
                    TimeSignature last = file.TimeSignatures[i];
                    double tick = last.Tick;
                    uint end = file.MidiTime + division * 128;
                    ticksPerBeat = division * 4.0 / last.Denominator;
                    beatCount = 0;
                    beatsPerBar = last.Numerator;
                    while (tick < end)
                    {
                        bool isBarDivider = beatCount % beatsPerBar == 0;

                        beatSeparators.Add(new BeatSeparator
                        {
                            IsBarSeparator = isBarDivider,
                            Tick = (uint)tick
                        });

                        ++beatCount;
                        tick += ticksPerBeat;
                    }
                }
                #endregion

                maxMidiTick = file.MidiTime;
                tempos = file.Tempos;
                keyNotes = file.Notes;
            }
        }

        public unsafe void CopyPixelsTo(IVideoExport videoExport)
        {
            videoExport.WriteFrame((RGBAColor*)finalCompositeBuffer.Data);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void FreeRenderer()
        {
            if (keyVertexBuffer.Length != 0)
            {
                for (int i = 0; i < keyVertexBuffer.Length; ++i)
                {
                    keyVertexBuffer[i].Dispose();
                }
            }
            if (vertexIndices != null)
            {
                UnsafeMemory.Free(vertexIndices);
                vertexIndices = null;
            }
            frameBuffer.Dispose();
            finalCompositeBuffer.Dispose();

            tempos = new();
            keyNotes = [];
            keyVertexBuffer = [];

            keyboard.Dispose();
            renderTarget.Dispose();
            context.Dispose();
        }

        public void RenderNextFrame()
        {
            if (tickLeft > maxMidiTick)
            {
                rendering = false;
                return;
            }

            context.SetRenderTarget(renderTarget);

            int contextWidth = context.Width;
            int contextHeight = context.Height;
            int stride = finalCompositeBuffer.Pitch / 4;
            RGBAColor* finalCompositeBufferPixels = (RGBAColor*)finalCompositeBuffer.Data;

            for (long i = 0, size = tempos.Count; i < size; ++i)
            {
                ref Tempo t = ref tempos[i];
                if (t.ActualTime > currentTime)
                {
                    break;
                }
                currentTick = (currentTime - t.ActualTime) * division / t.Value + t.Tick;
            }

            if (!(currentTick >= tickLeft && currentTick <= tickRight && frameDrawn))
            {
                while (!((tickLeft <= currentTick) && (currentTick <= tickRight)))
                {
                    tickLeft = tickRight;
                    tickRight = tickLeft + beatsPerScreen * division;
                }

                // draw frame

                context.Clear(RGBAColor.White);

                RenderKeyboard();
                RenderBackgroundKeyBars();
                RenderBeatSeparator();
                RenderNotes();

                using Surface frameData = context.ReadPixels();
                frameBuffer.UpdateSurface(frameData);

                // generate a preview
                Surface window = context.GetWindowSurface();
                frameBuffer.Blit(window);
                context.UpdateWindowSurface();

                frameBuffer.CopyPixelsTo(finalCompositeBuffer);
                frameDrawn = true;
            }
            else
            {
                int realX = lastRevColorX;
                int realWidth = lastRevColorWidth;
                unsafe
                {
                    RGBAColor* firstPixelInLine = finalCompositeBufferPixels;
                    firstPixelInLine += realX;
                    for (int i = 0; i < contextHeight; i++)
                    {
                        RGBAColor* ptr = firstPixelInLine;
                        for (int w = 0; w < realWidth; w++)
                        {
                            ptr->R = (byte)(255 - ptr->R);
                            ptr->G = (byte)(255 - ptr->G);
                            ptr->B = (byte)(255 - ptr->B);
                            ++ptr;
                        }
                        firstPixelInLine += stride;
                    }
                }
            }

            #region Color Reverse
            {

                // logic coordinates
                float kbWidth = (float)keyboardWidth;
                float x = (float)((currentTick - tickLeft) / (tickRight - tickLeft) * (1 - kbWidth)) + kbWidth;
                float barWidth = 2f / context.Width;

                int realX = (int)(x * contextWidth);
                int realWidth = (int)(barWidth * contextWidth);

                if (realX + realWidth > contextWidth)
                {
                    realWidth = contextWidth - realX;
                }

                unsafe
                {
                    RGBAColor* lineFirst = finalCompositeBufferPixels;
                    lineFirst += realX;
                    for (int i = 0; i < contextHeight; i++)
                    {
                        RGBAColor* ptr = lineFirst;
                        for (int w = 0; w < realWidth; w++)
                        {
                            ptr->R = (byte)(255 - ptr->R);
                            ptr->G = (byte)(255 - ptr->G);
                            ptr->B = (byte)(255 - ptr->B);
                            ++ptr;
                        }
                        lineFirst += stride;
                    }
                }

                lastRevColorX = realX;
                lastRevColorWidth = realWidth;
            }
            #endregion
            context.SetRenderTarget(null);
            context.Clear(RGBAColor.White);

            // context.Present();
            context.HandleEvents();
            currentTime += 1000000.0 / fps;
        }

        private void RenderBeatSeparator()
        {
            Vec4Color grey = new(177f / 255f, 177f / 255f, 177f / 255f, 1f);
            float halfSeparatorWidth = 1.0f / context.Width;
            float halfBarSeparatorWidth = 1.0f * halfSeparatorWidth; // 历史遗留.
            float kbWidth = (float)keyboardWidth;

            int leftBeatIndex = 0;
            int rightBeatIndex = 0;
            foreach (var beat in beatSeparators)
            {
                if (beat.Tick < tickLeft)
                {
                    leftBeatIndex++;
                }
                if (beat.Tick < tickRight)
                {
                    rightBeatIndex++;
                }
                else
                {
                    break;
                }
            }

            using VertexBuffer buffer = new(256 * 6, context);
            for (int i = leftBeatIndex; i <= rightBeatIndex; i++)
            {
                BeatSeparator separator = beatSeparators[i];
                float x = (float)((separator.Tick - tickLeft) / (tickRight - tickLeft)) * (1.0f - kbWidth) + kbWidth;

                if (separator.IsBarSeparator)
                {
                    var (V1, V2, V3, V4, V5, V6) = VertexHelper.MakeVertices(new Rect(
                        x - halfBarSeparatorWidth, 0.0f, 2f * halfBarSeparatorWidth, 1.0f), Vec4Color.Black);
                    buffer.PushRect(V1, V2, V3, V4, V5, V6);
                }
                else
                {
                    var (V1, V2, V3, V4, V5, V6) = VertexHelper.MakeVertices(new Rect(
                        x - halfSeparatorWidth, 0.0f, 2f * halfSeparatorWidth, 1.0f), grey);
                    buffer.PushRect(V1, V2, V3, V4, V5, V6);
                }
            }
            buffer.PushToContext();
        }

        private unsafe void RenderNotes()
        {
            notesOnScreen = 0;
            long* notesOnScreenEachKey = stackalloc long[128];
            Parallel.For(0, 128, (i) =>
            {
                long noteIndex = noteIndices[i];
                long nc = 0;

                Note* first = keyNotes[i].First();
                Note* ptr = first + noteIndex;
                Note* border = first + keyNotes[i].Count;

                int colorCount = trackColors.Length;
                RectVertexBuffer buffer = keyVertexBuffer[i];

                float keyHeight = 1.0f / 128f;

                float keyY = 1.0f - (i + 1) * (1.0f / 128.0f);
                float keyBottomY = keyY + 1.0f / 128.0f;

                bool flag = true;

                float kbWidth = (float)keyboardWidth;
                float drawAreaLength = 1.0f - kbWidth;

                float lengthPerTick = drawAreaLength / (float)(this.tickRight - this.tickLeft);
                float widthBorderThickness = 1.0f / context.Width;
                float heightBorderThickness = 1.0f / context.Height;

                float minimumDrawThreshold = 2.0f * widthBorderThickness;

                float tickLeft = (float)this.tickLeft;
                float tickRight = (float)this.tickRight;
                uint ceilingTickRight = (uint)Math.Ceiling(tickRight);

                while (ptr->Start < ceilingTickRight)
                {
                    if (ptr == border)
                    {
                        break;
                    }
                    float end = ptr->End;

                    if (end > tickLeft)
                    {
                        ++nc;
                        float start = ptr->Start;
                        if (flag)
                        {
                            noteIndex = ptr - first;
                            flag = false;
                        }

                        float noteX = kbWidth;
                        float noteRightX = 1.0f;
                        if (end < tickRight)
                        {
                            noteRightX -= (tickRight - end) * lengthPerTick;
                        }
                        if (start > tickLeft)
                        {
                            noteX += (start - tickLeft) * lengthPerTick;
                        }

                        // 求出Note在画面的长度.
                        float noteDrawLength = noteRightX - noteX;

                        int colorIndex = ptr->Track % colorCount;

                        VertexHelper.MakeVerticesInRectBuffer(
                                noteX, keyY, noteRightX, keyBottomY, in blendedColors[colorIndex],
                                ref buffer, context);

                        if (noteDrawLength > minimumDrawThreshold)
                        {
                            VertexHelper.MakeVerticesInRectBuffer(
                                noteX + widthBorderThickness, keyY + heightBorderThickness,
                                noteRightX - widthBorderThickness, keyBottomY - heightBorderThickness,
                                Vec4Color.BlendColor(in trackColors[colorIndex], in Vec4Color.White, ptr->Velocity / 127.0f),
                                ref buffer, context);
                        }
                    }
                    ++ptr;
                }
                buffer.PushToContext(context);
                noteIndices[i] = noteIndex;

                notesOnScreenEachKey[i] = nc;
            });
            for (int i = 0; i < 128; i++)
            {
                notesOnScreen += notesOnScreenEachKey[i];
            }
        }

        private const float singleKbHeight = 12.0f / 128.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RenderKeyboard()
        {
            float kbWidth = (float)keyboardWidth;
            const float singleTexHeight = singleKbHeight;

            Rect* rects = stackalloc Rect[11];
            Vertex* vertices = stackalloc Vertex[66];
            for (int i = 0; i < 11; i++)
            {
                rects[i] = new Rect(0.0f, 1.0f - (i + 1) * singleTexHeight, kbWidth, singleTexHeight);
            }
            
            VertexHelper.MakeVertices(rects, 11, vertices);
            context.DrawVertices(vertices, 66, keyboard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RenderBackgroundKeyBars()
        {
            const float singleBarHeight = 1.0f / 128.0f;
            float kbWidth = (float)keyboardWidth;
            Vec4Color whiteKeyBackground = (Vec4Color)new RGBAColor(255, 255, 255, 255);
            Vec4Color blackKeyBackground = (Vec4Color)new RGBAColor(237, 243, 254, 255);
            Vec4Color separator = (Vec4Color)new RGBAColor(198, 218, 244, 255);

            Vertex* keyBackgroundVertices = stackalloc Vertex[128 * 6 + 127 * 6 + 10 * 6];

            Vertex* copyBegin = keyBackgroundVertices;
            for (int i = 0; i < 128; i++)
            {
                VertexHelper.MakeVertices(
                    new Rect(kbWidth, 1.0f - (i + 1) * singleBarHeight, 1.0f - kbWidth, singleBarHeight),
                    Note.IsBlackKey(i) ? blackKeyBackground : whiteKeyBackground, copyBegin);

                copyBegin += 6;
            }
            float halfSeparatorHeight = 0.6f / context.Height;
            for (int i = 0; i < 127; i++)
            {
                VertexHelper.MakeVertices(
                    new Rect(kbWidth, 1.0f - (i + 1) * singleBarHeight - halfSeparatorHeight, 1.0f - kbWidth, 2 * halfSeparatorHeight),
                    separator, copyBegin);
                copyBegin += 6;
            }
            float octaveSeparatorHeight = 1f / context.Height;
            for (int i = 1; i < 11; i++)
            {
                VertexHelper.MakeVertices(
                    new Rect(kbWidth, 1.0f - i * singleKbHeight - octaveSeparatorHeight / 2.0f, 1.0f - kbWidth, octaveSeparatorHeight),
                    in Vec4Color.Black, copyBegin);
                copyBegin += 6;
            }
            context.DrawVertices(keyBackgroundVertices, 128 * 6 + 127 * 6 + 10 * 6);
        }

        public void SetColors(RGBAColor[] colors)
        {
            return;
        }

        public void SetColors(Vec4Color[] colors)
        {
            if (!overrideColorSettings)
            {
                trackColors = colors;

                blendedColors = new Vec4Color[trackColors.Length];
                trackColors.CopyTo(blendedColors, 0);

                for (int i = 0; i < blendedColors.Length; i++)
                {
                    blendedColors[i] = Vec4Color.BlendColor(in blendedColors[i], in Vec4Color.Black, 0.5f);
                }
            }
            else
            {
                trackColors = new Vec4Color[8];
                blendedColors = new Vec4Color[8];

                for (int i = 0; i < 8; i++)
                {
                    int k = overrideColorOrder[i];
                    trackColors[i] = DominoColors[k];
                    blendedColors[i] = DominoEdgeColors[k];
                }
            }
        }

        public void SetRenderConfiguration(RenderConfiguration configuration)
        {
            width = configuration.Width; height = configuration.Height;
            fps = configuration.FPS;
        }

        public void SetRenderProperty(string propertyName, string value)
        {
            throw new NotImplementedException();
        }

        public Renderer()
        {
            overrideColorSettings = false;
            isFrameBufferAvailable = false;
            beatsPerScreen = 8.0;
            assetManager = new(typeof(Renderer).Assembly);
            control = new RenderControl(this);
            beatSeparators = [];
        }

        internal RenderControl control;
        internal Vec4Color[] trackColors;
        internal Vec4Color[] blendedColors;
        internal UnmanagedList<Note>[] keyNotes;
        internal UnmanagedList<Tempo> tempos;
        internal List<BeatSeparator> beatSeparators;
        internal int width, height;
        internal int fps;
        internal uint maxMidiTick;
        internal double keyboardWidth = 0.075;
        internal double tickLeft;
        internal double tickRight;
        internal uint division;
        internal double beatsPerScreen;
        internal double currentTick;
        internal long[] noteIndices;
        internal bool isFrameBufferAvailable;
        internal RectVertexBuffer[] keyVertexBuffer = [];
        internal Context context;
        internal Texture keyboard;
        internal Texture renderTarget;
        internal int lastRevColorX;
        internal int lastRevColorWidth;
        internal long notesOnScreen;

        internal Surface frameBuffer;
        internal Surface finalCompositeBuffer;

        internal ResourceManager assetManager;

        public int PixelSize => 4;

        public bool Rendering => rendering;

        public bool RenderEnded => !rendering;

        public bool RenderInterrupted => !rendering;

        public double MidiLoaderOffset => 0.0;

        public string Description => "Domino 风格渲染器.";

        public ColorType SupportedColorType => ColorType.RGBA_Float;

        public Control ControlPanel => control;

        public bool HasWindow => true;

        public string Name => "Domino";

        public long NotesOnScreen => notesOnScreen;

        internal bool rendering = false;
        internal int previewWidth = 1600;
        internal int previewHeight = 900;
        internal bool frameDrawn = false;
        internal double currentTime;
        internal int* vertexIndices = null;

        internal bool overrideColorSettings;

        internal int[] overrideColorOrder = [0, 1, 2, 3, 4, 5, 6, 7];

        public static readonly Vec4Color[] DominoColors = [
            new(1f, 165f / 255f, 165f / 255f, 1f),
            new(1f, 189f / 255f, 65f / 255f, 1f),
            new(1f, 1f, 45f / 255f, 1f),
            new(141f / 255f, 1f, 85f / 255f, 1f),
            new(85f / 255f, 1f, 226f / 255f, 1f),
            new(127f / 255f, 180f / 255f, 1f, 1f),
            new(215f / 255f, 175f / 255f, 1f, 1f),
            new(1f, 145f / 255f, 251f / 255f, 1f)
        ];

        public static readonly Vec4Color[] DominoEdgeColors = [
            new(120f / 255f, 0f, 0f, 1f),
            new(120f / 255f, 74f / 255f, 0f, 1f),
            new(120f / 255f, 120f / 255f, 0f, 1f),
            new(40f / 255f, 120f / 255f, 0f, 1f),
            new(0f, 120f / 255f, 100, 1f),
            new(0f, 0f, 128f / 255f, 1f),
            new(60f / 255f, 0f, 120f / 255f, 1f),
            new(120f / 255f, 0f, 116f / 255f, 1f)
        ];

    }
}
