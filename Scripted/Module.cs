using FMR.Core;
using FMR.Core.Scripting;
using System;

namespace Scripted
{
    public class Module : IRenderModule
    {
        public Module()
        {
            Initializer.Init();
        }
        public string Description => "提供简易的脚本支持.";

        public Version APIVersion => new(1, 0, 0, 0);

        public string Name => "Scripted";
    }
}
