using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.SDLRender
{
    /// <summary>
    /// 表示2行2列的矩阵. 内部使用<see cref="Vec"/>类型.
    /// </summary>
    public struct Matrix2x2
    {
        /// <summary>
        /// 表示第一列.
        /// </summary>
        public Vec First;
        /// <summary>
        /// 表示第二列.
        /// </summary>
        public Vec Second;
        /// <summary>
        /// 使用给定向量初始化对应列.
        /// </summary>
        /// <param name="v1">第一列向量.</param>
        /// <param name="v2">第二列向量.</param>
        public Matrix2x2(Vec v1, Vec v2)
        {
            First = v1;
            Second = v2;
        }
        /// <summary>
        /// 初始化各项全为0的2x2矩阵.
        /// </summary>
        public Matrix2x2()
        {
            First = Vec.Zero;
            Second = Vec.Zero;
        }
        /// <summary>
        /// 初始化矩阵的所有值.
        /// </summary>
        /// <param name="a">用作<see cref="First"/>的第一个值.</param>
        /// <param name="b">用作<see cref="Second"/>的第一个值.</param>
        /// <param name="c">用作<see cref="First"/>的第二个值.</param>
        /// <param name="d">用作<see cref="Second"/>的第二个值.</param>
        public Matrix2x2(float a, float b, float c, float d)
        {
            First = new Vec(a, c);
            Second = new Vec(b, d);
        }
        /// <summary>
        /// 求出两矩阵的和. 此运算是可交换的.
        /// </summary>
        /// <param name="lhs">左运算矩阵.</param>
        /// <param name="rhs">右运算矩阵.</param>
        /// <returns>和矩阵.</returns>
        public static Matrix2x2 operator +(Matrix2x2 lhs, Matrix2x2 rhs)
        {
            return new Matrix2x2(lhs.First + rhs.First, lhs.Second + rhs.Second);
        }
        /// <summary>
        /// 求出矩阵的数乘.
        /// </summary>
        /// <param name="k">实数. 将<paramref name="mat"/>中每一个元素变为<paramref name="k"/>倍.</param>
        /// <param name="mat">参与运算的矩阵.</param>
        /// <returns>数乘后的矩阵.</returns>
        public static Matrix2x2 operator *(float k, Matrix2x2 mat)
        {
            return new Matrix2x2(k * mat.First, k * mat.Second);
        }
        /// <summary>
        /// 求出矩阵的数乘.
        /// </summary>
        /// <param name="mat">参与运算的矩阵.</param>
        /// <param name="k">实数. 将<paramref name="mat"/>中每一个元素变为<paramref name="k"/>倍.</param>
        /// <returns>数乘后的矩阵.</returns>
        public static Matrix2x2 operator *(Matrix2x2 mat, float k)
        {
            return new Matrix2x2(k * mat.First, k * mat.Second);
        }
        /// <summary>
        /// 求出<paramref name="mat"/>与<paramref name="vec"/>的乘积.
        /// </summary>
        /// <param name="mat">变换矩阵.</param>
        /// <param name="vec">参与运算的向量.</param>
        /// <returns>变换后的向量.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(Matrix2x2 mat, Vec vec)
        {
            Vec first = new(mat.First.X, mat.Second.X);
            Vec second = new(mat.First.Y, mat.Second.Y);
            return new Vec(Vec.Dot(first, vec), Vec.Dot(second, vec));
        }
        /// <summary>
        /// 执行矩阵相乘. 此操作是不可交换的.
        /// </summary>
        /// <param name="lhs">左操作矩阵.</param>
        /// <param name="rhs">右操作矩阵.</param>
        /// <returns>相乘后的矩阵.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator *(Matrix2x2 lhs, Matrix2x2 rhs)
        {
            Vec firstRow = new(lhs.First.X, lhs.Second.X);
            Vec secondRow = new(lhs.First.Y, lhs.Second.Y);
            return new Matrix2x2(
                Vec.Dot(firstRow, rhs.First), Vec.Dot(firstRow, rhs.Second),
                Vec.Dot(secondRow, rhs.First), Vec.Dot(secondRow, rhs.Second));
        }
        /// <summary>
        /// 获取指定位置的元素.
        /// </summary>
        /// <param name="row">行.</param>
        /// <param name="col">列.</param>
        /// <returns>指向对应元素的引用.</returns>
        public ref float this[int row, int col]
        {
            get
            {
                unsafe
                {
                    fixed (Matrix2x2* pMat = &this)
                    {
                        Vec* vecArray = (Vec*)pMat;
                        Vec* pVec = vecArray + col;
                        return ref *(((float*)pVec) + row);
                    }
                }
            }
        }
    }
}
