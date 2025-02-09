using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace FMR.Core.VulkanRender
{
    /// <summary>
    /// 表示一个 Vulkan 上下文.
    /// </summary>
    public class Context
    {
        internal Vk api;
        /// <summary>
        /// 获取 Vulkan API.
        /// </summary>
        public Context()
        {
            api = Vk.GetApi();
        }
    }
}
