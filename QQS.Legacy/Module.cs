using FMR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQS.Legacy
{
    public class Module : IRenderModule
    {
        public string Name => "QQS (Legacy)";

        public string Description => "使用旧实现完成的QMidicore Quaver Stream Renderer.";

        public Version APIVersion => new(1, 0, 0, 0);
    }
}
