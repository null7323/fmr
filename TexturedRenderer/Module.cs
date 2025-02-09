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

        public string Description => "支持纹理的渲染器.";

        public Version APIVersion => new(1, 0, 0, 0);
    }
}
