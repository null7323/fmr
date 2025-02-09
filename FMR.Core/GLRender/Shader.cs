using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.GLRender
{
    /// <summary>
    /// 表示OpenGL的单个着色器对象.
    /// </summary>
    public class Shader : IDisposable
    {
        /// <summary>
        /// 初始化一个新的<see cref="Shader"/>.
        /// </summary>
        /// <param name="sourceCode">着色器的源码.</param>
        /// <param name="shaderType">着色器的类型.</param>
        public Shader(string sourceCode, ShaderType shaderType)
        {
            type = shaderType;
            shader = GL.CreateShader(shaderType);
            GL.ShaderSource(shader, sourceCode);
            GL.CompileShader(shader);
            string info = GL.GetShaderInfoLog(shader);
            if (info != string.Empty)
            {
                Logging.Write(LogLevel.Error, $"Failed to compile shader.\n{info}");
                ThrowHelper.ThrowShaderCompilationFailed(info);
            }
        }
        /// <summary>
        /// 删除当前<see cref="Shader"/>内容.
        /// </summary>
        public void Delete()
        {
            if (shader != 0)
            {
                GL.DeleteShader(shader);
            }
            shader = 0;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 表示当前<see cref="Shader"/>的着色器类型.
        /// </summary>
        public ShaderType Type => type;

        internal ShaderType type;
        internal int shader;
        /// <inheritdoc/>
        ~Shader()
        {
            Delete();
        }
    }

    /// <summary>
    /// 表示一个完整的OpenGL渲染管线.
    /// </summary>
    public class Program : IDisposable
    {
        /// <summary>
        /// 使用给定的着色器链接得到一个完整的着色器.
        /// </summary>
        /// <param name="shaders">要链接的着色器.</param>
        public Program(params Shader[] shaders)
        {
            shaderProgram = GL.CreateProgram();
            foreach (Shader shader in shaders)
            {
                GL.AttachShader(shaderProgram, shader.shader);
            }
            GL.LinkProgram(shaderProgram);
            string info = GL.GetProgramInfoLog(shaderProgram);
            if (info != string.Empty)
            {
                Logging.Write(LogLevel.Error, $"Failed to link program.\n{info}");
            }
            foreach (Shader shader in shaders)
            {
                GL.DetachShader(shaderProgram, shader.shader);
            }
        }
        /// <summary>
        /// 使用给定的程序.
        /// </summary>
        /// <param name="program">要使用的着色器程序.</param>
        public static void Use(Program? program)
        {
            GL.UseProgram(program != null ? program.shaderProgram : 0);
        }
        /// <summary>
        /// 删除当前着色器程序.
        /// </summary>
        public void Delete()
        {
            if (shaderProgram != 0)
            {
                GL.DeleteProgram(shaderProgram);
            }
            shaderProgram = 0;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 使用当前着色器程序.
        /// </summary>
        public void UseCurrentProgram()
        {
            GL.UseProgram(shaderProgram);
        }
        /// <inheritdoc/>
        ~Program()
        {
            Delete();
        }
        internal int shaderProgram;
    }

    /// <summary>
    /// 表示编译OpenGL着色器出错的异常.
    /// </summary>
    public class ShaderCompilationFailedException : Exception
    {
        /// <summary>
        /// 使用空消息初始化当前异常.
        /// </summary>
        public ShaderCompilationFailedException() : this("Compilation failed.")
        {

        }
        /// <summary>
        /// 使用给定异常消息处理当前异常.
        /// </summary>
        /// <param name="message">异常消息.</param>
        public ShaderCompilationFailedException(string message) : base(message)
        {

        }
        /// <summary>
        /// 使用给定的异常消息初始化OpenGL着色器编译异常.
        /// </summary>
        /// <param name="message">异常消息.</param>
        /// <param name="innerException">内部异常.</param>
        public ShaderCompilationFailedException(string message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
