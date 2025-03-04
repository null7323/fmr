using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.GLRender
{
    /// <summary>
    /// 表示一个OpenGL 2D纹理数组.
    /// </summary>
    public class Texture2DArray : IDisposable
    {
        /// <summary>
        /// 初始化一个新的<see cref="Texture2DArray"/>实例, 这会生成一个纹理.
        /// </summary>
        /// <remarks>
        /// 生成的纹理默认使用线性过滤.
        /// </remarks>
        /// <param name="width">纹理的宽度.</param>
        /// <param name="height">纹理的高度.</param>
        /// <param name="depth">纹理的个数.</param>
        public Texture2DArray(int width, int height, int depth)
        {
            texArrayId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2DArray, texArrayId);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, depth);
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        /// <summary>
        /// 设置OpenGL纹理过滤模式.
        /// </summary>
        /// <param name="minFilter">拉伸时的过滤方式.</param>
        /// <param name="magFilter">缩小时的过滤方式.</param>
        public void SetFilterMode(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GL.BindTexture(TextureTarget.Texture2DArray, texArrayId);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        /// <summary>
        /// 将当前纹理数组绑定到OpenGL.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2DArray, texArrayId);
        }

        /// <summary>
        /// 将<paramref name="pixels"/>的内容拷贝到当前<see cref="Texture2DArray"/>的指定层.
        /// </summary>
        /// <param name="pixels">像素内容.</param>
        /// <param name="level">层级.</param>
        public void CopyTexImage(IntPtr pixels, int level)
        {
            GL.BindTexture(TextureTarget.Texture2DArray, texArrayId);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, width, height, level, PixelFormat.Rgba, PixelType.Byte, pixels);
        }

        internal int texArrayId;
        internal int width, height;
        internal int depth;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (texArrayId != 0)
            {
                GL.DeleteTexture(texArrayId);
            }
            texArrayId = 0;
            width = 0;
            height = 0;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~Texture2DArray()
        {
            if (texArrayId != 0)
            {
                GL.DeleteTexture(texArrayId);
            }
            texArrayId = 0;
            width = 0; height = 0;
        }
    }
}
