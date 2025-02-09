using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL3;

namespace FMR.Core.SDLRender
{
    /// <summary>
    /// 表示用于处理 SDL 事件的类型.
    /// </summary>
    public class EventHandler
    {
        internal Context context;
        internal EventHandler(Context renderer)
        {
            context = renderer;
        }
        /// <summary>
        /// 执行退出的所有事件.
        /// </summary>
        public void OnQuit()
        {
            onQuit(context);
        }
        internal OnQuitEvent onQuit = (renderer) => { };
    }

    /// <summary>
    /// 表示处理退出事件的类型.
    /// </summary>
    /// <param name="renderer"></param>
    public delegate void OnQuitEvent(Context renderer);
}
