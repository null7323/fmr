using FMR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QQS.Legacy
{
    internal unsafe class Canvas : IDisposable
    {
        internal uint* pixels;
        internal uint*[] indices;
        internal uint clearColor;

        internal uint* clearedPixels;
        internal readonly int width, height, frameSize;

        internal bool disposed = false;
        public Canvas(int width, int height, uint clearColor)
        {
            this.width = width;
            this.height = height;
            frameSize = width * height;

            pixels = UnsafeMemory.Allocate<uint>(width * height);
            clearedPixels = UnsafeMemory.Allocate<uint>(width * height);

            this.clearColor = clearColor;

            indices = new uint*[height];
            for (int i = 0; i < height; i++)
            {
                indices[i] = pixels + (height - i - 1) * width;
            }
            for (int i = 0, frameSize = width * height; i < frameSize; i++)
            {
                clearedPixels[i] = clearColor;
            }
            UnsafeMemory.Copy(pixels, clearedPixels, frameSize);
        }

        /// <summary>
        /// 绘制矩形边框.<br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(int x, int y, int width, int height, uint color)
        {
            int i;
            uint* ptr;
            //if (x < _Width)
            for (i = y; i < y + height; ++i)
            {
                indices[i][x] = color;
            }
            ptr = indices[y] + x;
            //if (y < _Height)
            for (i = x; i < x + width; ++i)
            {
                // indices[y][i] = color;
                *ptr++ = color;
            }
            //if (w > 1)
            for (i = y; i < y + height; ++i)
            {
                indices[i][x + width - 1] = color;
            }
            ptr = indices[y + height - 1] + x;
            //if (h > 1)
            for (i = x; i < x + width; ++i)
            {
                // indices[y + height - 1][i] = color;
                *ptr++ = color;
            }
        }

        /// <summary>
        /// 用指定的颜色填满指定区域.<br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void FillRectangle(int x, int y, int width, int height, uint color)
        {
            for (int i = y, yend = y + height; i != yend; ++i)
            {
                uint* p = indices[i] + x;
                for (int j = x, xend = x + width; j != xend; ++j)
                {
                    *p++ = color;
                }
            }
        }

        public void Clear()
        {
            if (pixels != null && clearedPixels != null)
            {
                UnsafeMemory.Copy(pixels, clearedPixels, frameSize);
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            if (pixels is not null)
            {
                UnsafeMemory.Free(pixels);
            }
            if (clearedPixels is not null)
            {
                UnsafeMemory.Free(clearedPixels);
            }
            pixels = null;
            clearedPixels = null;
            indices = [];
            disposed = true;
        }
    }
}
