using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core
{
    /// <summary>
    /// 表示一个可被排序的非托管类型.
    /// </summary>
    /// <typeparam name="T">类型参数.</typeparam>
    public interface IUnmanagedComparable<T> where T : IUnmanagedComparable<T>
    {
        /// <summary>
        /// 比较<paramref name="left"/>和<paramref name="right"/>, 并给出<paramref name="left"/>是否应该在<paramref name="right"/>前.
        /// </summary>
        /// <param name="left">左比较数.</param>
        /// <param name="right">右比较数.</param>
        /// <returns>一个<see langword="bool"/>值, 指示<paramref name="left"/>是否在<paramref name="right"/>前.</returns>
        public static abstract bool Compare(in T left, in T right);
    }
    /// <summary>
    /// 表示默认的比较器.
    /// </summary>
    /// <typeparam name="T">类型参数, 应当实现<see cref="IComparable{T}"/>.</typeparam>
    public static class DefaultComparer<T> where T : IComparable<T>
    {
        /// <summary>
        /// 默认的比较器.
        /// </summary>
        /// <param name="left">左侧元素.</param>
        /// <param name="right">右侧元素.</param>
        /// <returns>如果<see cref="IComparable{T}"/>的CompareTo方法返回值小于0, 返回<see langword="true"/>，否则返回<see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Predicator(in T left, in T right)
        {
            return left.CompareTo(right) < 0;
        }
    }
    /// <summary>
    /// 提供连续不安全数据的排序.
    /// </summary>
    /// <typeparam name="T">类型参数，应当满足<see langword="unmanaged"/>约束.</typeparam>
    public unsafe static class SortHelper<T> where T : unmanaged
    {
        // 实现: https://referencesource.microsoft.com/#q=GenericArraySortHelper<T>.IntroSort"

        /// <summary>
        /// 指示适用于稳定排序的调用插入排序阈值.
        /// </summary>
        public const int StableSortInsertionSortThreshold = 32;
        /// <summary>
        /// 获取递归的深度.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetRecursionDepth(long len)
        {
            long res = 0;
            while (len >= 1)
            {
                ++res;
                len /= 2;
            }
            return res;
        }
        /// <summary>
        /// 如果 a处元素 > b处元素, 进行交换
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SwapIfGreater(T* collection, long a, long b, delegate*<in T, in T, bool> pred)
        {
            if (a != b)
            {
                if (!pred(collection[a], collection[b]))
                {
                    (collection[b], collection[a]) = (collection[a], collection[b]);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertionSort(T* collection, long low, long high, delegate*<in T, in T, bool> pred)
        {
            long i, j;
            T t;
            for (i = low; i != high; ++i)
            {
                j = i;
                t = collection[i + 1];

                while (j >= low && pred(t, collection[j]))
                {
                    collection[j + 1] = collection[j];
                    --j;
                }

                collection[j + 1] = t;
            }
        }

        /// <summary>
        /// 使用给定的方法为连续的不安全数据排序.
        /// </summary>
        /// <remarks>
        /// 此方法使用内省排序. 这种排序方式不稳定，时间复杂度为O(n log n).
        /// </remarks>
        /// <param name="collection">指向不安全数据的指针.</param>
        /// <param name="size">这片数据的元素个数.</param>
        /// <param name="predicator">比较的方法. 对于pred(a, b)，若结果为 <see langword="true"/>，则a在b前面.</param>
        public static void Sort(in T* collection, long size, delegate*<in T, in T, bool> predicator)
        {
            if (size <= 1)
            {
                return;
            }
            IntroSort(collection, 0, size - 1, GetRecursionDepth(size), predicator);
        }

        /// <summary>
        /// 使用给定的方法为安全数据排序.
        /// </summary>
        /// <remarks>
        /// 这种排序方式稳定，空间复杂度O(n).
        /// </remarks>
        /// <param name="collection">指向不安全数据的指针.</param>
        /// <param name="keySelector">比较的方法. 对于pred(a, b)，若结果为 <see langword="true"/>，则a在b前面.</param>
        /// <param name="output">表示新的有序序列的起始位置.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void StableSort<U>(IEnumerable<T> collection, Func<T, U> keySelector, T* output) where U : IComparable<U>
        {
            IOrderedEnumerable<T> orderedCollection = Enumerable.OrderBy(collection, keySelector);
            T[] orderedArray = [.. orderedCollection];
            fixed (T* array = orderedArray)
            {
                UnsafeMemory.Copy(output, array, orderedArray.Length);
            }
        }

        /// <summary>
        /// 内省排序.
        /// </summary>
        private static void IntroSort(T* collection, long low, long high, long depth, delegate*<in T, in T, bool> pred)
        {
            while (high > low)
            {
                long partitionSize = high - low + 1;
                if (partitionSize < 16)
                {
                    if (partitionSize == 1)
                    {
                        return;
                    }
                    if (partitionSize == 2)
                    {
                        SwapIfGreater(collection, low, high, pred);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        SwapIfGreater(collection, low, high - 1, pred);
                        SwapIfGreater(collection, low, high, pred);
                        SwapIfGreater(collection, high - 1, high, pred);
                        return;
                    }
                    InsertionSort(collection, low, high, pred);
                }
                if (depth == 0)
                {
                    HeapSort(collection, low, high, pred);  
                    return;
                }
                --depth;

                long p = PickPivotAndPartition(collection, low, high, pred);
                IntroSort(collection, p + 1, high, depth, pred);
                high = p - 1;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DownHeap(in T* collection, long i, long n, long low, delegate*<in T, in T, bool> pred)
        {
            T d = collection[low + i - 1];
            long child;
            while (i <= n / 2)
            {
                child = 2 * i;
                if (child < n && pred(collection[low + child - 1], collection[low + child]))
                {
                    child++;
                }
                if (!pred(d, collection[low + child - 1]))
                {
                    break;
                }

                collection[low + i - 1] = collection[low + child - 1];
                i = child;
            }
            collection[low + i - 1] = d;
        }
        /// <summary>
        /// 堆排序.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HeapSort(in T* collection, long low, long high, delegate*<in T, in T, bool> pred)
        {
            long n = high - low + 1;
            for (long i = n / 2; i >= 1; --i)
            {
                DownHeap(collection, i, n, low, pred);
            }
            for (long i = n; i > 1; --i)
            {
                //Swap(lo, lo + i - 1);
                (collection[low + i - 1], collection[low]) = (collection[low], collection[low + i - 1]);
                DownHeap(collection, 1, i - 1, low, pred);
            }
        }
        private static long PickPivotAndPartition(in T* collection, long low, long high, delegate*<in T, in T, bool> pred)
        {
            long mid = low + (high - low) / 2;
            SwapIfGreater(collection, low, mid, pred);
            SwapIfGreater(collection, low, high, pred);
            SwapIfGreater(collection, mid, high, pred);

            T pivot = collection[mid];
            //Swap(mid, hi - 1);
            T m = collection[mid];
            (collection[high - 1], collection[mid]) = (m, collection[high - 1]);
            long left = low, right = high - 1;

            while (left < right)
            {
                while (pred(collection[++left], pivot))
                {

                }

                while (pred(pivot, collection[--right]))
                {

                }

                if (left >= right)
                {
                    break;
                }

                m = collection[left];
                collection[left] = collection[right];
                collection[right] = m;
            }

            m = collection[left];
            collection[left] = collection[high - 1];
            collection[high - 1] = m;
            return left;
        }
    }
}
