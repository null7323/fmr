using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.Collections
{
    /// <summary>
    /// 表示一个使用非托管类型的线性列表. 这相当于使用非托管内存的<see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">类型参数.</typeparam>
    public unsafe class UnmanagedList<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        /// 默认构造方法, 容量为<see cref="DefaultCapacity"/>.
        /// </summary>
        public UnmanagedList() : this(DefaultCapacity)
        {

        }

        /// <summary>
        /// 使用指定的大小构建<see cref="UnmanagedList{T}"/>.
        /// </summary>
        /// <param name="initCapacity">初始化的容量.</param>
        public UnmanagedList(long initCapacity)
        {
            if (initCapacity <= 0)
            {
                ThrowHelper.ThrowArgument("Capacity should not be less than or equal to 0.", "InitCapacity");
            }

            list = UnsafeMemory.Allocate<T>(initCapacity);
            end = list + initCapacity;
            last = list;
            UnsafeMemory.Set(list, 0, initCapacity * elementSize);
        }

        /// <summary>
        /// 将此容器中的元素按顺序拷贝至目标.
        /// </summary>
        /// <param name="destination">目标位置.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* destination)
        {
            UnsafeMemory.Copy(destination, list, last - list);
        }

        /// <summary>
        /// 判断当前列表是否包含任何元素.
        /// </summary>
        /// <returns>如果存在元素, 返回<see langword="true"/>; 否则返回<see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any()
        {
            return last - list != 0;
        }

        /// <summary>
        /// 向列表末尾添加一个元素.
        /// </summary>
        /// <param name="newValue">要添加的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T newValue)
        {
            if (end == last)
            {
                EnsureCapacity();
            }

            fixed (T* source = &newValue)
            {
                UnsafeMemory.Copy(last++, source, 1);
            }

            version++;
        }

        /// <summary>
        /// 向列表末尾追加连续的多个元素.
        /// </summary>
        /// <param name="source">要添加的元素地址.</param>
        /// <param name="count">要添加的元素个数.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(T* source, long count)
        {
            Reserve(count + Count);
            UnsafeMemory.Copy(last, source, count);
            last += count;
        }

        /// <summary>
        /// 裁剪当前列表. 如果当前列表占用率低于0.9，则执行列表的缩减.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            if (last - list < (end - list) * 9 / 10)
            {
                long size = last - list;
                T* ptr = UnsafeMemory.Allocate<T>(size);
                UnsafeMemory.Copy(ptr, list, size);
                last = ptr + size;
                end = last;
                UnsafeMemory.Free(list);
                list = ptr;
            }
        }

        /// <summary>
        /// 向指定的位置插入一个元素.
        /// </summary>
        /// <param name="index">要插入的位置.</param>
        /// <param name="newValue">要插入的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(long index, in T newValue)
        {
            T* ptr = list + index;
            if (ptr > end)
            {
                ThrowHelper.ThrowIndexOutOfRange("Index out of range.");
            }

            if (ptr == end)
            {
                Add(in newValue);
                return;
            }

            UnsafeMemory.Move(ptr + 1, ptr, last - ptr);
            fixed (T* source = &newValue)
            {
                UnsafeMemory.Copy(ptr, source, 1);
            }

            version++;
        }

        /// <summary>
        /// 移除位于指定位置的元素.
        /// </summary>
        /// <param name="position">要移除的元素的位置.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(long position)
        {
            if (list + position >= last)
            {
                ThrowHelper.ThrowIndexOutOfRange("Index out of range.");
            }

            T* ptr = list + position;
            if (last - ptr != 0)
            {
                UnsafeMemory.Move(ptr, ptr + 1, last - ptr);
            }

            last--;
            version++;
        }

        /// <summary>
        /// 移除最后一个元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast()
        {
            if (list != last)
            {
                last--;
                // UnsafeMemory.Set(last, 0, elementSize);
                version++;
            }
        }

        /// <summary>
        /// 设定要保留的最小容量.
        /// </summary>
        /// <param name="capacity">新设置的最小容量.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(long capacity)
        {
            if (capacity > Capacity)
            {
                T* ptr = UnsafeMemory.Allocate<T>(capacity);
                UnsafeMemory.Set(ptr, 0, capacity * elementSize);
                UnsafeMemory.Copy(ptr, list, last - list);
                last = ptr + (last - list);
                UnsafeMemory.Free(list);
                list = ptr;
                end = ptr + capacity;
            }
        }

        /// <summary>
        /// 使用指定的比较器对列表排序.
        /// </summary>
        /// <param name="predicator">元素比较的断言.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(delegate*<in T, in T, bool> predicator)
        {
            SortHelper<T>.Sort(list, Count, predicator);
        }

        /// <inheritdoc/>
        public unsafe void Dispose()
        {
            if (list != null)
            {
                UnsafeMemory.Free(list);
            }

            list = null;
            last = null;
            end = null;
            GC.SuppressFinalize(this);
            version++;
        }

        /// <summary>
        /// 清除所有的元素.
        /// </summary>
        public unsafe void Clear()
        {
            if (list != null)
            {
                UnsafeMemory.Free(list);
            }

            list = UnsafeMemory.Allocate<T>(32);
            end = list + 32;
            last = list;
            version++;
        }

        /// <summary>
        /// 寻找给定的元素的首个出现位置.
        /// </summary>
        /// <param name="targetValue">目标值.</param>
        /// <returns>该元素的索引. 如果该元素不存在, 返回<see cref="ObjectNotFound"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe long FindFirst(in T targetValue)
        {
            fixed (T* right = &targetValue)
            {
                for (T* ptr = list; ptr != last; ptr++)
                {
                    if (UnsafeMemory.Equal(ptr, right, elementSize))
                    {
                        return ptr - list;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// 返回指向首个元素的指针.
        /// </summary>
        /// <returns>一个指向类型<typeparamref name="T"/>元素的指针, 指向当前列表的元素首地址.</returns>
        public unsafe T* First()
        {
            return list;
        }

        /// <summary>
        /// Finalizer of <see cref="UnmanagedList{T}"/>.
        /// </summary>
        unsafe ~UnmanagedList()
        {
            if (list != null)
            {
                UnsafeMemory.Free(list);
            }

            list = null;
            last = null;
            end = null;
        }

        /// <summary>
        /// 将当前非托管数据复制到一个新的托管数组后返回.
        /// </summary>
        /// <returns>一个托管数组, 包含当前<see cref="UnmanagedList{T}"/>所有对象.</returns>
        public T[] ToArray()
        {
            T[] managedArray = new T[last - list];
            fixed (T* ptr = managedArray)
            {
                UnsafeMemory.Copy(ptr, list, last - list);
            }
            return managedArray;
        }

        /// <summary>
        /// 返回迭代器.
        /// </summary>
        /// <returns><see cref="Iterator"/>实例.</returns>
        public Iterator Begin()
        {
            return new Iterator(this);
        }

        /// <summary>
        /// 返回位于末尾的迭代器.
        /// </summary>
        /// <returns>位于末尾的<see cref="Iterator"/>实例.</returns>
        public Iterator End()
        {
            Iterator result = new(this);
            while (result.MoveNext())
            {
            }

            return result;
        }

        /// <summary>
        /// 表示一个迭代器.
        /// </summary>
        public struct Iterator : IDisposable
        {
            private unsafe T* last;

            private unsafe T* curr;

            private int ver;

            private UnmanagedList<T> ul;

            /// <summary>
            /// 表示当前的元素.
            /// </summary>
            public unsafe ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return ref *curr;
                }
            }
            /// <summary>
            /// 构造一个迭代器.
            /// </summary>
            /// <param name="ulist">要迭代的列表.</param>
            public unsafe Iterator(UnmanagedList<T> ulist)
            {
                ul = ulist;
                last = ulist.last - 1;
                curr = ulist.list - 1;
                ver = ulist.version;
            }

            /// <summary>
            /// 复制一个迭代器，将新迭代器后移一位.
            /// </summary>
            /// <param name="iterator">要操作的迭代器.</param>
            /// <returns>新迭代器.</returns>
            public unsafe static Iterator operator ++(in Iterator iterator)
            {
                Iterator result = default;
                result.curr = iterator.curr == iterator.last ? iterator.curr : iterator.curr + 1;
                result.last = iterator.last;
                result.ver = iterator.ver;
                result.ul = iterator.ul;
                return result;
            }

#pragma warning disable CS1591
            public unsafe static bool operator <(in Iterator Left, in Iterator Right)
            {
                return Left.curr < Right.curr;
            }

            public unsafe static bool operator >(in Iterator Left, in Iterator Right)
            {
                return Left.curr < Right.curr;
            }

            public unsafe static bool operator <=(in Iterator Left, in Iterator Right)
            {
                return Left.curr <= Right.curr;
            }

            public unsafe static bool operator >=(in Iterator Left, in Iterator Right)
            {
                return Left.curr >= Right.curr;
            }
#pragma warning restore CS1591

            /// <summary>
            /// 将迭代器后移一个元素.
            /// </summary>
            /// <returns>本次移动是否成功.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool MoveNext()
            {
                if (ver != ul.version)
                {
                    ThrowHelper.Throw("Invalid iterator");
                }

                if (curr != last)
                {
                    curr++;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 重设迭代器至初始位置.
            /// </summary>
            public unsafe void Reset()
            {
                ver = ul.version;
                curr = ul.list;
                last = ul.last;
            }
            /// <inheritdoc/>
            public unsafe void Dispose()
            {
                ver = -1;
                curr = null;
                last = null;
            }
        }

        private unsafe static readonly long elementSize = sizeof(T);

        internal unsafe T* list = null;

        internal unsafe T* end = null;

        internal unsafe T* last = null;

        private int version;

        /// <summary>
        /// The default capacity if <see cref="UnmanagedList{T}.UnmanagedList()"/> is called.
        /// </summary>
        public const int DefaultCapacity = 32;
        /// <summary>
        /// If "find" methods returns this value, it indicates that target is not found.
        /// </summary>
        public readonly long ObjectNotFound = long.MaxValue;

        /// <summary>
        /// 访问指定位置的元素. 此方法没有安全检查.
        /// </summary>
        /// <param name="index">索引位置.</param>
        /// <returns>指定元素的引用.</returns>
        public unsafe ref T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref list[index];
            }
        }

        /// <summary>
        /// 表示目前存有的元素数量.
        /// </summary>
        public unsafe long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return last - list;
            }
        }

        /// <summary>
        /// 表示下一次扩容前存储的最大元素数.
        /// </summary>
        public unsafe long Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return end - list;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EnsureCapacity()
        {
            if (last == end)
            {
                long size = last - list;
                long newSize = size * 2;
                T* ptr = UnsafeMemory.Allocate<T>(newSize);
                UnsafeMemory.Copy(ptr, list, size);
                UnsafeMemory.Set(ptr + size, 0, size * elementSize);
                last = ptr + (last - list);
                UnsafeMemory.Free(list);
                list = ptr;
                end = ptr + newSize;
                version++;
            }
        }

        /// <summary>
        /// 访问指定位置的元素.
        /// </summary>
        /// <param name="index">元素索引</param>
        /// <returns>元素的引用.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T At(long index)
        {
            if (index >= last - list)
            {
                ThrowHelper.ThrowIndexOutOfRange("Index out of range.");
            }
            return ref list[index];
        }
    }
}
