using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.Collections
{
    /// <summary>
    /// 表示一个可前后插入与删除元素的托管双向队列. 此容器使用环形数组实现.
    /// </summary>
    /// <typeparam name="T">类型参数.</typeparam>
    public class Deque<T> : IReadOnlyCollection<T>
    {
        internal int start;
        internal int tail;
        internal int count;
        internal T[] values = [];

        /// <summary>
        /// 表示缺省初始大小.
        /// </summary>
        public const int DefaultCapacity = 32;
        /// <summary>
        /// 使用 <see cref="DefaultCapacity"/> 作为容量初始化当前 <see cref="Deque{T}"/>.
        /// </summary>
        public Deque() : this(DefaultCapacity)
        {
            
        }
        /// <summary>
        /// 使用给定容量初始化当前 <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="capacity">初始化的容量.</param>
        public Deque(int capacity)
        {
            if (capacity <= 0)
            {
                ThrowHelper.ThrowArgument("Capacity <= 0.", nameof(capacity));
            }
            values = new T[capacity];
            start = tail = 0;
        }
        /// <inheritdoc/>
        public int Count => count;
        
        /// <summary>
        /// 获取或设置指定位置的元素.
        /// </summary>
        /// <param name="index">元素的索引.</param>
        /// <returns>该位置的元素.</returns>
        public T this[int index]
        {
            get
            {
                int offsetIndex = index + start;
                if (offsetIndex >= values.Length)
                {
                    offsetIndex -= values.Length;
                }
                return values[offsetIndex];
            }
            set
            {
                int offsetIndex = index + start;
                if (offsetIndex >= values.Length)
                {
                    offsetIndex -= values.Length;
                }
                values[offsetIndex] = value;
            }
        }

        /// <summary>
        /// 往队列的头部插入一个元素.
        /// </summary>
        /// <param name="item">待插入的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFront(T item)
        {
            if (count == values.Length)
            {
                DoubleCapacity();
            }
            if (start == 0)
            {
                start = values.Length;
            }
            values[--start] = item;
            ++count;
        }

        /// <summary>
        /// 获取某一元素对应的索引.
        /// </summary>
        /// <param name="item">要查找的元素.</param>
        /// <returns>元素第一次出现的索引. 如果没有查找到, 则返回 -1.</returns>
        public int IndexOf(T item)
        {
            int offsetIndex = start;
            for (int i = 0; i < count; ++i)
            {
                if (offsetIndex >= values.Length)
                {
                    offsetIndex -= values.Length;
                }
                if (EqualityComparer<T>.Default.Equals(item, values[offsetIndex]))
                {
                    return i;
                }
                offsetIndex++;
            }
            return -1;
        }

        /// <summary>
        /// 向<see cref="Deque{T}"/>末尾追加一个元素.
        /// </summary>
        /// <param name="item">要追加的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLast(T item)
        {
            if (count == values.Length)
            {
                DoubleCapacity();
            }
            if (tail == values.Length)
            {
                tail = 0;
            }
            values[tail++] = item;
            ++count;
        }

        /// <summary>
        /// 移除当前<see cref="Deque{T}"/>的所有元素.
        /// </summary>
        public void Clear()
        {
            start = tail = 0;
            values = new T[DefaultCapacity];
        }

        /// <summary>
        /// 判断当前<see cref="Deque{T}"/>是否包含元素<paramref name="item"/>.
        /// </summary>
        /// <param name="item">用于查找的元素.</param>
        /// <returns>如果当前队列包含 <paramref name="item"/>, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary>
        /// 将当前<see cref="Deque{T}"/>的所有内容复制到<paramref name="array"/>.
        /// </summary>
        /// <param name="array">复制的目标数组.</param>
        /// <param name="arrayIndex">复制的目标偏移.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (tail < start && tail != 0)
            {
                int firstSegLength = values.Length - start;
                ArraySegment<T> firstSeg = new(array, start, firstSegLength);
                firstSeg.CopyTo(array, arrayIndex);
                ArraySegment<T> secondSeg = new(array, start + firstSegLength, tail);
                secondSeg.CopyTo(array, arrayIndex + firstSegLength);
            }
            else
            {
                ArraySegment<T> seg = new(array, start, count);
                seg.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// 表示适用于<see cref="Deque{T}"/>的迭代器.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            internal Deque<T> deque;
            internal int iterIndex;
            internal T? currentElement;
            internal Enumerator(Deque<T> deque)
            {
                this.deque = deque;
                iterIndex = -1;
                currentElement = default;
            }
            /// <inheritdoc/>
            public readonly T Current => currentElement!;

            readonly object IEnumerator.Current => default!;

            /// <inheritdoc/>
            public readonly void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (iterIndex == deque.count)
                {
                    currentElement = default;
                    return false;
                }
                iterIndex++;
                T[] data = deque.values;
                int realIndex = iterIndex + deque.start;
                int capacity = data.Length;
                if (realIndex >= capacity)
                {
                    realIndex -= capacity;
                }
                currentElement = data[realIndex];
                return true;
            }

            /// <inheritdoc/>
            public void Reset()
            {
                iterIndex = -1;
                currentElement = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DoubleCapacity()
        {
            int rawSize = values.Length;
            int newSize = rawSize * 2;
            T[] buffer = new T[newSize];
            CopyTo(buffer, 0);
            values = buffer;
            start = 0;
            tail = rawSize;
        }
    }
}
