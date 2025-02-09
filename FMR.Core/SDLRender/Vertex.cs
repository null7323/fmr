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
    /// 表示绘制的一个顶点.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        /// <summary>
        /// 表示此顶点的坐标.
        /// </summary>
        public Vec Position;
        /// <summary>
        /// 当前顶点的颜色.
        /// </summary>
        public Vec4Color Color;
        /// <summary>
        /// 纹理的坐标. 此坐标是标准化的.
        /// </summary>
        public Vec TexCoord;
        /// <summary>
        /// 使用给定位置和颜色初始化<see cref="Vertex"/>.
        /// </summary>
        /// <param name="position">顶点位置.</param>
        /// <param name="color">顶点颜色.</param>
        public Vertex(Vec position, Vec4Color color)
        {
            Position = position;
            Color = color;
        }
        /// <summary>
        /// 使用给定位置和纹理坐标初始化<see cref="Vertex"/>.
        /// </summary>
        /// <param name="position">顶点位置.</param>
        /// <param name="texCoord">纹理坐标.</param>
        public Vertex(Vec position, Vec texCoord)
        {
            Position = position;
            Color = new Vec4Color(1.0f);
            TexCoord = texCoord;
        }
        /// <summary>
        /// 使用给定位置和纹理坐标初始化<see cref="Vertex"/>.
        /// </summary>
        /// <param name="position">顶点位置.</param>
        /// <param name="color">顶点颜色.</param>
        /// <param name="texCoord">纹理坐标.</param>
        public Vertex(Vec position, in Vec4Color color, Vec texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
    }

    /// <summary>
    /// 提供顶点绘制的辅助函数.
    /// </summary>
    public static class VertexHelper
    {
        /// <summary>
        /// 使用给定的矩形和纹理生成顶点.
        /// </summary>
        /// <param name="rect">矩形的范围.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vertex V1, Vertex V2, Vertex V3, Vertex V4, Vertex V5, Vertex V6) MakeVertices(in Rect rect)
        {
            Vec rightButtom = new(1.0f, 1.0f);
            Vertex v1 = new(rect.Position, Vec.Zero);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), Vec.UnitX);
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), Vec.UnitY);
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), rightButtom);
            return (v1, v2, v3, v2, v3, v4);
        }
        /// <summary>
        /// 使用给定的矩形和纹理生成顶点.
        /// </summary>
        /// <param name="rect">矩形的范围.</param>
        /// <param name="outputVertices">输出顶点的缓冲区.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MakeVertices(in Rect rect, Vertex* outputVertices)
        {
            Vec rightButtom = new(1.0f, 1.0f);
            Vertex v1 = new(rect.Position, Vec.Zero);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), Vec.UnitX);
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), Vec.UnitY);
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), rightButtom);
            outputVertices[0] = v1;
            outputVertices[1] = v2;
            outputVertices[2] = v3;
            outputVertices[3] = v2;
            outputVertices[4] = v3;
            outputVertices[5] = v4;
        }
        /// <summary>
        /// 使用给定的矩形和颜色生成顶点.
        /// </summary>
        /// <param name="rect">矩形的范围.</param>
        /// <param name="color">矩形的颜色.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vertex V1, Vertex V2, Vertex V3, Vertex V4, Vertex V5, Vertex V6) MakeVertices(in Rect rect, in Vec4Color color)
        {
            Vertex v1 = new(rect.Position, color);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), color);
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), color);
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), color);
            return (v1, v2, v3, v2, v3, v4);
        }
        /// <summary>
        /// 使用给定的矩形和颜色生成顶点.
        /// </summary>
        /// <param name="rect">矩形的范围.</param>
        /// <param name="topLeft">左上顶点的颜色.</param>
        /// <param name="topRight">右上顶点的颜色.</param>
        /// <param name="bottomLeft">左下顶点的颜色.</param>
        /// <param name="bottomRight">右下顶点的颜色.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vertex V1, Vertex V2, Vertex V3, Vertex V4, Vertex V5, Vertex V6) MakeVertices(in Rect rect, in Vec4Color topLeft, in Vec4Color topRight, in Vec4Color bottomRight, in Vec4Color bottomLeft)
        {
            Vertex v1 = new(rect.Position, topLeft);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), topRight);
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), bottomLeft);
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), bottomRight);
            return (v1, v2, v3, v2, v3, v4);
        }
        /// <summary>
        /// 使用给定的矩形和颜色与纹理生成顶点.
        /// </summary>
        /// <param name="rect">矩形的范围.</param>
        /// <param name="topLeft">左上顶点的颜色.</param>
        /// <param name="topRight">右上顶点的颜色.</param>
        /// <param name="bottomLeft">左下顶点的颜色.</param>
        /// <param name="bottomRight">右下顶点的颜色.</param>
        /// <param name="texUV">纹理坐标.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vertex V1, Vertex V2, Vertex V3, Vertex V4, Vertex V5, Vertex V6) MakeVertices(in Rect rect, in Vec4Color topLeft, in Vec4Color topRight, in Vec4Color bottomRight, in Vec4Color bottomLeft, in Rect texUV)
        {
            Vertex v1 = new(rect.Position, topLeft, texUV.Position);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), topRight, new Vec(texUV.Position.X + texUV.Width, texUV.Y));
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), bottomLeft, new Vec(texUV.Position.X, texUV.Y + texUV.Height));
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), bottomRight, new Vec(texUV.Position.X + texUV.Width, texUV.Y + texUV.Height));
            return (v1, v2, v3, v2, v3, v4);
        }
        /// <summary>
        /// 使用给定的矩形和颜色生成顶点.
        /// </summary>
        /// <param name="rect">矩形的范围.</param>
        /// <param name="color">矩形的颜色.</param>
        /// <param name="outputVertices">输出顶点的缓冲区.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MakeVertices(in Rect rect, in Vec4Color color, Vertex* outputVertices)
        {
            Vertex v1 = new(rect.Position, color);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), color);
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), color);
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), color);
            outputVertices[0] = v1;
            outputVertices[1] = v2;
            outputVertices[2] = v3;
            outputVertices[3] = v2;
            outputVertices[4] = v3;
            outputVertices[5] = v4;
        }
        /// <summary>
        /// 使用给定的矩形和纹理生成顶点.
        /// </summary>
        /// <param name="rects">每一个矩形的范围.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vertex[] MakeVertices(Rect[] rects)
        {
            List<Vertex> vertices = [];
            foreach (Rect rect in rects)
            {
                var (V1, V2, V3, V4, V5, V6) = MakeVertices(rect);
                vertices.Add(V1);
                vertices.Add(V2);
                vertices.Add(V3);
                vertices.Add(V4);
                vertices.Add(V5);
                vertices.Add(V6);
            }
            return [.. vertices];
        }
        /// <summary>
        /// 使用给定的矩形和纹理生成顶点.
        /// </summary>
        /// <param name="rects">每一个矩形的范围.</param>
        /// <param name="rectCount">矩形的个数.</param>
        /// <param name="outputVertices">输出顶点保存的缓冲区.</param>
        /// <returns>顶点元组.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MakeVertices(Rect* rects, int rectCount, Vertex* outputVertices)
        {
            Rect* ptr = rects;
            Vertex* outputPos = outputVertices;

            for (int i = 0; i < rectCount; i++)
            {
                MakeVertices(*ptr++, outputPos);
                outputPos += 6;
            }
        }
        /// <summary>
        /// 直接在<see cref="VertexBuffer"/>中生成顶点.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        /// <param name="buffer"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MakeVerticesInBuffer(in Rect rect, in Vec4Color color, ref VertexBuffer buffer)
        {
            if (buffer.nextWritePos + 6 > buffer.endOfBuffer)
            {
                buffer.PushToContextUnchecked();
            }
            Vertex* writePos = buffer.nextWritePos;
            Vertex v1 = new(rect.Position, color);
            Vertex v2 = new(new Vec(rect.Position.X + rect.Width, rect.Y), color);
            Vertex v3 = new(new Vec(rect.Position.X, rect.Y + rect.Height), color);
            Vertex v4 = new(new Vec(rect.X + rect.Width, rect.Y + rect.Height), color);
            *writePos++ = v1;
            *writePos++ = v2;
            *writePos++ = v3;
            *writePos++ = v2;
            *writePos++ = v3;
            *writePos++ = v4;
            buffer.nextWritePos = writePos;
        }
    }

    /// <summary>
    /// 表示一个手动管理的顶点缓冲区. 对象的销毁需要调用<see cref="Dispose"/>方法.
    /// </summary>
    public unsafe struct VertexBuffer : IDisposable
    {
        internal Vertex* buffer;
        internal Vertex* nextWritePos;
        internal Vertex* endOfBuffer;
        internal Context context;
        internal int bufferVertexSize;

        /// <summary>
        /// 表示默认的顶点最大容量
        /// </summary>
        public const int DefaultVertexCount = 262144 * 6;

        /// <summary>
        /// 使用给定容量初始化当前<see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="bufferVertexCount">顶点的最大个数. 必须是3的倍数.</param>
        /// <param name="context">当前上下文.</param>
        public VertexBuffer(int bufferVertexCount, Context context)
        {
            if (bufferVertexCount % 3 != 0)
            {
                ThrowHelper.Throw("Bad vertex count.");
            }
            buffer = UnsafeMemory.Allocate<Vertex>(bufferVertexCount);
            bufferVertexSize = bufferVertexCount;
            nextWritePos = buffer;
            endOfBuffer = buffer + bufferVertexCount;
            this.context = context;
        }
        /// <summary>
        /// 使用默认容量初始化当前<see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="context">当前上下文.</param>
        public VertexBuffer(Context context)
        {
            buffer = UnsafeMemory.Allocate<Vertex>(DefaultVertexCount);
            bufferVertexSize = DefaultVertexCount;
            nextWritePos = buffer;
            endOfBuffer = buffer + DefaultVertexCount;
            this.context = context;
        }

        /// <summary>
        /// 向缓冲区中推入一个矩形.
        /// </summary>
        /// <param name="v1">第一个顶点.</param>
        /// <param name="v2">第二个顶点.</param>
        /// <param name="v3">第三个顶点.</param>
        /// <param name="v4">第四个顶点.</param>
        /// <param name="v5">第五个顶点.</param>
        /// <param name="v6">第六个顶点.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRect(in Vertex v1, in Vertex v2, in Vertex v3, in Vertex v4, in Vertex v5, in Vertex v6)
        {
            if (nextWritePos + 6 > endOfBuffer)
            {
                lock (context)
                {
                    context.DrawVerticesInplace(buffer, (int)(nextWritePos - buffer));
                }
                nextWritePos = buffer;
            }
            *nextWritePos++ = v1;
            *nextWritePos++ = v2;
            *nextWritePos++ = v3;
            *nextWritePos++ = v4;
            *nextWritePos++ = v5;
            *nextWritePos++ = v6;
        }
        /// <summary>
        /// 向缓冲区中推入一个矩形.
        /// </summary>
        /// <param name="ptr">六个顶点的首地址.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRect(Vertex* ptr)
        {
            if (nextWritePos + 6 > endOfBuffer)
            {
                lock (context)
                {
                    context.DrawVerticesInplace(buffer, (int)(nextWritePos - buffer));
                }
                nextWritePos = buffer;
            }
            UnsafeMemory.Copy(nextWritePos, ptr, 6);
            nextWritePos += 6;
        }
        /// <summary>
        /// 向缓冲区中推入一个三角形.
        /// </summary>
        /// <param name="v1">第一个顶点.</param>
        /// <param name="v2">第二个顶点.</param>
        /// <param name="v3">第三个顶点.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            if (nextWritePos >= endOfBuffer)
            {
                lock (context)
                {
                    context.DrawVerticesInplace(buffer, (int)(nextWritePos - buffer));
                }
                nextWritePos = buffer;
            }
            *nextWritePos++ = v1;
            *nextWritePos++ = v2;
            *nextWritePos++ = v3;
        }

        /// <summary>
        /// 将现有的所有顶点推入上下文.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushToContext()
        {
            if (buffer != nextWritePos)
            {
                lock (context)
                {
                    context.DrawVerticesInplace(buffer, (int)(nextWritePos - buffer));
                }
            }
            nextWritePos = buffer;
        }
        /// <summary>
        /// 将现有的所有顶点推入上下文.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushToContextUnchecked()
        {
            lock (context)
            {
                context.DrawVerticesInplace(buffer, (int)(nextWritePos - buffer));
            }
            nextWritePos = buffer;
        }
        /// <summary>
        /// 向当前缓冲区追加顶点. 无安全检查.
        /// </summary>
        /// <param name="otherBuffer">需要追加的顶点.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendVerticesUnchecked(VertexBuffer otherBuffer)
        {
            int count = (int)(otherBuffer.nextWritePos - otherBuffer.buffer);
            UnsafeMemory.Copy(nextWritePos, otherBuffer.buffer, count);
            nextWritePos += count;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (buffer != null)
            {
                UnsafeMemory.Free(buffer);
            }
            buffer = null;
        }
    }
}
