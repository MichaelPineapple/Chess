using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace chess
{
    public static class Toolbox
    {
        public static void rIMAGE(TysonBitmap image, double x, double y, double w, double h)
        {
            image.Render(x, y, w, h, Color.White);
        }

        public const double TwoPi = Math.PI * 2, piOverTwo = Math.PI / 2;
        public static void rCIRCLE(double x, double y, double radius, int triangleAmount, Color hue)
        {
            List<Vector2d> vertz = new List<Vector2d>();
            for (int i = 0; i <= triangleAmount; i++)
            {
                vertz.Add(new Vector2d(
                    x + (radius * Math.Cos(i * TwoPi / triangleAmount)),
                    y + (radius * Math.Sin(i * TwoPi / triangleAmount))));
            }
            vertz.Add(new Vector2d(x, y));
            rSHAPE(hue, PrimitiveType.TriangleFan, vertz.ToArray());
        }
        public static void rCIRCLE(double x, double y, double radius, Color hue)
        {
            rCIRCLE(x, y, radius, 20, hue);
        }
        public static void rCIRCLE(Vector2d p, double radius, Color hue)
        {
            rCIRCLE(p.X, p.Y, radius, hue);
        }

        public static void rBOX(double _x, double _y, double _size, Color _c)
        {
            rBOX(_x, _y, _size, _size, _c);
        }
        public static void rBOX(Vector2d p, double _size, Color _c)
        {
            rBOX(p.X, p.Y, _size, _size, _c);
        }
        public static void rBOX(Vector2d p, double _w, double _h, Color _c)
        {
            rBOX(p.X, p.Y, _w, _h, _c);
        }
        public static void rBOX(double _x, double _y, double _w, double _h, Color _c)
        {
            rBOX(_x, _y, _w, _h, false, _c);
        }
        public static void rBOX(double _x, double _y, double _w, double _h, bool centered, Color _c)
        {
            Vector2d[] vectList = new Vector2d[4];
            if (centered)
            {
                double w = _w / 2, h = _h / 2;
                vectList[0] = new Vector2d(_x - w, _y + h);
                vectList[1] = new Vector2d(_x - w, _y - h);
                vectList[2] = new Vector2d(_x + w, _y - h);
                vectList[3] = new Vector2d(_x + w, _y + h);
            }
            else
            {
                vectList[0] = new Vector2d(_x, _y);
                vectList[1] = new Vector2d(_x + _w, _y);
                vectList[2] = new Vector2d(_x + _w, _y + _h);
                vectList[3] = new Vector2d(_x, _y + _h);
            }
            rSHAPE(_c, PrimitiveType.Quads, vectList);
        }
        public static void rSHAPE(Color _c, PrimitiveType _pt, params Vector2d[] _v)
        {
            GL.Enable(EnableCap.Blend);
            GL.Begin(_pt);
            GL.Color4(_c);
            for (int i = 0; i < _v.Length; i++) V(_v[i]);
            GL.End();
            GL.Disable(EnableCap.Blend);
        }
        private static void V(double _x, double _y)
        {
            GL.Vertex2(_x, _y);
        }
        private static void V(Vector2d _p)
        {
            V(_p.X, _p.Y);
        }
    }

    public class TysonBitmap
    {
        int image_id = 0;

        public TysonBitmap(string ImageDirectory)
        {
            Bitmap image = new Bitmap(ImageDirectory);
            GL.GenTextures(1, out image_id);
            BitmapData bitmapdata;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            bitmapdata = image.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, image_id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                (OpenTK.Graphics.OpenGL.PixelFormat)(int)All.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
        }
        public TysonBitmap(Bitmap image)
        {
            GL.GenTextures(1, out image_id);
            BitmapData bitmapdata;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            bitmapdata = image.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, image_id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                (OpenTK.Graphics.OpenGL.PixelFormat)(int)All.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
        }


        public void Render(double x, double y, double w, double h, Color c)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, this.image_id);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(c);
            GL.TexCoord2(0, 1); GL.Vertex2(x - w, y + h);
            GL.TexCoord2(1, 1); GL.Vertex2(x + w, y + h);
            GL.TexCoord2(1, 0); GL.Vertex2(x + w, y - h);
            GL.TexCoord2(0, 0); GL.Vertex2(x - w, y - h);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
            GL.Flush();
        }

        public int getID()
        {
            return this.image_id;
        }
    }

}

