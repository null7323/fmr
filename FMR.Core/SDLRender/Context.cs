using SDL3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.SDLRender
{
    /// <summary>
    /// 表示一个基于SDL2的渲染器.
    /// </summary>
    public class Context : IDisposable
    {
        internal nint renderer;
        internal nint window;
        internal int width;
        internal int height;
        internal int windowWidth;
        internal int windowHeight;
        internal int verticesInBuffer;
        internal EventHandler handler;

        internal const int renderFlushThreshold = 131072;

        /// <summary>
        /// 创建一个<see cref="Context"/>. 这会创建一个窗口.
        /// </summary>
        /// <param name="caption">窗口标题.</param>
        /// <param name="width">窗口的宽.</param>
        /// <param name="height">窗口的高.</param>
        /// <param name="forceOpenGL">是否强制使用OpenGL.</param>
        public Context(string caption, int width, int height, bool forceOpenGL = false)
        {
            SDL.SDL_WindowFlags windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;
            if (forceOpenGL)
            {
                windowFlag |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
            }

            window = SDL.SDL_CreateWindow(caption, width, height, windowFlag);
            renderer = SDL.SDL_CreateRenderer(window, null!);
            this.width = width;
            this.height = height;
            windowWidth = width;
            windowHeight = height;
            verticesInBuffer = 0;
            handler = new EventHandler(this);
        }
        /// <summary>
        /// 获取当前窗口的<see cref="Surface"/>对象.
        /// </summary>
        /// <returns>一个<see cref="Surface"/>对象, 包含了当前窗口的画布信息.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Surface GetWindowSurface()
        {
            unsafe
            {
                return new Surface(SDL.SDL_GetWindowSurface(window), false);
            }
        }
        /// <summary>
        /// 从渲染对象中读取像素.
        /// </summary>
        /// <returns>包含像素信息的<see cref="Surface"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Surface ReadPixels()
        {
            SDL.SDL_Surface* ptr = SDL.SDL_RenderReadPixels(renderer, null);
            verticesInBuffer = 0;
            return new Surface(ptr);
        }

        /// <summary>
        /// 将一定量的顶点发送至设备以绘图. 传入的顶点会被更改.
        /// </summary>
        /// <param name="vertices">需要绘制的顶点.</param>
        /// <param name="vertexCount">顶点的个数.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DrawVertices(Vertex* vertices, int vertexCount)
        {
            int contextWidth = width;
            int contextHeight = height;
            for (long i = 0; i < vertexCount; i++)
            {
                SDL.SDL_Vertex* vertex = (SDL.SDL_Vertex*)(vertices + i);
                ref SDL.SDL_FPoint pt = ref vertex->position;
                pt.x *= contextWidth;
                pt.y *= contextHeight;
            }
            _ = SDL.SDL_RenderGeometry(renderer, null, (SDL.SDL_Vertex*)vertices, vertexCount, null, 0);
            verticesInBuffer += vertexCount;

            if (verticesInBuffer >= renderFlushThreshold)
            {
                Flush();
            }
        }
        /// <summary>
        /// 将一定量的顶点发送至设备以绘图. 传入的顶点会被更改.
        /// </summary>
        /// <param name="vertices">需要绘制的顶点.</param>
        /// <param name="vertexCount">顶点的个数.</param>
        /// <param name="tex">绘制的纹理.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DrawVertices(Vertex* vertices, int vertexCount, Texture tex)
        {
            for (long i = 0; i < vertexCount; i++)
            {
                SDL.SDL_Vertex* vertex = (SDL.SDL_Vertex*)(vertices + i);
                ref SDL.SDL_FPoint pt = ref vertex->position;
                pt.x *= width;
                pt.y *= height;
            }
            _ = SDL.SDL_RenderGeometry(renderer, tex.tex, (SDL.SDL_Vertex*)vertices, vertexCount, null, 0);
            verticesInBuffer += vertexCount;

            if (verticesInBuffer >= renderFlushThreshold)
            {
                Flush();
            }
        }

        /// <summary>
        /// 将一定量的矩形顶点发送至设备以绘图. 传入的顶点会被更改.
        /// </summary>
        /// <param name="vertices">需要绘制的矩形顶点.</param>
        /// <param name="vertexCount">顶点的个数.</param>
        /// <param name="indices">顶点的顺序.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DrawRectVertices(Vertex* vertices, int vertexCount, int* indices)
        {
            int contextWidth = width, contextHeight = height;
            for (long i = 0; i < vertexCount; i++)
            {
                SDL.SDL_Vertex* vertex = (SDL.SDL_Vertex*)(vertices + i);
                ref SDL.SDL_FPoint pt = ref vertex->position;
                pt.x *= contextWidth;
                pt.y *= contextHeight;
            }
            SDL.SDL_RenderGeometry(renderer, null, (SDL.SDL_Vertex*)vertices, vertexCount, indices, vertexCount * 6 / 4);
            verticesInBuffer += vertexCount;

            if (verticesInBuffer >= renderFlushThreshold)
            {
                Flush();
            }
        }

        /// <summary>
        /// 使用当前的<see cref="EventHandler"/>处理所有剩余事件.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleEvents()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event ev))
            {
                switch (ev.type)
                {
                    case (uint)SDL.SDL_EventType.SDL_EVENT_QUIT:
                        handler.OnQuit();
                        break;
                    case (uint)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
                        if (ev.key.key == (int)SDL.SDL_Keycode.SDLK_ESCAPE)
                        {
                            handler.OnQuit();
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// 表示当前<see cref="Context"/>的事件处理器.
        /// </summary>
        public EventHandler ContextEventHandler => handler;

        /// <summary>
        /// 交换缓冲, 显示最新渲染至上下文的画面.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Present()
        {
            SDL.SDL_RenderPresent(renderer);
            verticesInBuffer = 0;
        }

        /// <summary>
        /// 强制刷新当前上下文.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flush()
        {
            SDL.SDL_FlushRenderer(renderer);
            verticesInBuffer = 0;
        }
        /// <summary>
        /// 设置渲染的目标.
        /// </summary>
        /// <param name="tex">目标纹理. 如果为<see langword="null"/>, 则目标恢复为窗口.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetRenderTarget(Texture? tex)
        {
            if (tex is null)
            {
                _ = SDL.SDL_SetRenderTarget(renderer, null);
                width = windowWidth;
                height = windowHeight;
            }
            else
            {
                _ = SDL.SDL_SetRenderTarget(renderer, tex.tex);
                width = tex.width;
                height = tex.height;
            }
        }
        /// <summary>
        /// 使用上一次调用<see cref="Fill(RGBAColor)"/>的颜色填充当前<see cref="Context"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Fill()
        {
            _ = SDL.SDL_RenderFillRect(renderer, null);
        }

        /// <summary>
        /// 将当前<see cref="Context"/>以颜色<paramref name="color"/>填充.
        /// </summary>
        /// <param name="color">填充色.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Fill(RGBAColor color)
        {
            _ = SDL.SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
            _ = SDL.SDL_RenderFillRect(renderer, null);
        }
        /// <summary>
        /// 将当前<see cref="Context"/>的指定部分以颜色<paramref name="color"/>填充.
        /// </summary>
        /// <param name="rect">填充区域.</param>
        /// <param name="color">填充色.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Rect rect, RGBAColor color)
        {
            _ = SDL.SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
            SDL.SDL_FRect fill = rect.AsSDLRectF(width, height);
            _ = SDL.SDL_RenderFillRect(renderer, ref fill);
        }
        /// <summary>
        /// 使用上一次绘制的颜色填充渲染器.
        /// </summary>
        public void Clear()
        {
            _ = SDL.SDL_RenderClear(renderer);
        }
        /// <summary>
        /// 使用给定颜色填充渲染器.
        /// </summary>
        /// <param name="color">填充色.</param>
        public void Clear(RGBAColor color)
        {
            _ = SDL.SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
            _ = SDL.SDL_RenderClear(renderer);
        }
        /// <summary>
        /// 设置当前纹理的混色模式.
        /// </summary>
        /// <param name="mode">混色模式.</param>
        public void SetBlendMode(BlendMode mode)
        {
            if (mode != BlendMode.Reverse)
            {
                _ = SDL.SDL_SetRenderDrawBlendMode(renderer, (uint)mode);
            }
            else
            {
                uint revMode = SDL.SDL_ComposeCustomBlendMode(
                    SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE, SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE, SDL.SDL_BlendOperation.SDL_BLENDOPERATION_SUBTRACT,
                    SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE, SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ZERO, SDL.SDL_BlendOperation.SDL_BLENDOPERATION_ADD);
                _ = SDL.SDL_SetRenderDrawBlendMode(renderer, revMode);
            }
        }

        /// <summary>
        /// 更新窗口的<see cref="Surface"/>数据.
        /// </summary>
        public void UpdateWindowSurface()
        {
            _ = SDL.SDL_UpdateWindowSurface(window);
        }

        /// <summary>
        /// 启用分辨率无关的坐标系.
        /// </summary>
        /// <param name="logicalWidth">渲染的逻辑宽度.</param>
        /// <param name="logicalHeight">渲染的逻辑高度.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnableLogicalResolution(int logicalWidth, int logicalHeight)
        {
            SDL.SDL_SetRenderLogicalPresentation(renderer, logicalWidth, logicalHeight,
                SDL.SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE);
        }
        /// <summary>
        /// 禁用分辨率无关渲染.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DisableLogicalResolution()
        {
            SDL.SDL_SetRenderLogicalPresentation(renderer, width, height, SDL.SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED);
        }

        /// <summary>
        /// 获取窗口的宽.
        /// </summary>
        public int Width => width;
        /// <summary>
        /// 获取窗口的高.
        /// </summary>
        public int Height => height;
        /// <inheritdoc/>
        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            renderer = nint.Zero;
            window = nint.Zero;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~Context()
        {
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            renderer = nint.Zero;
            window = nint.Zero;
        }
    }
}
