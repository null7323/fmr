using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.GLRender
{
    /// <summary>
    /// 表示一个Open GL的本机窗口.
    /// </summary>
    public class Context : NativeWindow
    {
        /// <summary>
        /// 使用初始设置初始化本机窗口.
        /// </summary>
        public Context(int width, int height) : base(NativeWindowSettings.Default)
        {
            ClientSize = new Vector2i(width, height);
            VSync = VSyncMode.Off;
            Context?.MakeCurrent();
        }
        /// <summary>
        /// 交换前后缓冲.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapBuffers()
        {
            Context.SwapBuffers();
        }
        /// <summary>
        /// 处理当前窗口所有事件.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void HandleEvents()
        {
            NewInputFrame();
            if (GLFW.WindowShouldClose(WindowPtr))
            {
                WindowShouldClose = true;
            }
            ProcessWindowEvents(false);
        }
        /// <summary>
        /// 表示当前窗口应当关闭.
        /// </summary>
        public bool WindowShouldClose { get; private set; } = false;
    }
}
