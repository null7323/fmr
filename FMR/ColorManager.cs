using FMR.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FMR
{
    internal sealed class ColorManager
    {
        internal RGBAColor[] iColors;
        internal Vec4Color[] fColors;

        public static readonly uint[] DefaultColors = [
            0xFF3366FF, 0xFFFF7E33, 0xFF33FF66, 0xFFFF3381, 0xFF33E1E1, 0xFFE433E1,
            0xFF99E133, 0xFF4B33E1, 0xFFFFCC33, 0xFF33B4FF, 0xFFFF3333, 0xFF33FFB1,
            0xFFFF33CC, 0xFF4EFF33, 0xFF9933FF, 0xFFE7FF33, 0xFF3366FF, 0xFFFF7E33,
            0xFF33FF66, 0xFFFF3381, 0xFF33E1E1, 0xFFE433E1, 0xFF99E133, 0xFF4B33E1,
            0xFFFFCC33, 0xFF33B4FF, 0xFFFF3333, 0xFF33FFB1, 0xFFFF33CC, 0xFF4EFF33,
            0xFF9933FF, 0xFFE7FF33, 0xFF3366FF, 0xFFFF7E33, 0xFF33FF66, 0xFFFF3381,
            0xFF33E1E1, 0xFFE433E1, 0xFF99E133, 0xFF4B33E1, 0xFFFFCC33, 0xFF33B4FF,
            0xFFFF3333, 0xFF33FFB1, 0xFFFF33CC, 0xFF4EFF33, 0xFF9933FF, 0xFFE7FF33,
            0xFF3366FF, 0xFFFF7E33, 0xFF33FF66, 0xFFFF3381, 0xFF33E1E1, 0xFFE433E1,
            0xFF99E133, 0xFF4B33E1, 0xFFFFCC33, 0xFF33B4FF, 0xFFFF3333, 0xFF33FFB1,
            0xFFFF33CC, 0xFF4EFF33, 0xFF9933FF, 0xFFE7FF33, 0xFF3366FF, 0xFFFF7E33,
            0xFF33FF66, 0xFFFF3381, 0xFF33E1E1, 0xFFE433E1, 0xFF99E133, 0xFF4B33E1,
            0xFFFFCC33, 0xFF33B4FF, 0xFFFF3333, 0xFF33FFB1, 0xFFFF33CC, 0xFF4EFF33,
            0xFF9933FF, 0xFFE7FF33, 0xFF3366FF, 0xFFFF7E33, 0xFF33FF66, 0xFFFF3381,
            0xFF33E1E1, 0xFFE433E1, 0xFF99E133, 0xFF4B33E1, 0xFFFFCC33, 0xFF33B4FF,
            0xFFFF3333, 0xFF33FFB1, 0xFFFF33CC, 0xFF4EFF33, 0xFF9933FF, 0xFFE7FF33
        ];

        public ColorManager()
        {
            iColors = new RGBAColor[96];
            fColors = new Vec4Color[96];
            unsafe
            {
                fixed (uint* colors = DefaultColors)
                {
                    for (int i = 0; i < 96; i++)
                    {
                        iColors[i] = *(RGBAColor*)(colors + i);
                        fColors[i] = iColors[i].AsVecColor();
                    }
                }
            }
        }

        public void ReadBitmap(Bitmap bmp)
        {
            int bmpWidth = bmp.Width, bmpHeight = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmpWidth, bmpHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int numBytes = data.Stride * bmpHeight;
            RGBAColor[] pickedColors = new RGBAColor[numBytes / 4];

            unsafe
            {
                fixed (RGBAColor* pDest = pickedColors)
                {
                    RGBAColor* first = (RGBAColor*)data.Scan0;
                    Buffer.MemoryCopy(first, pDest, numBytes, numBytes);
                }
            }

            bmp.UnlockBits(data);

            for (int i = 0; i < pickedColors.Length; i++)
            {
                ref RGBAColor col = ref pickedColors[i];
                // fix color order, swap R and B.
                byte trueR = col.B;
                byte trueB = col.R;
                col.B = trueB;
                col.R = trueR;
            }

            iColors = pickedColors;
            fColors = new Vec4Color[iColors.Length];
            for (int i = 0; i < iColors.Length; i++)
            {
                fColors[i] = iColors[i].AsVecColor();
            }
        }

        public bool ReadJson(string fileName, ColorType jsonColorType)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            string colorData = File.ReadAllText(fileName);
            if (jsonColorType.HasFlag(ColorType.RGBA_Int))
            {
                return ReadIntColor(colorData);
            }
            else
            {
                return ReadFloatColor(colorData);
            }
        }

        internal bool ReadIntColor(string colorData)
        {
            RGBAColor[] lastColors = iColors;
            try
            {
                RGBAColor[]? newColors = JsonSerializer.Deserialize<RGBAColor[]>(colorData);
                if (newColors is not null)
                {
                    iColors = newColors;
                    fColors = new Vec4Color[iColors.Length];
                    for (int i = 0; i < iColors.Length; i++)
                    {
                        fColors[i] = iColors[i].AsVecColor();
                    }
                    return true;
                }
            }
            catch
            {
                iColors = lastColors;
            }
            return false;
        }

        internal bool ReadFloatColor(string colorData)
        {
            Vec4Color[] lastColors = fColors;
            try
            {
                Vec4Color[]? newColors = JsonSerializer.Deserialize<Vec4Color[]>(colorData);
                if (newColors is not null)
                {
                    fColors = newColors;
                    iColors = new RGBAColor[fColors.Length];
                    for (int i = 0; i < fColors.Length; i++)
                    {
                        iColors[i] = fColors[i].AsRGBAColorInt();
                    }
                    return true;
                }
            }
            catch
            {
                fColors = lastColors;
            }
            return false;
        }

        public void Shuffle()
        {
            Random rand = new(DateTime.Now.Millisecond);
            int numOfColors = iColors.Length;
            for (int i = 0; i < numOfColors; i++)
            {
                int x, y;
                x = rand.Next(0, numOfColors);
                do
                {
                    y = rand.Next(0, numOfColors);
                } while (y == x);

                RGBAColor col = iColors[x];
                Vec4Color fCol = fColors[x];

                iColors[x] = iColors[y];
                fColors[x] = fColors[y];

                iColors[y] = col;
                fColors[y] = fCol;
            }
        }

        public void Default()
        {
            iColors = new RGBAColor[96];
            fColors = new Vec4Color[96];
            unsafe
            {
                fixed (uint* colors = DefaultColors)
                {
                    for (int i = 0; i < 96; i++)
                    {
                        iColors[i] = *(RGBAColor*)(colors + i);
                        fColors[i] = iColors[i].AsVecColor();
                    }
                }
            }
        }

        public void SetColors(RGBAColor[] colors)
        {
            iColors = colors;
            fColors = new Vec4Color[iColors.Length];
            for (int i = 0; i < iColors.Length; i++)
            {
                fColors[i] = iColors[i].AsVecColor();
            }
        }

        public void SetColors(Vec4Color[] colors)
        {
            fColors = colors;
            iColors = new RGBAColor[fColors.Length];
            for (int i = 0; i < fColors.Length; i++)
            {
                iColors[i] = fColors[i].AsRGBAColorInt();
            }
        }

        public RGBAColor[] Colors => iColors;
        public Vec4Color[] Vec4Colors => fColors;

        public int ColorCount
        {
            get
            {
                if (iColors is null)
                {
                    return 0;
                }
                return iColors.Length;
            }
        }
    }
}
