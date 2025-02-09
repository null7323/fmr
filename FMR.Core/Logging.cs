using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core
{
    /// <summary>
    /// 日志等级.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试级别日志.
        /// </summary>
        Debug = 0,
        /// <summary>
        /// 信息级别日志.
        /// </summary>
        Info = 1,
        /// <summary>
        /// 警告级别日志.
        /// </summary>
        Warning = 2,
        /// <summary>
        /// 异常级别日志.
        /// </summary>
        Error = 3
    }
    /// <summary>
    /// 表示日志数据.
    /// </summary>
    public struct LogData
    {
        /// <summary>
        /// 日志记录时间.
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// 日志等级.
        /// </summary>
        public LogLevel Level;
        /// <summary>
        /// 日志信息.
        /// </summary>
        public string Message;
    }
    /// <summary>
    /// 提供统一的日志推送.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// 表示接受日志并输出的类型.
        /// </summary>
        /// <param name="data">日志信息.</param>
        public delegate void LogRecv(LogData data);
#pragma warning disable CA2211
        /// <summary>
        /// 表示所有日志接受方.
        /// </summary>
        public static LogRecv LogReceiver = (data) => { };
#pragma warning restore CA2211

        private static readonly object lockRoot = new();
        private static readonly Queue<LogData> messageHistory = [];
        private static int maxHistoryCount = 500;
        /// <summary>
        /// 向<see cref="LogReceiver"/>中写入新的日志.
        /// </summary>
        /// <param name="level">日志水平.</param>
        /// <param name="message">日志信息.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(LogLevel level, string message)
        {
            DateTime dateTime = DateTime.Now;
            lock (lockRoot)
            {
                LogReceiver(new LogData { Time = dateTime, Level = level, Message = message });
            }
            PushLog(dateTime, level, message);
        }
        /// <summary>
        /// 向<see cref="LogReceiver"/>中写入新的日志. 只在Debug模式下有效.
        /// </summary>
        /// <param name="level">日志水平.</param>
        /// <param name="message">日志信息.</param>
        [Conditional("DEBUG")]
        public static void DebugWrite(LogLevel level, string message)
        {
            DateTime dateTime = DateTime.Now;
            lock (lockRoot)
            {
                LogReceiver(new LogData { Time = dateTime, Level = level, Message = message });
            }
            PushLog(dateTime, level, message);
        }

        internal static void PushLog(DateTime time, LogLevel level, string message)
        {
            lock (messageHistory)
            {
                while (messageHistory.Count >= maxHistoryCount)
                {
                    messageHistory.Dequeue();
                }
                messageHistory.Enqueue(new LogData { Time = time, Level = level, Message = message });
            }
        }

        /// <summary>
        /// 表示存储的所有历史日志的条目数.
        /// </summary>
        public static int LogHistoryCount
        {
            get => maxHistoryCount;
            set
            {
                lock (lockRoot)
                {
                    maxHistoryCount = Math.Max(1, value);
                    messageHistory.EnsureCapacity(maxHistoryCount);
                }
            }
        }
    }

}
