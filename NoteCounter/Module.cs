using FMR.Core;

namespace NoteCounter
{
    public class Module : IRenderModule
    {
        public Module()
        {
            
        }
        public string Description => "提供支持自定义内容的Midi计数器功能.\n特别鸣谢: 来自 @超盐酸钠 (原CJC) 的思路.";

        public Version APIVersion => new(1, 0, 0, 0);

        public string Name => "Note Counter";
    }
}
