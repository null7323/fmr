using FMR.Core;

namespace DominoRenderer
{
    public class Module : IRenderModule
    {
        public string Description => "һ��Domino����Midi��Ⱦ��.";

        public Version APIVersion => new(1, 0, 0, 0);

        public string Name => "Domino";
    }

}
