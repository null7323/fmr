
using FMR.Core;

namespace NativeRenderer
{
    public class Module : IRenderModule
    {
        public string Description => "提供对本机代码渲染的支持.";

        public Version APIVersion => new(1, 0, 0, 0);

        public string Name => "Native";
    }

}
