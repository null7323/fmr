using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.GLRender
{
    /// <summary>
    /// 表示一个OpenGL数组缓冲区.
    /// </summary>
    public class ArrayBuffer : IDisposable
    {
        /// <summary>
        /// 初始化一个新的<see cref="ArrayBuffer"/>. 这会要求OpenGL生成一个缓冲.
        /// </summary>
        public ArrayBuffer()
        {
            bufferId = GL.GenBuffer();
            location = -1;
        }
        /// <summary>
        /// 使用来自<paramref name="data"/>的数据填充<see cref="ArrayBuffer"/>.
        /// </summary>
        /// <param name="data">缓冲区数据.</param>
        /// <param name="sizeInBytes">需要填入的数据大小.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillBuffer(IntPtr data, int sizeInBytes)
        {
            if (bufferId == 0)
            {
                return;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, data, BufferUsageHint.StaticDraw);
        }
        /// <summary>
        /// 使用来自<paramref name="data"/>的数据填充<see cref="ArrayBuffer"/>.
        /// </summary>
        /// <param name="data">缓冲区数据.</param>
        /// <param name="sizeInBytes">需要填入的数据大小.</param>
        /// <param name="hint">指示 OpenGL 应该如何处理数据.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillBuffer(IntPtr data, int sizeInBytes, BufferUsageHint hint)
        {
            if (bufferId == 0)
            {
                return;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, data, hint);
        }
        /// <summary>
        /// 使用来自<paramref name="data"/>的数据填充<see cref="ArrayBuffer"/>.
        /// </summary>
        /// <param name="data">缓冲区数据.</param>
        /// <param name="elementCount">需要填入的数据元素个数.</param>
        /// <param name="hint">指示 OpenGL 应该如何处理数据.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void FillBuffer<T>(T* data, int elementCount, BufferUsageHint hint) where T : unmanaged
        {
            if (bufferId == 0)
            {
                return;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, elementCount * sizeof(T), (IntPtr)data, hint);
        }
        /// <summary>
        /// 设置缓冲所适用的顶点属性.
        /// </summary>
        /// <typeparam name="TElement">基元类型.</typeparam>
        /// <param name="location">适用于着色器的位置.</param>
        /// <param name="vectorElementCount">每次处理的元素个数.(向量的维数)</param>
        /// <param name="elementType">GL处理的元素类型.</param>
        public void SetVertexAttribute<TElement>(int location, int vectorElementCount, VertexAttribPointerType elementType) where TElement : unmanaged
        {
            this.location = location;
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            GL.VertexAttribPointer(location, vectorElementCount, elementType, false, Unsafe.SizeOf<TElement>() * vectorElementCount, 0);
        }
        /// <summary>
        /// 启用当前缓冲对应的顶点属性.
        /// </summary>
        /// <param name="enableOrNot">决定是否启用顶点属性.</param>
        public void EnableVertexAttribute(bool enableOrNot)
        {
            if (location < 0)
            {
                ThrowHelper.Throw<InvalidOperationException>();
            }
            if (enableOrNot)
            {
                GL.EnableVertexAttribArray(location);
            }
            else
            {
                GL.DisableVertexAttribArray(location);
            }
        }
        internal ArrayBuffer(int bufferId)
        {
            this.bufferId = bufferId;
            location = -1;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (bufferId != 0)
            {
                GL.DeleteBuffer(bufferId);
            }
            bufferId = 0;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        ~ArrayBuffer()
        {
            if (bufferId != 0)
            {
                GL.DeleteBuffer(bufferId);
            }
            bufferId = 0;
        }
        /// <summary>
        /// 获取当前顶点缓冲适用的顶点位置. 如果此值小于0, 则说明仍未调用<see cref="SetVertexAttribute{TElement}(int, int, VertexAttribPointerType)"/>分配位置.
        /// </summary>
        public int Location => location;
        internal int bufferId;
        internal int location;
    }
}
