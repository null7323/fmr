using FMR.Core;

namespace DominoRenderer
{
    public class Module : IRenderModule
    {
        public string Description => "一个Domino风格的Midi渲染器.";

        public Version APIVersion => new(1, 0, 0, 0);

        public string Name => "Domino";
    }

}
