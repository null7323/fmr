using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Silk.NET.Vulkan;
using Microsoft.Win32.SafeHandles;
using System.IO;
using FMR.Core.NTKernel;
using System.Runtime;
using System.Runtime.Intrinsics;

namespace FMR.Core
{
    /// <summary>
    /// 提供不安全内存的访问. 此类是兼容的产物.
    /// </summary>
    public unsafe static class UnsafeMemory
    {
        /// <summary>
        /// 释放所指的内存.
        /// </summary>
        /// <param name="ptr">需要释放的内存块.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
            NativeMemory.Free(ptr);
        }

        /// <summary>
        /// 等效于<see cref="NativeMemory.Realloc(void*, nuint)"/>.
        /// </summary>
        /// <param name="ptr">分配的指针.</param>
        /// <param name="size">新分配的大小.</param>
        /// <returns>新分配的指针.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* ReAllocate(void* ptr, long size)
        {
            return NativeMemory.Realloc(ptr, (nuint)size);
        }

        /// <summary>
        /// 等效于<see cref="NativeMemory.AlignedAlloc(nuint, nuint)"/>.
        /// </summary>
        /// <param name="size">分配大小.</param>
        /// <param name="alignment">对齐长度.</param>
        /// <returns>分配的内存.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocateAligned(long size, long alignment) 
        {
            return NativeMemory.AlignedAlloc((nuint)size, (nuint)alignment);
        }

        /// <summary>
        /// 使用 <see cref="MaximumSIMDAlignment"/> 作为内存对齐值分配一定大小的内存.
        /// </summary>
        /// <param name="size">分配大小.</param>
        /// <returns>分配的内存.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocateAligned(long size)
        {
            return NativeMemory.AlignedAlloc((nuint)size, MaximumSIMDAlignment);
        }

        /// <summary>
        /// 等效于<see cref="NativeMemory.AlignedFree(void*)"/>.
        /// </summary>
        /// <param name="ptr">要释放的对齐内存.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FreeAligned(void* ptr)
        {
            NativeMemory.AlignedFree(ptr);
        }

        /// <summary>
        /// 等效于<see cref="NativeMemory.AlignedRealloc(void*, nuint, nuint)"/>.
        /// </summary>
        /// <param name="ptr">分配的指针.</param>
        /// <param name="size">新分配的大小.</param>
        /// <param name="alignment">对齐长度.</param>
        /// <returns>新分配的指针.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* ReAllocateAligned(void* ptr, long size, long alignment)
        {
            return NativeMemory.AlignedRealloc(ptr, (nuint)size, (nuint)alignment);
        }

        /// <summary>
        /// 按照参数类型进行分配. 等效于对<see cref="NativeMemory.Alloc(nuint, nuint)"/>的调用.
        /// </summary>
        /// <typeparam name="T">类型参数，应该符合<see langword="unmanaged"/>的约束以保证取地址安全.</typeparam>
        /// <param name="count">类型为T的元素个数.</param>
        /// <returns>指向新分配内存的指针.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Allocate<T>(long count) where T : unmanaged
        {
            return (T*)NativeMemory.Alloc((nuint)(sizeof(T) * count));
        }

        /// <summary>
        /// 将一片内存复制至另一片内存. 等效于<see cref="NativeMemory.Copy(void*, void*, nuint)"/>.
        /// </summary>
        /// <typeparam name="T">类型参数，应该符合<see langword="unmanaged"/>约束以保证取地址安全.</typeparam>
        /// <param name="destination">目的地址.</param>
        /// <param name="source">源地址.</param>
        /// <param name="count">类型为T的元素个数.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(T* destination, T* source, long count) where T : unmanaged
        {
            count *= sizeof(T);
            NativeMemory.Copy(source, destination, (nuint)count);
        }

        /// <summary>
        /// 将一片内存复制至另一片内存. 等效于<see cref="NativeMemory.Copy(void*, void*, nuint)"/>.
        /// </summary>
        /// <typeparam name="T">类型参数，应该符合<see langword="unmanaged"/>约束以保证取地址安全.</typeparam>
        /// <param name="destination">目的地址.</param>
        /// <param name="source">源地址.</param>
        /// <param name="count">类型为T的元素个数.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move<T>(T* destination, T* source, long count) where T : unmanaged
        {
            count *= sizeof(T);
            NativeMemory.Copy(source, destination, (nuint)count);
        }

