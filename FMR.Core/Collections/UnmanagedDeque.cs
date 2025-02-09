using FMR.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FMR.Core.Collections
{
    /// <summary>
    /// 表示一个可前后插入与删除元素的双向队列. 此容器使用环形数组实现.
    /// </summary>
    /// <typeparam name="T">类型参数, 必须符合<see langword="unmanaged"/>约束.</typeparam>
    public unsafe class UnmanagedDeque<T> : IDisposable where T : unmanaged
    {
        internal T* dataArray;
        internal T* head;
        internal T* tail;
        internal T* arrayEnd;
        internal long count;

        internal void DoubleCapacity()
        {
            long oldSize = arrayEnd - dataArray;
            long newSize = oldSize * 2;
            T* newBuffer = UnsafeMemory.Allocate<T>(newSize);
            if (head < tail)
            {
                UnsafeMemory.Copy(newBuffer, head, tail - head);
            }
            else
            {
                long firstPartSize = arrayEnd - head;
                UnsafeMemory.Copy(newBuffer, head, firstPartSize);
                UnsafeMemory.Copy(newBuffer + firstPartSize, dataArray, tail - dataArray);
            }
            head = newBuffer;
            tail = newBuffer + oldSize;
            arrayEnd = newBuffer + newSize;
            UnsafeMemory.Free(dataArray);
            dataArray = newBuffer;
        }
        /// <summary>
        /// 默认的初始容量.
        /// </summary>
        public const long InitialCapacity = 24;
        /// <summary>
        /// 使用<see cref="InitialCapacity"/>初始化一个<see cref="UnmanagedDeque{T}"/>实例.
        /// </summary>
        public UnmanagedDeque() : this(InitialCapacity)
        {

        }
        /// <summary>
        /// 使用给定容量初始化一个<see cref="UnmanagedDeque{T}"/>实例.
        /// </summary>
        /// <param name="initCapacity">初始化的容量.</param>
        public UnmanagedDeque(long initCapacity)
        {
            initCapacity = Math.Max(initCapacity, 1);
            dataArray = UnsafeMemory.Allocate<T>(initCapacity);
            arrayEnd = dataArray + initCapacity;
            head = dataArray;
            tail = head;
            count = 0;
        }

        /// <summary>
        /// 往队列的头部插入一个元素.
        /// </summary>
        /// <param name="value">待插入的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushFront(in T value)
        {
            if (count == (arrayEnd - dataArray))
            {
                DoubleCapacity();
            }
            if (head == dataArray)
            {
                head = arrayEnd;
            }
            *--head = value;
            ++count;
        }
        /// <summary>
        /// 往队列的尾部插入一个元素.
        /// </summary>
        /// <param name="value">待插入的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBack(in T value)
        {
            if (count == (arrayEnd - dataArray))
            {
                DoubleCapacity();
            }
            if (tail == arrayEnd)
            {
                tail = dataArray;
            }
            *tail++ = value;
            ++count;
        }
        /// <summary>
        /// 从队列的头部删去一个元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopFront()
        {
            if (count == 0)
            {
                ThrowHelper.Throw("Bad operation: not enough element to dequeue.");
            }
            if ((++head) == arrayEnd)
            {
                head = dataArray;
            }
            --count;
        }
        /// <summary>
        /// 从队列的尾部删去一个元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopBack()
        {
            if (count == 0)
            {
                ThrowHelper.Throw("Bad operation: not enough element to dequeue.");
            }
            if (tail == dataArray)
            {
                tail = arrayEnd;
            }
            --tail;
            --count;
        }
        /// <summary>
        /// 获取当前元素总数.
        /// </summary>
        public long Count => count;
        /// <summary>
        /// 获取第一个元素.
        /// </summary>
        /// <returns>第一个元素.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek()

        {
            return ref *head;
        }
        /// <summary>
        /// 访问指定位置的元素.
        /// </summary>
        /// <remarks>
        /// 此方法无安全检查.
        /// </remarks>
        /// <param name="index">元素的索引.</param>
        /// <returns>指向该元素的引用.</returns>
        public ref T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                T* ptr = head + index;
                if (ptr >= arrayEnd)
                {
                    ptr -= (arrayEnd - dataArray);
                }
                return ref *ptr;
            }
        }
        /// <summary>
        /// 为当前队列预留指定大小的空间.
        /// </summary>
        /// <param name="capacity">预留后的总元素数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(long capacity)
        {
            long oldSize = arrayEnd - dataArray;

            if (oldSize >= capacity)
            {
                return;
            }

            long newSize = capacity;
            T* newBuffer = UnsafeMemory.Allocate<T>(newSize);
            if (head < tail)
            {
                UnsafeMemory.Copy(newBuffer, head, tail - head);
            }
            else
            {
                long firstPartSize = arrayEnd - head;
                UnsafeMemory.Copy(newBuffer, head, firstPartSize);
                UnsafeMemory.Copy(newBuffer + firstPartSize, dataArray, tail - dataArray);
            }
            head = newBuffer;
            tail = newBuffer + oldSize;
            arrayEnd = newBuffer + newSize;
            UnsafeMemory.Free(dataArray);
            dataArray = newBuffer;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (dataArray != null)
            {
                UnsafeMemory.Free(dataArray);
            }
            dataArray = null;
            arrayEnd = null;
            head = null;
            tail = null;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        ~UnmanagedDeque()
        {
            if (dataArray != null)
            {
                UnsafeMemory.Free(dataArray);
            }
            dataArray = null;
            arrayEnd = null;
            head = null;
            tail = null;
        }
    }
}
