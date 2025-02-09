using SDL3;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.SDLRender
{
    /// <summary>
    /// 表示一个二维向量.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec
    {
        /// <summary>
        /// 表示<see cref="Vec"/>的第一个分量.
        /// </summary>
        public float X;
        /// <summary>
        /// 表示<see cref="Vec"/>的第二个分量.
        /// </summary>
        public float Y;
        /// <summary>
        /// 表示一个零向量.
        /// </summary>
        public static readonly Vec Zero = new();
        /// <summary>
        /// 表示沿X正方向的单位向量.
        /// </summary>
        public static readonly Vec UnitX = new(1.0f, 0.0f);
        /// <summary>
        /// 表示沿Y正方向的单位向量.
        /// </summary>
        public static readonly Vec UnitY = new(0.0f, 1.0f);
        /// <summary>
        /// 初始化一个零向量.
        /// </summary>
        public Vec()
        {
            X = Y = 0.0f;
        }
        /// <summary>
        /// 使用给定值初始化<see cref="Vec"/>的每一个分量.
        /// </summary>
        /// <param name="v">分量的值.</param>
        public Vec(float v)
        {
            X = Y = v;
        }
        /// <summary>
        /// 使用给定值依次初始化<see cref="Vec"/>的每一个分量.
        /// </summary>
        /// <param name="x">初始化<see cref="X"/>的值.</param>
        /// <param name="y">初始化<see cref="Y"/>的值.</param>
        public Vec(float x, float y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// 执行向量加法.
        /// </summary>
        /// <returns>和向量.</returns>
        public static Vec operator +(Vec v1, Vec v2)
        {
            return new(v1.X + v2.X, v1.Y + v2.Y);
        }
        /// <summary>
        /// 执行向量减法.
        /// </summary>
        /// <returns>差向量.</returns>
        public static Vec operator -(Vec v1, Vec v2)
        {
            return new(v1.X - v2.X, v1.Y - v2.Y);
        }
        /// <summary>
        /// 求出给定向量的相反向量.
        /// </summary>
        /// <param name="v">原向量.</param>
        /// <returns>向量<paramref name="v"/>的相反向量.</returns>
        public static Vec operator -(Vec v)
        {
            return new(-v.X, -v.Y);
        }
        /// <summary>
        /// 求出给定向量的数乘.
        /// </summary>
        /// <param name="k">标量, 将向量长度变化为<paramref name="k"/>倍.</param>
        /// <param name="v">参与运算的向量.</param>
        /// <returns>数乘后的向量.</returns>
        public static Vec operator *(float k, Vec v)
        {
            return new(k * v.X, k * v.Y);
        }
        /// <summary>
        /// 判断<paramref name="v1"/>与<paramref name="v2"/>是否是相等向量.
        /// </summary>
        /// <param name="v1">第一个向量.</param>
        /// <param name="v2">第二个向量.</param>
        /// <returns>如果<paramref name="v1"/>与<paramref name="v2"/>相等, 返回<see langword="true"/>, 否则返回<see langword="false"/>.</returns>
        public static bool operator==(Vec v1, Vec v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }
        /// <summary>
        /// 判断<paramref name="v1"/>与<paramref name="v2"/>是否不是相等向量.
        /// </summary>
        /// <param name="v1">第一个向量.</param>
        /// <param name="v2">第二个向量.</param>
        /// <returns>如果<paramref name="v1"/>与<paramref name="v2"/>不相等, 返回<see langword="true"/>, 否则返回<see langword="false"/>.</returns>
        public static bool operator!=(Vec v1, Vec v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }
        /// <summary>
        /// 求出两向量的点乘.
        /// </summary>
        /// <param name="v1">第一个向量.</param>
        /// <param name="v2">第二个向量.</param>
        /// <returns><paramref name="v1"/>与<paramref name="v2"/>的内积.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vec v1, Vec v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }
        /// <summary>
        /// 将向量沿逆时针旋转<paramref name="angle"/>角度.
        /// </summary>
        /// <param name="angle">旋转角, 单位是弧度.</param>
        /// <returns>旋转后的向量.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vec Rotate(double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double x = X * cos - Y * sin;
            double y = Y * cos + X * sin;
            return new((float)x, (float)y);
        }
        /// <summary>
        /// 求出此向量的模.
        /// </summary>
        /// <returns>此向量的模长.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double Norm()
        {
            return Math.Sqrt((double)X * X + (double)Y * Y);
        }

        /// <summary>
        /// 将<see cref="ValueTuple{T1, T2}"/>类型表示的元组转换为<see cref="Vec"/>.
        /// </summary>
        /// <param name="vec">元组形式的平面向量.</param>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vec((float X, float Y) vec)
        {
            return new Vec(vec.X, vec.Y);
        }

        /// <summary>
        /// 将<see cref="Vec"/>类型表示的向量转换为元组.
        /// </summary>
        /// <param name="vec"><see cref="Vec"/>形式的向量.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static implicit operator (float X, float Y)(Vec vec)
        {
            return (vec.X, vec.Y);
        }

        /// <summary>
        /// 获取向量的元素.
        /// </summary>
        /// <param name="index">元素下标. 只能为0或1.</param>
        /// <returns>指向所求元素的引用.</returns>
        public ref float this[int index]
        {
            get
            {
                unsafe
                {
                    fixed (Vec* pVec = &this)
                    {
                        return ref *(((float*)pVec) + index);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            if (obj is Vec vec)
            {
                return this == vec;
            }
            return false;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// 表示一个矩形. 各项的取值范围都应该在[0.0f, 1.0f]内.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Rect
    {
        /// <summary>
        /// 表示<see cref="Rect"/>左上顶点的位置.
        /// </summary>
        [FieldOffset(0)]
        public Vec Position;
        /// <summary>
        /// 表示<see cref="Rect"/>的大小.
        /// </summary>
        [FieldOffset(8)]
        public Vec Size;
        /// <summary>
        /// 表示<see cref="Rect"/>左上顶点的X坐标.
        /// </summary>
        [FieldOffset(0)]
        public float X;
        /// <summary>
        /// 表示<see cref="Rect"/>左上顶点的Y坐标.
        /// </summary>
        [FieldOffset(4)]
        public float Y;
        /// <summary>
        /// 表示<see cref="Rect"/>的宽度.
        /// </summary>
        [FieldOffset(8)]
        public float Width;
        /// <summary>
        /// 表示<see cref="Rect"/>的高度.
        /// </summary>
        [FieldOffset(12)]
        public float Height;
        /// <summary>
        /// 初始化新的<see cref="Rect"/>实例.
        /// </summary>
        /// <param name="x">X 坐标.</param>
        /// <param name="y">Y 坐标</param>
        /// <param name="w">宽度.</param>
        /// <param name="h">高度.</param>
        public Rect(float x, float y, float w, float h)
        {
            X = x; Y = y; Width = w; Height = h;
        }
        /// <summary>
        /// 初始化新的<see cref="Rect"/>实例.
        /// </summary>
        /// <param name="position">矩形的左上顶点位置.</param>
        /// <param name="size">矩形的大小.</param>
        public Rect(Vec position, Vec size)
        {
            Position = position;
            Size = size;
        }
        /// <summary>
        /// 表示填充用的矩形.
        /// </summary>
        public static readonly Rect FillingRect = new(0.0f, 0.0f, 1.0f, 1.0f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SDL.SDL_FRect AsSDLRectF(int parentWidth, int parentHeight)
        {
            return new SDL.SDL_FRect
            {
                x = parentWidth * X,
                y = parentHeight * Y,
                w = parentWidth * Width,
                h = parentHeight * Height
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SDL.SDL_Rect AsSDLRect(int parentWidth, int parentHeight)
        {
            return new SDL.SDL_Rect
            {
                x = (int)(parentWidth * X),
                y = (int)(parentHeight * Y),
                w = (int)(parentWidth * Width),
                h = (int)(parentHeight * Height)
            };
        }
    }
}
