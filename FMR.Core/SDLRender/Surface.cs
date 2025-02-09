using SDL3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.LineIterator;

namespace FMR.Core.SDLRender
{
    /// <summary>
    /// 表示一个 SDL 的平面.
    /// </summary>
    public unsafe class Surface : IDisposable
    {
        internal bool requiresManualDispose;
        internal SDL.SDL_Surface* pSurface;
        internal Surface(SDL.SDL_Surface* surface, bool requiresDispose = true)
        {
            pSurface = surface;
            requiresManualDispose = requiresDispose;
        }
        /// <summary>
        /// 创建一个空的<see cref="Surface"/>.
        /// </summary>
        /// <param name="width">宽.</param>
        /// <param name="height">高.</param>
        /// <returns>一个空的<see cref="Surface"/>对象.</returns>
        public static Surface Create(int width, int height)
        {
            SDL.SDL_Surface* surface = SDL.SDL_CreateSurface(width, height, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);
            _ = SDL.SDL_SetSurfaceBlendMode(surface, (uint)BlendMode.None);
            return new Surface(surface);
        }
        /// <summary>
        /// 使用给定的像素创建一个<see cref="Surface"/>. 源像素数据应在当前<see cref="Surface"/>的生命周期内保持有效.
        /// </summary>
        /// <param name="pixels">源像素.</param>
        /// <param name="width">宽.</param>
        /// <param name="height">高.</param>
        /// <returns>一个<see cref="Surface"/>实例, 内容指向<paramref name="pixels"/>.</returns>
        public static Surface CreateFrom(RGBAColor* pixels, int width, int height)
        {
            SDL.SDL_Surface* surface = SDL.SDL_CreateSurfaceFrom(width, height, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888, (nint)pixels, width * 4);
            _ = SDL.SDL_SetSurfaceBlendMode(surface, (uint)BlendMode.None);
            return new Surface(surface);
        }
        /// <summary>
        /// 锁定当前<see cref="Surface"/>.
        /// </summary>
        public void Lock()
        {
            _ = SDL.SDL_LockSurface((nint)pSurface);
        }
        /// <summary>
        /// 解锁当前<see cref="Surface"/>.
        /// </summary>
        public void Unlock()
        {
            SDL.SDL_UnlockSurface((nint)pSurface);
        }
        /// <summary>
        /// 设置当前<see cref="Surface"/>内容为<paramref name="pixels"/>.
        /// </summary>
        /// <param name="pixels">指向像素数据的缓冲区的指针.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSurface(byte* pixels)
        {
            UnsafeMemory.Copy((byte*)pSurface->pixels, pixels, pSurface->pitch * pSurface->h);
        }
        /// <summary>
        /// 使用<paramref name="surface"/>的数据填充当前<see cref="Surface"/>.
        /// </summary>
        /// <param name="surface">用于填充当前<see cref="Surface"/>的数据.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSurface(Surface surface)
        {
            bool requiresDispose = false;
            Surface convertedSurface = surface;
            if (surface.pSurface->format != pSurface->format)
            {
                SDL.SDL_Surface* pConvertedSurface = SDL.SDL_ConvertSurface(surface.pSurface, pSurface->format);
                convertedSurface = new Surface(pConvertedSurface);
                requiresDispose = true;
            }
            UnsafeMemory.Copy((byte*)pSurface->pixels, (byte*)convertedSurface.pSurface->pixels, pSurface->pitch * pSurface->h);
            if (requiresDispose)
            {
                convertedSurface.Dispose();
            }
        }
        /// <summary>
        /// 获取当前<see cref="Surface"/>的像素信息.
        /// 如果当前的<see cref="Surface"/>内容为空, 返回<see cref="nint.Zero"/>.
        /// </summary>
        public nint Data
        {
            get
            {
                if (pSurface == null)
                {
                    return nint.Zero;
                }
                return pSurface->pixels;
            }
        }
        /// <summary>
        /// 表示每行的字节数.
        /// </summary>
        public int Pitch
        {
            get => pSurface->pitch;
        }
        /// <summary>
        /// 获取当前<see cref="Surface"/>的宽.
        /// </summary>
        public int Width
        {
            get
            {
                if (pSurface == null)
                {
                    return 0;
                }
                return pSurface->w;
            }
        }
        /// <summary>
        /// 获取当前<see cref="Surface"/>的高.
        /// </summary>
        public int Height
        {
            get
            {
                if (pSurface == null)
                {
                    return 0;
                }
                return pSurface->h;
            }
        }
        /// <summary>
        /// 获取或设置当前<see cref="Surface"/>是否启用RLE.
        /// </summary>
        public bool RLE
        {
            get
            {
                return SDL.SDL_SurfaceHasRLE((nint)pSurface);
            }
            set
            {
                _ = SDL.SDL_SetSurfaceRLE((nint)pSurface, new SDL.SDLBool(Convert.ToByte(value)));
            }
        }
        /// <summary>
        /// 获取实际的<see cref="Surface"/>地址.
        /// </summary>
        public nint NativeHandle
        {
            get => (nint)pSurface;
        }
        /// <summary>
        /// 将当前<see cref="Surface"/>的内容复制到<paramref name="targetSurface"/>中.
        /// </summary>
        /// <param name="targetSurface">目标<see cref="Surface"/>.</param>
        /// <param name="targetBlitRect">将当前<see cref="Surface"/>复制到<paramref name="targetSurface"/>的<paramref name="targetBlitRect"/>中.</param>
        /// <param name="sourceRect">从当前的<see cref="Surface"/>中取出<paramref name="sourceRect"/>的部分进行复制.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Blit(Surface targetSurface, Rect targetBlitRect, Rect sourceRect)
        {
            int dstWidth = targetSurface.Width;
            int dstHeight = targetSurface.Height;
            int srcWidth = Width;
            int srcHeight = Height;
            SDL.SDL_Rect dst = new()
            {
                x = (int)(targetBlitRect.X * dstWidth),
                y = (int)(targetBlitRect.Y * dstHeight),
                w = (int)(targetBlitRect.Width * dstWidth),
                h = (int)(targetBlitRect.Height * dstHeight)
            };
            SDL.SDL_Rect src = new()
            {
                x = (int)(sourceRect.X * srcWidth),
                y = (int)(sourceRect.Y * srcHeight),
                w = (int)(sourceRect.Width * srcWidth),
                h = (int)(sourceRect.Height * srcHeight)
            };
            _ = SDL.SDL_BlitSurfaceScaled(pSurface, ref src, targetSurface.pSurface, ref dst, SDL.SDL_ScaleMode.SDL_SCALEMODE_LINEAR);
        }
        /// <summary>
        /// 将当前<see cref="Surface"/>的内容复制到<paramref name="targetSurface"/>中.
        /// </summary>
        /// <param name="targetSurface">目标<see cref="Surface"/>.</param>
        /// <param name="targetBlitRect">将当前<see cref="Surface"/>复制到<paramref name="targetSurface"/>的<paramref name="targetBlitRect"/>中.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Blit(Surface targetSurface, Rect targetBlitRect)
        {
            int dstWidth = targetSurface.Width;
            int dstHeight = targetSurface.Height;
            int srcWidth = Width;
            int srcHeight = Height;
            SDL.SDL_Rect dst = new()
            {
                x = (int)(targetBlitRect.X * dstWidth),
                y = (int)(targetBlitRect.Y * dstHeight),
                w = (int)(targetBlitRect.Width * dstWidth),
                h = (int)(targetBlitRect.Height * dstHeight)
            };
            _ = SDL.SDL_BlitSurfaceScaled(pSurface, null, targetSurface.pSurface, ref dst, SDL.SDL_ScaleMode.SDL_SCALEMODE_LINEAR);
        }
        /// <summary>
        /// 将当前<see cref="Surface"/>的内容复制到<paramref name="targetSurface"/>中.
        /// </summary>
        /// <param name="targetSurface">目标<see cref="Surface"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Blit(Surface targetSurface)
        {
            int dstWidth = targetSurface.Width;
            int dstHeight = targetSurface.Height;
            SDL.SDL_Rect dst = new()
            {
                x = 0,
                y = 0,
                w = dstWidth,
                h = dstHeight
            };
            _ = SDL.SDL_BlitSurfaceScaled(pSurface, null, targetSurface.pSurface, ref dst, SDL.SDL_ScaleMode.SDL_SCALEMODE_LINEAR);
        }
        /// <summary>
        /// 将当前<see cref="Surface"/>的内容复制到<paramref name="targetSurface"/>中. 此方法没有安全检查, 因此请保证复制不存在越界.
        /// </summary>
        /// <param name="targetSurface">目标<see cref="Surface"/>.</param>
        /// <param name="targetBlitRect">将当前<see cref="Surface"/>复制到<paramref name="targetSurface"/>的<paramref name="targetBlitRect"/>中.</param>
        /// <param name="sourceRect">从当前的<see cref="Surface"/>中取出<paramref name="sourceRect"/>的部分进行复制.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlitUnsafe(Surface targetSurface, Rect targetBlitRect, Rect sourceRect)
        {
            int dstWidth = targetSurface.Width;
            int dstHeight = targetSurface.Height;
            int srcWidth = Width;
            int srcHeight = Height;
            SDL.SDL_Rect dst = new()
            {
                x = (int)(targetBlitRect.X * dstWidth),
                y = (int)(targetBlitRect.Y * dstHeight),
                w = (int)(targetBlitRect.Width * dstWidth),
                h = (int)(targetBlitRect.Height * dstHeight)
            };
            SDL.SDL_Rect src = new()
            {
                x = (int)(sourceRect.X * srcWidth),
                y = (int)(sourceRect.Y * srcHeight),
                w = (int)(sourceRect.Width * srcWidth),
                h = (int)(sourceRect.Height * srcHeight)
            };
            _ = SDL.SDL_BlitSurfaceUncheckedScaled(pSurface, ref src, targetSurface.pSurface, ref dst, SDL.SDL_ScaleMode.SDL_SCALEMODE_LINEAR);
        }
        /// <summary>
        /// 将当前<see cref="Surface"/>的内容复制到<paramref name="targetSurface"/>中. 此方法没有安全检查, 因此请保证复制不存在越界.
        /// </summary>
        /// <param name="targetSurface">目标<see cref="Surface"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlitUnsafe(Surface targetSurface)
        {
            SDL.SDL_Rect dst = new()
            {
                x = 0,
                y = 0,
                w = targetSurface.Width,
                h = targetSurface.Height
            };
            _ = SDL.SDL_BlitSurfaceUncheckedScaled(pSurface, null, targetSurface.pSurface, ref dst, SDL.SDL_ScaleMode.SDL_SCALEMODE_LINEAR);
        }
        /// <summary>
        /// 将当前<see cref="Surface"/>的所有像素直接复制到<paramref name="target"/>.
        /// </summary>
        /// <param name="target">复制目标.</param>
        public void CopyPixelsTo(Surface target)
        {
            UnsafeMemory.Copy((byte*)target.pSurface->pixels, (byte*)pSurface->pixels, pSurface->pitch * pSurface->h);
        }
        /// <summary>
        /// 设置当前平面的混色模式.
        /// </summary>
        /// <param name="mode">混色模式.</param>
        public void SetBlendMode(BlendMode mode)
        {
            if (mode != BlendMode.Reverse)
            {
                _ = SDL.SDL_SetSurfaceBlendMode(pSurface, (uint)mode);
            }
            else
            {
                uint revMode = SDL.SDL_ComposeCustomBlendMode(
                    SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE, SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE, SDL.SDL_BlendOperation.SDL_BLENDOPERATION_SUBTRACT,
                    SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE, SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ZERO, SDL.SDL_BlendOperation.SDL_BLENDOPERATION_ADD);
                _ = SDL.SDL_SetSurfaceBlendMode(pSurface, revMode);
            }
        }
        /// <summary>
        /// 使用<paramref name="color"/>填充当前<see cref="Surface"/>.
        /// </summary>
        /// <param name="color">填充用的颜色.</param>
        public void Fill(RGBAColor color)
        {
            SDL.SDL_Rect rect = new()
            {
                x = 0,
                y = 0,
                w = Width,
                h = Height
            };
            _ = SDL.SDL_FillSurfaceRect(pSurface, &rect, (uint)color);
        }
        /// <summary>
        /// 交换<see cref="Surface"/>的实际内容.
        /// </summary>
        /// <param name="surface">新的平面结构指针..</param>
        /// <returns>原有的原始<see cref="Surface"/>指针.</returns>
        public nint SwapSurface(nint surface)
        {
            nint old = (nint)pSurface;
            pSurface = (SDL.SDL_Surface*)surface;
            return old;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (pSurface != null && requiresManualDispose)
            {
                SDL.SDL_DestroySurface(pSurface);
            }
            pSurface = null;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        ~Surface()
        {
            if (pSurface != null && requiresManualDispose)
            {
                SDL.SDL_DestroySurface(pSurface);
            }
            pSurface = null;
        }
    }
}
