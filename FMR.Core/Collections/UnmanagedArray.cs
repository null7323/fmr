using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.Collections
{
    /// <summary>
    /// 表示使用不安全模式存储的数组, 相当于<see cref="Array"/>.
    /// </summary>
    /// <typeparam name="T">类型参数.</typeparam>
    public unsafe class UnmanagedArray<T> : IDisposable where T : unmanaged
    {
        internal T* ptr = null;
        internal long count;

        /// <summary>
        /// 默认的数组大小.
        /// </summary>
        public const long DefaultCapacity = 32;

        /// <summary>
        /// 以<see cref="DefaultCapacity"/>为容量初始化一个<see cref="UnmanagedArray{T}"/>实例.
        /// </summary>
        public UnmanagedArray()
        {
            ptr = UnsafeMemory.Allocate<T>(count);
            count = DefaultCapacity;
        }

        /// <summary>
        /// 以指定容量初始化一个<see cref="UnmanagedArray{T}"/>实例.
        /// </summary>
        /// <param name="initSize">数组的容量.</param>
        public UnmanagedArray(long initSize)
        {
            ptr = UnsafeMemory.Allocate<T>(initSize);
            count = DefaultCapacity;
        }

        /// <summary>
        /// 以给定数据源和大小初始化一个<see cref="UnmanagedArray{T}"/>实例.
        /// </summary>
        /// <param name="source">数据源.</param>
        /// <param name="count">复制的元素个数和容量.</param>
        public UnmanagedArray(T* source, long count)
        {
            ptr = UnsafeMemory.Allocate<T>(count);
            UnsafeMemory.Copy(ptr, source, count);
        }

        /// <summary>
        /// 将当前数组的所有内容复制到<paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">复制的目标位置.</param>
        public void CopyTo(T* destination)
        {
            UnsafeMemory.Copy(destination, ptr, count);
        }

        /// <summary>
        /// 用给定的断言对数组排序.
        /// </summary>
        /// <param name="comparer">断言, 给出两元素的大小关系.</param>
        public void Sort(delegate*<in T, in T, bool> comparer)
        {
            SortHelper<T>.Sort(ptr, count, comparer);
        }

        /// <summary>
        /// 获取指定位置的元素.
        /// </summary>
        /// <param name="index">元素的索引. 如果索引过大, 会抛出<see cref="IndexOutOfRangeException"/>异常.</param>
        /// <returns>元素的引用.</returns>
        public ref T At(long index)
        {
            if (index >= count)
            {
                ThrowHelper.ThrowIndexOutOfRange("Index out of range.");
            }
            return ref ptr[index];
        }

        /// <summary>
        /// 数组首个元素的引用.
        /// </summary>
        public ref T First => ref *ptr;

        /// <summary>
        /// 获得指定位置的数组元素.
        /// </summary>
        /// <param name="index">元素的索引. 此方法无安全检查.</param>
        /// <returns>该元素的引用.</returns>
        public ref T this[long index]
        {
            get => ref ptr[index];
        }

        /// <summary>
        /// 数组的长度.
        /// </summary>
        public long Count => count;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (ptr != null)
            {
                UnsafeMemory.Free(ptr);
            }
            ptr = null;
            count = 0;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        ~UnmanagedArray()
        {
            if (ptr != null)
            {
                UnsafeMemory.Free(ptr);
            }
            ptr = null;
            count = 0;
        }
    }
}
