using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpticalSimulator.Service
{
    public abstract class SkiaService
    {
        public WriteableBitmap Bitmap { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SkiaService(int width, int height)
        {
            Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            Width = width;
            Height = height;
        }

        public void Update()
        {
            Bitmap.Lock();

            using (SKSurface surface = SKSurface.Create(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul, Bitmap.BackBuffer, Width * 4))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear();
                Draw(canvas);
            }

            Bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));

            Bitmap.Unlock();
        }

        protected abstract void Draw(SKCanvas canvas);
    }
}
