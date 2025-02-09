using SDL3;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace FMR.Core.SDLRender
{
    /// <summary>
    /// 表示一个 SDL 纹理.
    /// </summary>
    public unsafe class Texture : IDisposable
    {
        internal SDL.SDL_Texture* tex;
        internal int width;
        internal int height;
        internal Context context;
        internal Texture(SDL.SDL_Texture* texture, int width, int height, Context context)
        {
            tex = texture;
            this.width = width;
            this.height = height;
            this.context = context;
        }

        /// <summary>
        /// 使用指定尺寸创建一个新的<see cref="Texture"/>.
        /// </summary>
        /// <param name="context">纹理所属的上下文.</param>
        /// <param name="width">纹理的宽, 单位像素.</param>
        /// <param name="height">纹理的高, 单位像素.</param>
        /// <returns>一个空的<see cref="Texture"/>.</returns>
        public static Texture Create(Context context, int width, int height)
        {
            SDL.SDL_Texture* tex = SDL.SDL_CreateTexture(context.renderer, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, width, height);
            _ = SDL.SDL_SetTextureBlendMode(tex, (uint)BlendMode.Blend);
            return new Texture(tex, width, height, context);
        }

        /// <summary>
        /// 使用指定尺寸创建一个新的<see cref="Texture"/>. 此<see cref="Texture"/>可用作渲染对象.
        /// </summary>
        /// <param name="context">纹理所属的上下文.</param>
        /// <param name="width">纹理的宽, 单位像素.</param>
        /// <param name="height">纹理的高, 单位像素.</param>
        /// <returns>一个空的<see cref="Texture"/>.</returns>
        public static Texture CreateTarget(Context context, int width, int height)
        {
            SDL.SDL_Texture* tex = SDL.SDL_CreateTexture(context.renderer, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height);
            _ = SDL.SDL_SetTextureBlendMode(tex, (uint)BlendMode.Blend);
            return new Texture(tex, width, height, context);
        }

        /// <summary>
        /// 使用给定的图像路径创建一个<see cref="Texture"/>.
        /// </summary>
        /// <param name="context">纹理所属的上下文.</param>
        /// <param name="imagePath">纹理源图像的路径.</param>
        /// <returns>含有来自<paramref name="imagePath"/>的图像的信息的<see cref="Texture"/>.</returns>
        public static Texture CreateFrom(Context context, string imagePath)
        {
            using Bitmap bmp = new(imagePath);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            nint pixels = data.Scan0;

            SDL.SDL_Surface* rawSurface = SDL.SDL_CreateSurfaceFrom(
                bmp.Width, bmp.Height, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, pixels, data.Stride);

            SDL.SDL_Surface* pSurface = SDL.SDL_ConvertSurface(rawSurface, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);

            SDL.SDL_DestroySurface(rawSurface);
            bmp.UnlockBits(data);

            SDL.SDL_Texture* tex = SDL.SDL_CreateTextureFromSurface(context.renderer, pSurface);
            SDL.SDL_DestroySurface(pSurface);

            _ = SDL.SDL_SetTextureBlendMode(tex, (uint)BlendMode.Blend);
            return new Texture(tex, bmp.Width, bmp.Height, context);
        }
        /// <summary>
        /// 将当前<see cref="Task"/>的内容复制到上下文.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw()
        {
            _ = SDL.SDL_RenderTexture(context.renderer, tex, null, null);
        }
        /// <summary>
        /// 将当前<see cref="Texture"/>内容绘制到上下文.
        /// </summary>
        /// <param name="dstRect">上下文的绘制区域.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Rect dstRect)
        {
            SDL.SDL_FRect fill = dstRect.AsSDLRectF(context.width, context.height);
            _ = SDL.SDL_RenderTexture(context.renderer, tex, null, ref fill);
        }
        /// <summary>
        /// 将当前<see cref="Texture"/>指定部分的内容绘制到上下文.
        /// </summary>
        /// <param name="dstRect">上下文的绘制区域.</param>
        /// <param name="srcRect">参与绘制的<see cref="Texture"/>区域.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Rect dstRect, Rect srcRect)
        {
            SDL.SDL_FRect dst = dstRect.AsSDLRectF(context.width, context.height);
            SDL.SDL_FRect src = srcRect.AsSDLRectF(width, height);
            _ = SDL.SDL_RenderTexture(context.renderer, tex, ref src, ref dst);
        }
        /// <summary>
        /// 将当前的<see cref="Texture"/>旋转后绘制到上下文.
        /// </summary>
        /// <param name="dstRect">上下文的绘制区域.</param>
        /// <param name="texRotation">旋转角度. 此参数为正时旋转方向为逆时针, 单位为弧度.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Rect dstRect, double texRotation)
        {
            SDL.SDL_FRect dst = dstRect.AsSDLRectF(context.width, context.height);
            texRotation = -texRotation;
            texRotation = texRotation * 180.0 / Math.PI;
            _ = SDL.SDL_RenderTextureRotated(context.renderer, tex, null, ref dst, texRotation, null, SDL.SDL_FlipMode.SDL_FLIP_NONE);
        }
        /// <summary>
        /// 设置当前纹理的过滤模式.
        /// </summary>
        /// <param name="mode">过滤模式. 此枚举不应组合.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScaleMode(ScaleMode mode)
        {
            _ = SDL.SDL_SetTextureScaleMode(tex, (SDL.SDL_ScaleMode)mode);
        }
        /// <summary>
        /// 锁定当前<see cref="Texture"/>, 并返回内部的像素数据.
        /// </summary>
        /// <returns>一个<see cref="TextureData"/>对象, 包含像素与跨度信息.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextureData Lock()
        {
            SDL.SDL_Rect rect = Rect.FillingRect.AsSDLRect(width, height);
            _ = SDL.SDL_LockTexture(tex, ref rect, out nint pixels, out int pitch);
            return new TextureData(pixels, pitch, width, height);
        }
        /// <summary>
        /// 解锁当前<see cref="Texture"/>, 像素数据不再可用.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            SDL.SDL_UnlockTexture(tex);
        }
        /// <summary>
        /// 强制更新纹理内容.
        /// </summary>
        /// <param name="pixels">更新的像素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void UpdateTexture(byte* pixels)
        {
            _ = SDL.SDL_UpdateTexture(tex, null, (nint)pixels, width * 4);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (tex != null)
            {
                SDL.SDL_DestroyTexture(tex);
            }
            tex = null;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        ~Texture()
        {
            if (tex != null)
            {
                SDL.SDL_DestroyTexture(tex);
            }
            tex = null;
        }
    }

    /// <summary>
    /// 表示纹理缩放方式.
    /// </summary>
    public enum ScaleMode
    {
        /// <summary>
        /// 只使用邻近像素进行过滤.
        /// </summary>
        Nearest,
        /// <summary>
        /// 使用线性插值的方式过滤像素.
        /// </summary>
        Linear
    }

    /// <summary>
    /// 表示用于<see cref="BlendMode"/>的混色模式.
    /// </summary>
    public enum BlendMode
    {
        /// <summary>
        /// 不进行混色.
        /// </summary>
        None = 0,
        /// <summary>
        /// 根据 Alpha 值混色.
        /// </summary>
        Blend = 1,
        /// <summary>
        /// 预乘 Alpha 通道的混色.
        /// </summary>
        PremultipliedBlend = 0x10,
        /// <summary>
        /// 相加混色.
        /// </summary>
        Add = 2,
        /// <summary>
        /// 预乘 Alpha 的加法混色.
        /// </summary>
        PremultipliedAdd = 0x20,
        /// <summary>
        /// 调制混色.
        /// </summary>
        Modulate = 4,
        /// <summary>
        /// 乘法混色.
        /// </summary>
        Multiply = 8,
        /// <summary>
        /// 反色混色.
        /// </summary>
        Reverse = -1,
        /// <summary>
        /// 不合法的混色.
        /// </summary>
        Invalid = 0x7FFFFFFF
    }

    /// <summary>
    /// 表示锁定后的<see cref="Texture"/>信息.
    /// </summary>
    public struct TextureData
    {
        internal nint data;
        internal int pitch;
        internal int width;
        internal int height;
        internal int sizeInBytes;

        internal TextureData(nint data, int pitch, int width, int height)
        {
            this.data = data;
            this.pitch = pitch;
            this.width = width;
            this.height = height;
            sizeInBytes = pitch * height;
        }

        /// <summary>
        /// 将当前的像素拷贝到<paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">目标缓冲区.</param>
        public readonly void CopyTo(nint destination)
        {
            unsafe
            {
                UnsafeMemory.Copy((byte*)destination, (byte*)data, sizeInBytes);
            }
        }
        /// <summary>
        /// 获取第一行像素的首地址.
        /// </summary>
        public readonly nint Scan0 => data;
        /// <summary>
        /// 获取每一行的总字节数.
        /// </summary>
        public readonly int Pitch => pitch;
        /// <summary>
        /// 表示<see cref="Texture"/>锁定部分的宽.
        /// </summary>
        public readonly int Width => width;
        /// <summary>
        /// 表示<see cref="Texture"/>锁定部分的高.
        /// </summary>
        public readonly int Height => height;
        /// <summary>
        /// 获取缓冲区所需的最小大小.
        /// </summary>
        public readonly int SizeInBytes => sizeInBytes;
    }

    /// <summary>
    /// 表示小型纹理图集.
    /// </summary>
    public unsafe class AtlasTexture : Texture
    {
        internal AtlasTexture(SDL.SDL_Texture* texture, int size, Context context) : base(texture, size, size, context)
        {

        }

        /// <summary>
        /// 生成一个空的纹理图集.
        /// </summary>
        /// <param name="size">纹理的大小 (单位: 边长).</param>
        /// <param name="context">SDL 上下文.</param>
        /// <returns>一个小型纹理的图集.</returns>
        public static AtlasTexture Create(AtlasTextureSize size, Context context)
        {
            int texSize = (int)size;
            SDL.SDL_Texture* tex = SDL.SDL_CreateTexture(context.renderer, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, texSize, texSize);
            return new AtlasTexture(tex, texSize, context);
        }

        /// <summary>
        /// 表示小型纹理集的尺寸.
        /// </summary>
        public enum AtlasTextureSize
        {
            /// <summary>
            /// 默认值.
            /// </summary>
            Default = 4096,
            /// <summary>
            /// 相比于默认情形扩充的尺寸.
            /// </summary>
            Extended = 8192,
            /// <summary>
            /// 支持的最大尺寸.
            /// </summary>
            Maximum = 16384
        }

    }
}
