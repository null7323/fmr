using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.GLRender
{
    /// <summary>
    /// 表示一个OpenGL 2D纹理.
    /// </summary>
    public class Texture2D : IDisposable
    {
        /// <summary>
        /// 初始化一个新的<see cref="Texture2D"/>实例. 这会创建一个新的OpenGL纹理.
        /// </summary>
        public Texture2D()
        {
            texId = GL.GenTexture();
            width = height = 0;
        }
        internal Texture2D(int id, int width, int height)
        {
            texId = id;
            this.width = width;
            this.height = height;
        }
        /// <summary>
        /// 从<paramref name="image"/>生成OpenGL纹理.
        /// </summary>
        /// <param name="image">像素源信息.</param>
        /// <returns>一个OpenGL纹理.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture2D FromImage(Bitmap image)
        {
            int id = GL.GenTexture();
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, 
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);
            return new Texture2D(id, image.Width, image.Height);
        }
        /// <summary>
        /// 从指定的RGBA颜色生成OpenGL纹理.
        /// </summary>
        /// <param name="pixels">像素数据.</param>
        /// <param name="width">源像素数据的宽.</param>
        /// <param name="height">源像素数据的高.</param>
        /// <returns>一个OpenGL纹理.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture2D FromPixels(IntPtr pixels, int width, int height)
        {
            int id = GL.GenTexture();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            return new Texture2D(id, width, height);
        }
        /// <summary>
        /// 设置OpenGL纹理过滤模式.
        /// </summary>
        /// <param name="minFilter">拉伸时的过滤方式.</param>
        /// <param name="magFilter">缩小时的过滤方式.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFilterMode(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        }
        /// <summary>
        /// 设置纹理的环绕行为.
        /// </summary>
        /// <param name="wrapMode">纹理的环绕模式.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void SetWrapMode(TextureWrapMode wrapMode)
        {
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
        }
        /// <summary>
        /// 将当前OpenGL纹理绑定到OpenGL上.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, texId);
        }
        /// <summary>
        /// 启用指定位置的<see cref="Texture2D"/>纹理.
        /// </summary>
        /// <param name="unit">指定启用的单位.</param>
        public static void Activate(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (texId != 0)
            {
                GL.DeleteTexture(texId);
            }
            width = height = 0;
            texId = 0;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        ~Texture2D()
        {
            if (texId != 0)
            {
                GL.DeleteTexture(texId);
            }
            width = height = 0;
            texId = 0;
        }
        /// <summary>
        /// 获取纹理的宽.
        /// </summary>
        public int Width => width;
        /// <summary>
        /// 获取纹理的高.
        /// </summary>
        public int Height => height;

        internal int texId;
        internal int width, height;
    }
}
