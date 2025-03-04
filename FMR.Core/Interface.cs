using OpenCvSharp.XPhoto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FMR.Core
{
#pragma warning disable CS1591
    /// <summary>
    /// 表示包装渲染后导出的像素信息的结构.
    /// </summary>
    public unsafe struct RenderedPixels
    {
        internal byte* pixels;
        internal bool requiresDispose;
    }

    /// <summary>
    /// 表示颜色类型.
    /// </summary>
    public enum ColorType : int
    {
        /// <summary>
        /// 指示不支持任何颜色类型.
        /// </summary>
        RGBA_None = 0,
        /// <summary>
        /// 指示颜色类型为<see cref="RGBAColor"/>.
        /// </summary>
        RGBA_Int = 0b1,
        /// <summary>
        /// 指示颜色类型为<see cref="Vec4Color"/>.
        /// </summary>
        RGBA_Float = 0b10,
        /// <summary>
        /// 指示颜色类型为<see cref="RGBAColor"/>或<see cref="Vec4Color"/>.
        /// </summary>
        RGBA_Both = RGBA_Int | RGBA_Float
    }
    [StructLayout(LayoutKind.Sequential)]
    public record struct RenderConfiguration
    {
        public int FPS;
        public int Width;
        public int Height;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBAColor
    {
        public byte R, G, B, A;
        public RGBAColor()
        {
            R = G = B = A = 0;
        }
        public RGBAColor(byte r, byte g, byte b, byte a)
        {
            R = r; G = g; B = b; A = a;
        }
        /// <summary>
        /// 将当前颜色转化为<see cref="Vec4Color"/>类型.
        /// </summary>
        /// <returns>以<see cref="Vec4Color"/>表示的颜色.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec4Color AsVecColor()
        {
            return new Vec4Color
            {
                R = R / 255.0f,
                G = G / 255.0f,
                B = B / 255.0f,
                A = A / 255.0f
            };
        }
        public static explicit operator Vec4Color(RGBAColor color)
        {
            return color.AsVecColor();
        }
        public static explicit operator uint(RGBAColor color)
        {
            unsafe
            {
                return *(uint*)&color;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGBAColor BlendColor(RGBAColor left, RGBAColor right, double leftWeight)
        {
            double rightWeight = 1.0 - leftWeight;
            return new RGBAColor(
                (byte)(left.R * leftWeight + right.R * rightWeight),
                (byte)(left.G * leftWeight + right.G * rightWeight),
                (byte)(left.B * leftWeight + right.B * rightWeight),
                (byte)(left.A * leftWeight + right.A * rightWeight));
        }
        public static readonly RGBAColor White = new(255, 255, 255, 255);
        public static readonly RGBAColor Black = new(0, 0, 0, 255);
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4Color
    {
        public float R, G, B, A;
        public Vec4Color()
        {
            R = 0f; G = 0f; B = 0f; A = 0f;
        }

        public Vec4Color(float value)
        {
            R = G = B = A = value;
        }
        public Vec4Color(float r, float g, float b, float a)
        {
            R = r; G = g; B = b; A = a;
        }
        /// <summary>
        /// 将当前颜色转化为<see cref="RGBAColor"/>类型.
        /// </summary>
        /// <returns>以<see cref="RGBAColor"/>表示的颜色.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RGBAColor AsRGBAColorInt()
        {
            return new RGBAColor
            {
                R = (byte)(R * 255),
                G = (byte)(G * 255),
                B = (byte)(B * 255),
                A = (byte)(A * 255)
            };
        }
        public static explicit operator RGBAColor(in Vec4Color color)
        {
            return color.AsRGBAColorInt(); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4Color BlendColor(in Vec4Color left, in Vec4Color right, float leftWeight)
        {
            float rightWeight = 1.0f - leftWeight;
            return new Vec4Color(
                left.R * leftWeight + right.R * rightWeight,
                left.G * leftWeight + right.G * rightWeight,
                left.B * leftWeight + right.B * rightWeight,
                left.A * leftWeight + right.A * rightWeight);
        }
        public static readonly Vec4Color White = new(1f);
        public static readonly Vec4Color Black = new(0f, 0f, 0f, 1f);
    }

    public interface INamedObject
    {
        /// <summary>
        /// 当前实例的名称.
        /// </summary>
        public string Name { get; }
    }
#pragma warning restore CS1591
    /// <summary>
    /// 表示一个可复用的Midi加载器接口.
    /// </summary>
    public interface IMidiLoader : IDisposable, INamedObject
    {
        /// <summary>
        /// 指示加载的Midi事件是否用Tick为单位表示
        /// </summary>
        public bool TickBased { get; }

        /// <summary>
        /// 指示加载的Midi事件是否用具体时间表示
        /// </summary>
        public bool TimeBased { get; }

        /// <summary>
        /// 要求Midi加载器必须加载到给定的具体时间
        /// </summary>
        /// <param name="time">目标时间, 单位微秒.</param>
        public void ParseTo(double time);

        /// <summary>
        /// 对Midi文件预处理. 完成此步骤必须给出<see cref="NoteCount"/>, <see cref="Division"/>, <see cref="Duration"/>属性, 且<see cref="PreLoaded"/>属性必须变为<see langword="true"/>.
        /// </summary>
        public void PreLoad();

        /// <summary>
        /// 打开Midi文件.
        /// </summary>
        /// <param name="path"></param>
        public void OpenFile(string path);

        /// <summary>
        /// 指示当前指示器是否具有流式加载的能力.
        /// </summary>
        public bool CanParseOnRender { get; }

        /// <summary>
        /// 判断Midi是否完全加载完成.
        /// </summary>
        public bool Parsed { get; }

        /// <summary>
        /// 判断Midi是否预处理完成
        /// </summary>
        public bool PreLoaded { get; }

        /// <summary>
        /// 音轨数.
        /// </summary>
        public int TrackCount { get; }

        /// <summary>
        /// 分解度.
        /// </summary>
        public ushort Division { get; }

        /// <summary>
        /// 音符总数.
        /// </summary>
        public long NoteCount { get; }

        /// <summary>
        /// Midi 总时长.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// 清除所有数据, 并恢复到未加载Midi的状态.
        /// </summary>
        public void Reset();
    }

    /// <summary>
    /// 表示一个可复用Midi渲染器接口.
    /// </summary>
    public unsafe interface IMidiRenderer : IDisposable, INamedObject
    {
        /// <summary>
        /// 设置渲染器的某项属性，这些属性由Renderer确定.
        /// </summary>
        /// <param name="propertyName">属性名.</param>
        /// <param name="value">属性值.</param>
        public void SetRenderProperty(string propertyName, string value);

        /// <summary>
        /// 设置Renderer的最基本配置.
        /// </summary>
        /// <param name="configuration">渲染基本配置.</param>
        public void SetRenderConfiguration(RenderConfiguration configuration);

        /// <summary>
        /// 渲染下一帧.
        /// </summary>
        public void RenderNextFrame();

        /// <summary>
        /// 将渲染的结果复制至指定的输出设备.
        /// </summary>
        /// <param name="videoExport">输出帧的设备.</param>
        public void CopyPixelsTo(IVideoExport videoExport);

        /// <summary>
        /// 准备渲染，此步骤将完成渲染的准备内容.
        /// </summary>
        public void BeginRender();

        /// <summary>
        /// 设置渲染器的Midi文件. 这文件会在<see cref="BeginRender"/>前调用.
        /// </summary>
        /// <param name="midiFile">要设置的Midi文件. 这文件的类型总是与该渲染器属于同一模块.</param>
        public void BindMidiFile(IMidiLoader midiFile);

        /// <summary>
        /// 判断当然渲染是否在进行.
        /// </summary>
        public bool Rendering { get; }

        /// <summary>
        /// 判断整个渲染过程是否结束.
        /// </summary>
        public bool RenderEnded { get; }

        /// <summary>
        /// 判断渲染是否被打断. 此值为<see langword="true"/>时, <see cref="RenderEnded"/>也必须为<see langword="true"/>.
        /// </summary>
        public bool RenderInterrupted { get; }

        /// <summary>
        /// 释放Renderer的内容.
        /// </summary>
        /// <remarks>
        /// 注意: 调用此方法不代表此<see cref="IMidiRenderer"/>实例被销毁; 当<see cref="BeginRender"/>被调用时,
        /// 此<see cref="IMidiRenderer"/>实例仍应能进行下一次渲染.
        /// </remarks>
        public void FreeRenderer();

        /// <summary>
        /// Midi加载器应加载的时间位移，单位为微秒.
        /// </summary>
        public double MidiLoaderOffset { get; }

        /// <summary>
        /// 当前Renderer的描述.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 设置渲染的颜色.
        /// </summary>
        /// <param name="colors">包含一定颜色的数组.</param>
        public void SetColors(RGBAColor[] colors);

        /// <summary>
        /// 设置渲染的颜色.
        /// </summary>
        /// <param name="colors">包含一定颜色的数组.</param>
        public void SetColors(Vec4Color[] colors);

        /// <summary>
        /// 获取当前渲染器支持的颜色类型. 调用方依据此值选择<see cref="SetColors(RGBAColor[])"/>或<see cref="SetColors(Vec4Color[])"/>进行调用.
        /// </summary>
        public ColorType SupportedColorType { get; }

        /// <summary>
        /// 表示一个用户控制的界面. 这会在模块设置界面启用.
        /// </summary>
        public Control ControlPanel { get; }

        /// <summary>
        /// 判断该渲染器是否提供窗口.
        /// </summary>
        public bool HasWindow { get; }

        /// <summary>
        /// 获取屏幕上绘制的音符数.
        /// </summary>
        public long NotesOnScreen { get; }
    }

    /// <summary>
    /// 表示将帧导出为视频的接口.
    /// </summary>
    public unsafe interface IVideoExport : IDisposable, INamedObject
    {
        /// <summary>
        /// 设置导出路径.
        /// </summary>
        /// /// <remarks>
        /// 只能在<see cref="BeginWrite"/>之前调用.
        /// </remarks>
        /// <param name="path">视频文件路径.</param>
        public void SetExportPath(string path);

        /// <summary>
        /// 将视频的<paramref name="propertyName"/>属性设置为<paramref name="value"/>，这属性由<see cref="IVideoExport"/>的实现提供.
        /// </summary>
        /// <remarks>
        /// 只能在<see cref="BeginWrite"/>之前调用.
        /// </remarks>
        /// <param name="propertyName">属性名.</param>
        /// <param name="value">属性值.</param>
        public void SetVideoProperty(string propertyName, string value);

        /// <summary>
        /// 向<see cref="IVideoExport"/>对象写入一帧.
        /// </summary>
        /// <remarks>
        /// 必须在<see cref="BeginWrite"/>之后调用.
        /// </remarks>
        /// <param name="pixels">指向帧的指针.</param>
        public void WriteFrame(RGBAColor* pixels);

        /// <summary>
        /// 设置帧的尺寸.
        /// </summary>
        /// <remarks>
        /// 只能在<see cref="BeginWrite"/>之前调用.
        /// </remarks>
        /// <param name="width">宽度，单位像素.</param>
        /// <param name="height">高度，单位像素.</param>
        public void SetFrameSize(int width, int height);

        /// <summary>
        /// 设置视频的每秒帧数.
        /// </summary>
        /// <remarks>
        /// 只能在<see cref="BeginWrite"/>之前调用.
        /// </remarks>
        /// <param name="fps">每秒帧数.</param>
        public void SetVideoFPS(int fps);

        /// <summary>
        /// 准备渲染. 写入的准备操作在此步完成.
        /// </summary>
        public void BeginWrite();
        
        /// <summary>
        /// 终止渲染. 必要时应释放资源.
        /// </summary>
        public void EndWrite();
        
        /// <summary>
        /// 表示用户控制界面. 这会在导出栏使用.
        /// </summary>
        public Control ControlPanel { get; }
    }

    /// <summary>
    /// 表示一个渲染模块. 该接口用于实现模块的自述.
    /// </summary>
    public interface IRenderModule : INamedObject
    {
        /// <summary>
        /// <see cref="IRenderModule"/>对象的描述.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 此<see cref="IRenderModule"/>开发时使用API的版本.
        /// </summary>
        /// <remarks>
        /// 此版本将用于主程序启动时API兼容性的检测.
        /// </remarks>
        public Version APIVersion { get; }
    }

    /// <summary>
    /// 当前API版本.
    /// </summary>
    public static class APIVersion
    {
        /// <summary>
        /// 当前API版本.
        /// </summary>
        public static Version CurrentAPIVersion => new(1, 0, 0, 0);
    }
}
