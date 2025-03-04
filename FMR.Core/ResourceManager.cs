using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core
{
    /// <summary>
    /// 提供渲染模块资源操作的支持.
    /// </summary>
    public class ResourceManager
    {
        internal string assemblyName;

        /// <summary>
        /// 使用指定的<see cref="Assembly"/>初始化当前的<see cref="ResourceManager"/>实例.
        /// </summary>
        /// <param name="module">模块的程序集.</param>
        public ResourceManager(Assembly module)
        {
            string location = module.Location;
            assemblyName = Path.GetFileNameWithoutExtension(location);
        }

        /// <summary>
        /// 获取指定资源文件的路径.
        /// </summary>
        /// <param name="resourceName">资源文件名.</param>
        /// <returns>资源路径.</returns>
        public string GetPathOf(string resourceName)
        {
            string fileName = Path.GetFileName(resourceName);
            return $"{ResourcePath}\\{fileName}";
        }

        /// <summary>
        /// 获取当前资源文件夹下的所有子文件夹.
        /// </summary>
        /// <returns>子文件夹路径.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetDirectories()
        {
            return Directory.GetDirectories(ResourcePath);
        }

        /// <summary>
        /// 获取当前程序集下的资源所在文件夹.
        /// </summary>
        public string ResourcePath => $"Assets\\{assemblyName}";
    }
}
