using FMR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexturedRenderer
{
    public class Module : IRenderModule
    {
        public string Name => "Textured";

        public string Description => "֧���������Ⱦ��.";

        public Version APIVersion => new(1, 0, 0, 0);
    }
}
