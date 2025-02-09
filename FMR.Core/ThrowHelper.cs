using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core
{
    /// <summary>
    /// 提供一系列<see langword="throw"/>操作的方法.
    /// </summary>
    public static class ThrowHelper
    {
        /// <summary>
        /// 抛出<see cref="Exception"/>类型的异常.
        /// </summary>
        /// <param name="message">描述异常的信息.</param>
        /// <exception cref="Exception"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw(string message)
        {
            throw new Exception(message);
        }

        /// <summary>
        /// 抛出<see cref="IndexOutOfRangeException"/>类型的异常.
        /// </summary>
        /// <param name="message">描述异常的信息.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowIndexOutOfRange(string message)
        {
            throw new IndexOutOfRangeException(message);
        }

        /// <summary>
        /// 抛出<see cref="IndexOutOfRangeException"/>类型的异常.
        /// </summary>
        /// <param name="message">描述异常的信息.</param>
        /// <param name="innerException">内部异常.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowIndexOutOfRange(string message, Exception innerException)
        {
            throw new IndexOutOfRangeException(message, innerException);
        }

        /// <summary>
        /// 抛出<see cref="ArgumentException"/>类型的异常.
        /// </summary>
        /// <param name="message">异常信息.</param>
        /// <param name="name">异常参数的名称.</param>
        /// <exception cref="ArgumentException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgument(string message, string name)
        {
            throw new ArgumentException(message, name);
        }

        /// <summary>
        /// 抛出<see cref="FileNotFoundException"/>类型的异常
        /// </summary>
        /// <param name="fileName">不存在的文件名.</param>
        /// <exception cref="FileNotFoundException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowFileNotExist(string fileName)
        {
            throw new FileNotFoundException($"{fileName} does not exist.");
        }

        /// <summary>
        /// 抛出<see cref="NotSupportedException"/>类型的异常.
        /// </summary>
        /// <param name="message">描述异常的信息.</param>
        /// <exception cref="NotSupportedException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupported(string message)
        {
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// 抛出<see cref="NotImplementedException"/>类型的异常.
        /// </summary>
        /// <param name="message">描述异常的信息.</param>
        /// <exception cref="NotImplementedException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotImplemented(string message)
        {
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// 抛出<see cref="NotImplementedException"/>类型的异常.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotImplemented()
        {
            throw new NotImplementedException(); 
        }

        /// <summary>
        /// 抛出<see cref="GLRender.ShaderCompilationFailedException"/>类型的异常.
        /// </summary>
        /// <param name="message">异常信息.</param>
        /// <exception cref="GLRender.ShaderCompilationFailedException"></exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowShaderCompilationFailed(string message)
        {
            throw new GLRender.ShaderCompilationFailedException(message);
        }

        /// <summary>
        /// 抛出<typeparamref name="TException"/>类型的异常.
        /// </summary>
        /// <typeparam name="TException">抛出的异常类型.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw<TException>() where TException : Exception, new()
        {
            throw new TException();
        }
    }
}
