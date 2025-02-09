using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core
{
    /// <summary>
    /// 提供一系列简化<see cref="Assembly"/>操作的方法.
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// 加载程序集.
        /// </summary>
        /// <param name="path">程序集路径</param>
        /// <returns>加载的程序集.</returns>
        public static Assembly Load(string path)
        {
#if UNSAFE_LOAD
            return Assembly.UnsafeLoadFrom(path);
#else
            return Assembly.LoadFrom(path);
#endif
        }

        /// <summary>
        /// 从给定程序集加载实现某一接口的所有类型的实例.
        /// </summary>
        /// <remarks>
        /// 构造实例时，调用无参构造函数.
        /// </remarks>
        /// <typeparam name="T">接口类型.</typeparam>
        /// <param name="assembly">给定的程序集.</param>
        /// <returns>所有不为<see langword="null"/>的实例.</returns>
        public static T[] GetImplementationsOf<T>(Assembly assembly)
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                ThrowHelper.ThrowArgument("Type argument should be an interface type.", nameof(T));
            }

            Type[] publicTypes = assembly.GetExportedTypes();
            List<T> instanceList = [];
            foreach (Type type in publicTypes)
            {
                Type[] implementedInterfaces = type.GetInterfaces();
                if (implementedInterfaces.Contains(interfaceType))
                {
                    T? instance = (T?)Activator.CreateInstance(type);
                    if (instance is not null)
                    {
                        instanceList.Add(instance);
                    }
                }
            }
            return [.. instanceList];
        }
    }
}
