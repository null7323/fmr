using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core
{
    /// <summary>
    /// 提供渲染模块资源操作的支持.
    /// </summary>
    public class AssetManager
    {
        internal string assemblyName;

        /// <summary>
        /// 使用指定的<see cref="Assembly"/>初始化当前的<see cref="AssetManager"/>实例.
        /// </summary>
        /// <param name="module">模块的程序集.</param>
        public AssetManager(Assembly module)
        {
            string location = module.Location;
            assemblyName = Path.GetFileNameWithoutExtension(location);
        }

        /// <summary>
        /// 获取指定资源文件的路径.
        /// </summary>
        /// <param name="assetFileName">资源文件名.</param>
        /// <returns>资源路径.</returns>
        public string GetPathOf(string assetFileName)
        {
            string fileName = Path.GetFileName(assetFileName);
            return $"Assets\\{assemblyName}\\{fileName}";
        }

        /// <summary>
        /// 获取当前程序集下的资源所在文件夹.
        /// </summary>
        public string AssetPath => $"Assets\\{assemblyName}";
    }
}
