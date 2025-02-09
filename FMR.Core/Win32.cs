using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.NTKernel
{
    /// <summary>
    /// 堆分配选项.
    /// </summary>
    public enum HeapOptions
    {
        /// <summary>
        /// 如果硬件强制实施数据执行防护，则从此堆分配的所有内存块都允许代码执行.
        /// 在从堆运行代码的应用程序中使用此标志堆.
        /// 如果未指定<see cref="EnableExecute"/>, 且应用程序尝试从受保护的页面运行代码, 则应用程序将收到异常, 状态代码 STATUS_ACCESS_VIOLATION.
        /// </summary>
        EnableExecute = 0x00040000,
        /// <summary>
        /// 当堆函数访问此堆时，不使用序列化访问.
        /// </summary>
        /// <remarks>
        /// 此选项适用于所有后续堆函数调用, 或者, 可以在单个堆函数调用上指定此选项.
        /// 无法为使用此选项创建的堆启用低碎片化堆 (LFH) . 不能锁定使用此选项创建的堆.
        /// </remarks>
        NoSerialize = 0x00000001,
        /// <summary>
        /// 系统引发异常以指示失败, 而不是返回<see langword="null"/>.
        /// </summary>
        GenerateExceptions = 0x00000004
    }
    internal unsafe static partial class Kernel32Wraper
    {
        [LibraryImport("kernel32.dll")]
        internal static partial int AllocConsole();

        [LibraryImport("kernel32.dll")]
        internal static partial void FreeConsole();

        [LibraryImport("kernel32.dll")]
        internal static partial void* GetProcessHeap();

        [LibraryImport("kernel32.dll")]
        internal static partial void* HeapCreate(int flOptions, nuint dwInitialSize, nuint dwMaximumSize);

        [LibraryImport("kernel32.dll")]
        internal static partial void* HeapAlloc(void* hHeap, uint dwFlags, nuint dwBytes);

        [LibraryImport("kernel32.dll")]
        internal static partial int HeapFree(void* hHeap, uint dwFlags, void* lpMem);

        [LibraryImport("kernel32.dll")]
        internal static partial int GetLastError();

        [LibraryImport("kernel32.dll")]
        internal static partial void SetLastError(int dwErrCode);

        [LibraryImport("kernel32.dll")]
        internal static partial void* GetCurrentProcess();

        [LibraryImport("kernel32.dll")]
        internal static partial int ReadProcessMemory(IntPtr hProcess, void* lpBaseAddress, void* lpBuffer, nuint nSize, nuint* lpNumberOfBytesRead);

        [LibraryImport("kernel32.dll")]
        internal static partial int WriteProcessMemory(IntPtr hProcess, void* lpBaseAddress, void* lpBuffer, nuint nSize, nuint* lpNumberOfBytesWritten);
    }
    /// <summary>
    /// 提供有关部分Win32的底层操作.
    /// </summary>
    public static partial class Win32
    {
        /// <summary>
        /// 分配一个控制台. 此方法等效于内部方法<see cref="Kernel32Wraper.AllocConsole"/>.
        /// </summary>
        /// <returns>状态码.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AllocConsole()
        {
            return Kernel32Wraper.AllocConsole();
        }

        /// <summary>
        /// 释放控制台. 此方法等效于内部方法<see cref="Kernel32Wraper.FreeConsole"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FreeConsole()
        {
            Kernel32Wraper.FreeConsole();
        }

        /// <summary>
        /// 获取进程堆. 此方法等效于内部方法<see cref="Kernel32Wraper.GetProcessHeap"/>.
        /// </summary>
        /// <returns>堆指针.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetProcessHeap()
        {
            unsafe
            {
                void* pHeap = Kernel32Wraper.GetProcessHeap();
                return new IntPtr(pHeap);
            }
        }

        /// <summary>
        /// 创建可由调用进程使用的专用堆对象. 这等效于内部方法<see cref="Kernel32Wraper.HeapCreate(int, nuint, nuint)"/>.
        /// </summary>
        /// <remarks>
        /// 函数在进程的虚拟地址空间中保留空间, 并为此块的指定初始部分分配物理存储.
        /// </remarks>
        /// <param name="options">堆分配选项.</param>
        /// <param name="initialSize">堆的初始大小(字节). 此值确定为堆提交的初始内存量, 该值向上舍入为系统页面大小的倍数.</param>
        /// <param name="maximumSize">堆的最大大小(字节). 此值将舍入到系统页大小的倍数，然后在堆的进程虚拟地址空间中保留该大小的块.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr HeapCreate(HeapOptions options, nuint initialSize, nuint maximumSize)
        {
            unsafe
            {
                return (IntPtr)Kernel32Wraper.HeapCreate((int)options, initialSize, maximumSize);
            }
        }

        /// <summary>
        /// 用指定的堆分配内存. 此方法等效于内部方法<see cref="Kernel32Wraper.HeapAlloc(void*, uint, nuint)"/>.
        /// </summary>
        /// <param name="heap">堆指针.</param>
        /// <param name="flags">分配设置,</param>
        /// <param name="bytes">分配大小.</param>
        /// <returns>指向新内存的指针.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr HeapAlloc(IntPtr heap, HeapOptions flags, nuint bytes)
        {
            unsafe
            {
                return (IntPtr)Kernel32Wraper.HeapAlloc((void*)heap, (uint)flags, bytes);
            }
        }

        /// <summary>
        /// 释放由给定的堆分配的内存. 此方法等效于内部方法<see cref="Kernel32Wraper.HeapFree(void*, uint, void*)"/>.
        /// </summary>
        /// <param name="heap">堆指针.</param>
        /// <param name="flags">分配设置.</param>
        /// <param name="memPtr">分配的指针.</param>
        /// <returns>状态码.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int HeapFree(IntPtr heap, HeapOptions flags, IntPtr memPtr)
        {
            unsafe
            {
                return Kernel32Wraper.HeapFree((void*)heap, (uint)flags, (void*)memPtr);
            }
        }

        /// <summary>
        /// 检索调用线程的最后错误代码值.
        /// </summary>
        /// <remarks>
        /// 最后一个错误代码按线程进行维护. 多个线程不会覆盖彼此的最后一个错误代码.
        /// </remarks>
        /// <returns>调用线程的最后错误代码. 这个错误代码由<see cref="SetLastError(int)"/>指定.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLastError()
        {
            return Kernel32Wraper.GetLastError();
        }

        /// <summary>
        /// 设置调用线程的最后错误代码.
        /// </summary>
        /// <param name="errorCode">线程的最后一个错误代码.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLastError(int errorCode)
        {
            Kernel32Wraper.SetLastError(errorCode);
        }

        /// <summary>
        /// 检索当前进程的伪句柄.
        /// </summary>
        /// <returns>当前进程的伪句柄.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetCurrentProcess()
        {
            unsafe
            {
                return (IntPtr)Kernel32Wraper.GetCurrentProcess();
            }
        }

        /// <summary>
        /// 读取给定进程的部分内存.
        /// </summary>
        /// <param name="process">包含正在读取的内存的进程句柄. 句柄必须具有对进程的PROCESS_VM_READ访问权限. </param>
        /// <param name="address">指向从中读取的指定进程中基址的指针.</param>
        /// <param name="buffer">指向从指定进程的地址空间接收内容的缓冲区的指针.</param>
        /// <param name="size">要从指定进程读取的字节数.</param>
        /// <param name="numberOfBytesRead">指向某一变量的引用，该变量接收传输到指定缓冲区的字节数.</param>
        /// <returns>如果该函数成功, 则返回值为非零值. 如果函数失败, 则返回值为0. 要获得更多的错误信息, 请调用<see cref="GetLastError"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadProcessMemory(IntPtr process, IntPtr address, IntPtr buffer, nuint size, ref nuint numberOfBytesRead)
        {
            unsafe
            {
                fixed (nuint* lpNumberOfBytesRead = &numberOfBytesRead)
                {
                    return Kernel32Wraper.ReadProcessMemory(process, (void*)address, (void*)buffer, size, lpNumberOfBytesRead);
                }
            }
        }

        /// <summary>
        /// 将数据写入到指定进程中的内存区域.
        /// </summary>
        /// <param name="process">要修改的进程内存的句柄.</param>
        /// <param name="address">指向将数据写入到的指定进程中基址的指针.</param>
        /// <param name="buffer">指向缓冲区的指针, 该缓冲区包含要写入指定进程的地址空间中的数据.</param>
        /// <param name="size">要写入指定进程的字节数.</param>
        /// <param name="numberOfBytesWritten">指向变量的引用, 该变量接收传输到指定进程的字节数.</param>
        /// <returns>如果该函数成功, 则返回值为非零值. 如果函数失败, 则返回值为0. 要获得更多的错误信息, 请调用<see cref="GetLastError"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteProcessMemory(IntPtr process, IntPtr address, IntPtr buffer, nuint size, ref nuint numberOfBytesWritten)
        {
            unsafe
            {
                fixed (nuint* lpNumberOfBytesWritten = &numberOfBytesWritten)
                {
                    return Kernel32Wraper.WriteProcessMemory(process, (void*)address, (void*)buffer, size, lpNumberOfBytesWritten);
                }
            }
        }

        /// <summary>
        /// 读取给定进程的部分内存.
        /// </summary>
        /// <param name="process">包含正在读取的内存的进程句柄. 句柄必须具有对进程的PROCESS_VM_READ访问权限. </param>
        /// <param name="address">指向从中读取的指定进程中基址的指针.</param>
        /// <param name="buffer">指向从指定进程的地址空间接收内容的缓冲区的指针.</param>
        /// <param name="size">要从指定进程读取的字节数.</param>
        /// <returns>如果该函数成功, 则返回值为非零值. 如果函数失败, 则返回值为0. 要获得更多的错误信息, 请调用<see cref="GetLastError"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadProcessMemory(IntPtr process, IntPtr address, IntPtr buffer, nuint size)
        {
            unsafe
            {
                return Kernel32Wraper.ReadProcessMemory(process, (void*)address, (void*)buffer, size, null);
            }
        }

        /// <summary>
        /// 将数据写入到指定进程中的内存区域.
        /// </summary>
        /// <param name="process">要修改的进程内存的句柄.</param>
        /// <param name="address">指向将数据写入到的指定进程中基址的指针.</param>
        /// <param name="buffer">指向缓冲区的指针, 该缓冲区包含要写入指定进程的地址空间中的数据.</param>
        /// <param name="size">要写入指定进程的字节数.</param>
        /// <returns>如果该函数成功, 则返回值为非零值. 如果函数失败, 则返回值为0. 要获得更多的错误信息, 请调用<see cref="GetLastError"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteProcessMemory(IntPtr process, IntPtr address, IntPtr buffer, nuint size)
        {
            unsafe
            {
                return Kernel32Wraper.WriteProcessMemory(process, (void*)address, (void*)buffer, size, null);
            }
        }
    }
}
