using System;
using System.Drawing;
using ComputeSharp;

namespace MandelbrotSet
{
    internal class Program
    {
        static void Main()
        {

            //3840 x 2160
            int mul = 240, rx = 16, ry = 9;
            int width = rx * mul, height = ry * mul;
            using var texture = GraphicsDevice.Default.AllocateReadWriteTexture2D<Bgra32, float4>(width, height);
            GraphicsDevice.Default.For(texture.Width, texture.Height, new Mandelbrot(texture));
            texture.Save("Mandelbrot smooth " + width + "x" + height + ".png");/**/
            //Console.WriteLine("Hello World!");
            //Console.ReadKey();
        }
        /**
        static int Num(com z)
        {
            com c = new com(0f, 0f);
            for (int n = 0; n < maxLayers; n++)
            {
                if (c.SqMag > 4) return n;
                c = c * c + z;
            }
            return -1;
        }
        static Color FromNum(int n)
        {
            if (n == -1) return Color.Black;
            Color color = Color.White;
            for (int i = 0; i < n; i++)
            {
                color = Color.FromArgb(
                    Blend(color.R, BaseColor.R), 
                    Blend(color.G, BaseColor.G), 
                    Blend(color.B, BaseColor.B)
                    );
            }
            return color;
        }
        static int Blend(int a, int b)
        {
            return (int)(a * (1 - opacity) + opacity * b);
        }
        /**/
    }
    /**
#pragma warning disable IDE1006 // Naming Styles
    struct com
    {
        public float r, i;
        public com(float _r, float _i)
        {
            r = _r; i = _i;
        }
        public static com operator + (com left, com right)
        {
            return new com
            {
                r = left.r + right.r,
                i = left.i + right.i
            };
        }
        public static com operator * (com left, com right)
        {
            return new com
            {
                r = left.r * right.r - left.i * right.i,
                i = left.r * right.i + left.i * right.r
            };
        }
        public float SqMag
        {
            get
            {
                return r * r + i * i;
            }
        }
    }
#pragma warning restore IDE1006 // Naming Styles
    /**/
    [AutoConstructor]
    public readonly partial struct Mandelbrot : IComputeShader
    {
        const float esc = 65536.0f;
        public readonly IReadWriteNormalizedTexture2D<float4> texture;
        static readonly int MaxLayers = 5000;
        static readonly float opacity = 0.02f;
        static float2 corner = new(-1.6f, -0.9f);
        static readonly float2 size = new(3.2f / 1, 1.8f / 1);
        static float4 shade = new(0.999f, 0.001f, 0.001f, opacity);
        static float4 Blend(float4 A, float4 B)
        {
            return new float4
                (
                A.R * A.A * (1 - B.A) + B.R * B.A,
                A.G * A.A * (1 - B.A) + B.G * B.A,
                A.B * A.A * (1 - B.A) + B.B * B.A,
                A.A * (1 - B.A) + B.A
                );
        }
        static float4 BlendSmooth(float4 A, float4 B, float n)
        {
            return new float4
                (
                A.R * Hlsl.Pow(A.A * (1 - B.A), n) + B.R * B.A,
                A.G * Hlsl.Pow(A.A * (1 - B.A), n) + B.G * B.A,
                A.B * Hlsl.Pow(A.A * (1 - B.A), n) + B.B * B.A,
                Hlsl.Pow(A.A * (1 - B.A), n) + B.A
                );
        }
        static float2 Mul(float2 left, float2 right)
        {
            return new Float2(left.X * right.X - left.Y * right.Y, left.X * right.Y + left.Y * right.X);
        }
        static float2 Inv(float2 Z)
        {
            return new float2(Z.X / SqMag(Z), -Z.Y / SqMag(Z));
        }
        static float SqMag(float2 Z)
        {
            return Z.X * Z.X + Z.Y * Z.Y;
        }
        public void Execute()
        {
            float4 color = new(0.999f, 0.999f, 0.999f, 1.0f);
            float cR = corner.X + size.X * ThreadIds.X / texture.Width;
            float cI = corner.Y + size.Y * ThreadIds.Y / texture.Height;
            float2 Z = new float2(0.0f, 0.0f);
            float2 C = new float2(cR, cI);
            int n;
            for (n = 0; n < MaxLayers; n++)
            {
                if (SqMag(Z) > esc)
                {
                    texture[ThreadIds.XY].RGBA = BlendSmooth(color, shade, Hlsl.Log2(Hlsl.Log2(SqMag(Z)) / 2f) + 4.0f);
                    break;
                }
                Z = Mul(Z, Z) + C;
                color = Blend(color, shade);
            }
            //float Sn = n - Hlsl.Log2(Hlsl.Log2(SqMag(Z)) / 2f) + 4.0f;
            if (SqMag(Z) <= esc)
            {
                texture[ThreadIds.XY].RGBA = new(0.0f, 0.0f, 0.0f, 1.0f);
            }
        }
    }
}