        /// <summary>
        /// 将一片内存统一设置为给定值. 等效于<see cref="NativeMemory.Fill(void*, nuint, byte)"/>.
        /// </summary>
        /// <param name="ptr">目标地址.</param>
        /// <param name="value">填充的值.</param>
        /// <param name="count">填充的个数.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(void* ptr, byte value, long count)
        {
            NativeMemory.Fill(ptr, (nuint)count, value);
        }

        /// <summary>
        /// 判断两片内存是否相等. 等效于<see cref="MemoryExtensions.SequenceEqual{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>.
        /// </summary>
        /// <param name="left">左侧内存块.</param>
        /// <param name="right">右侧内存块.</param>
        /// <param name="sizeInBytes">需要比较的长度，单位byte.</param>
        /// <returns>如果两片内存内容一致，返回<see langword="true"/>，否则返回<see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equal(void* left, void* right, long sizeInBytes)
        {
            byte* pLeft = (byte*)left;
            byte* pRight = (byte*)right;
            Span<byte> leftBuffer, rightBuffer;
            while (sizeInBytes > int.MaxValue)
            {
                leftBuffer = new(pLeft, int.MaxValue);
                rightBuffer = new(pRight, int.MaxValue);
                if (!leftBuffer.SequenceEqual(rightBuffer))
                {
                    return false;
                }
                sizeInBytes -= int.MaxValue;
                pLeft += int.MaxValue;
                pRight += int.MaxValue;
            }
            leftBuffer = new(pLeft, (int)sizeInBytes);
            rightBuffer = new(pRight, (int)sizeInBytes);
            return leftBuffer.SequenceEqual(rightBuffer);
        }

        /// <summary>
        /// 获取某一对象的实际地址. 等效于<see langword="fixed"/>语句.
        /// </summary>
        /// <remarks>
        /// 对象可能会移动，因此此方法是不安全的. 请在确保对象不会移动的前提下使用此方法返回的地址.
        /// </remarks>
        /// <typeparam name="T">类型参数.</typeparam>
        /// <param name="obj">待获取的对象.</param>
        /// <returns>给定对象的地址.</returns>
        public static T* GetActualAddressOf<T>(ref T obj) where T : unmanaged
        {
            fixed (T* ptr = &obj)
            {
                return ptr;
            }
        }

        /// <summary>
        /// 反转<see langword="ushort"/>类型整数的端序.
        /// </summary>
        /// <param name="data">要翻转的整数.</param>
        /// <returns>转换后的整数.</returns>
        public static ushort ReverseEndian(ushort data)
        {
            return (ushort)(((data & 0xFF00) >> 8) | ((data & 0x00FF) << 8));
        }

        /// <summary>
        /// 反转<see langword="uint"/>类型整数的端序.
        /// </summary>
        /// <param name="data">要翻转的整数.</param>
        /// <returns>转换后的整数.</returns>
        public static uint ReverseEndian(uint data)
        {
            const uint hi8 = 0xFF000000;
            const uint low8 = 0x000000FF;
            const uint midHi8 = 0x00FF0000;
            const uint midLow8 = 0x0000FF00;
            return ((data & hi8) >> 24) | ((data & midHi8) >> 8) | ((data & midLow8) << 8) | ((data & low8) << 24);
        }

        /// <summary>
        /// 表示最大可能内存对齐值.
        /// </summary>
        public static uint MaximumSIMDAlignment
        {
            get
            {
                if (Avx512F.IsSupported)
                {
                    return 64;
                }
                if (Avx2.IsSupported)
                {
                    return 32;
                }
                if (Avx.IsSupported)
                {
                    return 32;
                }
                if (Sse42.IsSupported)
                {
                    return 16;
                }
                if (Sse2.IsSupported)
                {
                    return 16;
                }
                return (uint)sizeof(IntPtr);
            }
        }

        /// <summary>
        /// 获取一片内存对齐后的地址.
        /// </summary>
        /// <typeparam name="T">类型参数.</typeparam>
        /// <param name="address">原始地址.</param>
        /// <param name="alignment">内存对齐值.</param>
        /// <returns>对齐后的地址.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetAlignedAddress<T>(T* address, nint alignment) where T : unmanaged
        {
            return (T*)((nint)address + (alignment - 1) & ~(alignment - 1));
        }
    }
}
