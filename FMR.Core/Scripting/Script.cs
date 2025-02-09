using Microsoft.CSharp;
using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SDL3;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.Scripting
{
    /// <summary>
    /// 表示初始化<see cref="Script"/>对象时的脚本语言类型.
    /// </summary>
    public enum LanguageType
    {
        /// <summary>
        /// 指示<see cref="Script"/>根据文件扩展名自动选择脚本语言类型.
        /// </summary>
        DetermineByExtensionName = CSharp | VisualBasic,
        /// <summary>
        /// 指示<see cref="Script"/>使用<see cref="CSharpCodeProvider"/>进行编译.
        /// </summary>
        CSharp = 1,
        /// <summary>
        /// 指示<see cref="Script"/>使用<see cref="VBCodeProvider"/>进行编译.
        /// </summary>
        VisualBasic = 2,
    }
    /// <summary>
    /// 表示一个脚本文件.
    /// </summary>
    public class Script
    {
        internal Assembly assembly;

        /// <summary>
        /// 初始化一个新的<see cref="Script"/>对象.
        /// </summary>
        /// <param name="fileName">要编译和使用的Script文件.</param>
        /// <param name="language">Script文件的语言类型. 支持C#与Visual Basic.</param>
        /// <param name="dependencyLocations">所有可能的依赖程序集的位置.</param>
        public Script(string fileName, LanguageType language, string[] dependencyLocations)
        {
            CodeDomProvider? provider = null;
            switch (language)
            {
                case LanguageType.CSharp:
                    provider = new CSharpCodeProvider();
                    break;
                case LanguageType.VisualBasic:
                    provider = new VBCodeProvider();
                    break;
                default:
                    if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        provider = new CSharpCodeProvider();
                    }
                    else if (fileName.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
                    {
                        provider = new VBCodeProvider();
                    }
                    break;

            }
            if (provider is null)
            {
                ThrowHelper.Throw("Bad script name: cannot determine which compiler to use.");
                return;
            }
            CompilerParameters parameters = new()
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                CompilerOptions = "/optimize /unsafe /nostdlib"
            };
            parameters.ReferencedAssemblies.AddRange(dependencyLocations);
            CompilerResults result = provider.CompileAssemblyFromFile(parameters, fileName);
            if (result.Errors.Count > 0)
            {
                StringBuilder builder = new();
                builder.Append("Compilation failed. Error details:\n");
                foreach (var error in result.Errors)
                {
                    builder.AppendLine(error.ToString());
                }
                Logging.Write(LogLevel.Error, builder.ToString());
                ThrowHelper.Throw(builder.ToString());
            }
            assembly = result.CompiledAssembly;
        }

        /// <summary>
        /// 编译后的程序集.
        /// </summary>
        public Assembly CompiledAssembly => assembly;
    }

    /// <summary>
    /// 初始化必需的依赖. 此类只与Script插件相关, 不需要Script编写者调用.
    /// </summary>
    public static class Initializer
    {
        /// <summary>
        /// 判断所需依赖是否已经初始化.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// 初始化所有依赖, 并将<see cref="Initialized"/>设置为<see langword="true"/>.
        /// </summary>
        public static void Init()
        {
            if (Initialized) return;
            _ = SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);
            _ = SDL.SDL_SetHint(SDL.SDL_HINT_FRAMEBUFFER_ACCELERATION, "1");

            Logging.Write(LogLevel.Info, "Script dependencies initialized.");
            Initialized = true;
        }

        /// <summary>
        /// 释放所有依赖, 并将<see cref="Initialized"/>设置为<see langword="false"/>.
        /// </summary>
        public static void Dispose()
        {
            if (!Initialized) return;
            SDL.SDL_Quit();
            Initialized = false;
        }
    }
}
