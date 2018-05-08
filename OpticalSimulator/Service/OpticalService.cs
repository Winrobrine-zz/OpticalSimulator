using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpticalSimulator.Service
{
    public enum OpticalType
    {
        oga,
        bolga,
        bollen,
        olen,
    }

    public class OpticalService : SkiaService
    {
        private OpticalType optical;
        private string objectPath;
        private SKBitmap opticalBitmap, objectBitmap;
        private SKBitmap horizontalBitmap, verticalBitmap;
        private SKBitmap reverseBitmap;

        public OpticalType Optical
        {
            get { return optical; }
            set
            {
                optical = value;
                opticalBitmap = GetOpticalBitmap(optical);
            }
        }
        public string ObjectPath
        {
            get { return objectPath; }
            set
            {
                objectPath = value;
                objectBitmap = GetBitmapByPath(objectPath);
                horizontalBitmap = HorizontalBitmap(objectBitmap);
                verticalBitmap = VerticalBitmap(objectBitmap);
                reverseBitmap = ReverseBitmap(objectBitmap);
            }
        }
        public float A { get; set; }
        public float B { get; set; }
        public float F { get; set; }
        public float ObjectSize { get; set; }
        public float ImageSize { get; set; }
        public float M { get; set; }

        private SKPaint linePaint, pointPaint, imagePaint;

        public OpticalService(int width, int height) : base(width, height)
        {
            opticalBitmap = GetOpticalBitmap(Optical);
            ObjectSize = 100;
            linePaint = new SKPaint() { StrokeWidth = 2 };
            pointPaint = new SKPaint() { StrokeWidth = 10 };
            imagePaint = new SKPaint() { ColorFilter = SKColorFilter.CreateBlendMode(SKColors.White.WithAlpha(192), SKBlendMode.DstIn) };
        }

        private SKBitmap GetOpticalBitmap(OpticalType type)
        {
            using (Stream stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream(string.Format("OpticalSimulator.Image.{0}.png", type.ToString())))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                SKBitmap bitmap = SKBitmap.Decode(skStream);
                return bitmap.Resize(new SKImageInfo(bitmap.Width * 2, bitmap.Height * 2), SKBitmapResizeMethod.Lanczos3);
            }
        }

        private SKBitmap GetBitmapByPath(string path)
        {
            if (!File.Exists(path))
                return null;
            using (Stream stream = new StreamReader(path).BaseStream)
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                return SKBitmap.Decode(skStream);
            }
        }

        private SKBitmap HorizontalBitmap(SKBitmap bitmap)
        {
            SKColor[] pixels = bitmap.Pixels;

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width / 2; j++)
                {
                    SKColor tmp = pixels[i * bitmap.Width + j];
                    pixels[i * bitmap.Width + j] = pixels[i * bitmap.Width + (bitmap.Width - j - 1)];
                    pixels[i * bitmap.Width + (bitmap.Width - j - 1)] = tmp;
                }
            }

            return new SKBitmap(bitmap.Width, bitmap.Height) { Pixels = pixels };
        }

        private SKBitmap VerticalBitmap(SKBitmap bitmap)
        {
            SKColor[] pixels = bitmap.Pixels;

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height / 2; j++)
                {
                    SKColor tmp = pixels[i + j * bitmap.Width];
                    pixels[i + j * bitmap.Width] = pixels[i + (bitmap.Height - j - 1) * bitmap.Width];
                    pixels[i + (bitmap.Height - j - 1) * bitmap.Width] = tmp;
                }
            }

            return new SKBitmap(bitmap.Width, bitmap.Height) { Pixels = pixels };
        }

        private SKBitmap ReverseBitmap(SKBitmap bitmap)
        {
            return new SKBitmap(bitmap.Width, bitmap.Height) { Pixels = bitmap.Pixels.Reverse().ToArray() };
        }

        protected override void Draw(SKCanvas canvas)
        {
            // B값
            if (Optical == OpticalType.oga || Optical == OpticalType.bollen)
                B = A * F / (A - F);
            else if (Optical == OpticalType.bolga || Optical == OpticalType.olen)
                B = A * -F / (A + F);

            // 배율
            M = B / A;
            // 상 크기
            ImageSize = ObjectSize * Math.Abs(M);

            // 원점
            SKPoint originPoint = new SKPoint(Width / 2, Height / 2);

            // 초점, 구심 좌표
            SKPoint[] xPoints = new SKPoint[2];
            // 물체 좌표
            SKPoint objPoint = originPoint + new SKPoint(-A, -ObjectSize);

            // 1번 광선 경로
            SKPoint[] points_1 = new SKPoint[3];
            points_1[0] = objPoint;
            points_1[1] = originPoint + new SKPoint(0, -ObjectSize);

            // 2번 광선 경로
            SKPoint[] points_2 = new SKPoint[3];
            points_2[0] = objPoint;

            // 3번 광선 경로
            SKPoint[] points_3 = new SKPoint[2];

            // 4번 광선 경로
            SKPoint[] points_4 = new SKPoint[3];
            points_4[0] = objPoint;
            points_4[1] = originPoint;

            // 1번 광선 경로 (가상)
            SKPoint[] points_1v = new SKPoint[2];
            // 2번 광선 경로 (가상)
            SKPoint[] points_2v = new SKPoint[2];
            // 3번 광선 경로 (가상)
            SKPoint[] points_3v = new SKPoint[2];
            // 4번 광선 경로 (가상)
            SKPoint[] points_4v = new SKPoint[2];

            // 광학기기 종류별 처리
            switch (Optical)
            {
                case OpticalType.oga:
                    points_1[2] = originPoint + new SKPoint(-10 * F, 9 * ObjectSize);
                    points_2[1] = originPoint + new SKPoint(0, ObjectSize * F / (A - F));
                    points_2[2] = originPoint + new SKPoint(-10 * F, ObjectSize * F / (A - F));
                    points_3[0] = objPoint;
                    points_3[1] = originPoint + new SKPoint(0, 2 * ObjectSize * F / (A - 2 * F));
                    points_4[2] = originPoint + new SKPoint(-10 * F, 10 * ObjectSize * F / A);

                    if (A < F)
                    {
                        points_1v[0] = points_1[1];
                        points_1v[1] = originPoint + new SKPoint(10 * F, -11 * ObjectSize);
                        points_2v[0] = points_2[1];
                        points_2v[1] = originPoint + new SKPoint(10 * F, ObjectSize * F / (A - F));
                        points_3v[0] = points_3[1];
                        points_3v[1] = originPoint + new SKPoint(10 * F, 12 * ObjectSize * F / (A - 2 * F));
                        points_4v[0] = points_4[1];
                        points_4v[1] = originPoint + new SKPoint(10 * F, -10 * ObjectSize * F / A);
                    }
                    else if (A < 2 * F)
                    {
                        points_3v[0] = points_3[0];
                        points_3v[1] = originPoint + new SKPoint(-10 * F, -8 * ObjectSize * F / (A - 2 * F));
                    }

                    xPoints[0] = originPoint + new SKPoint(-F, 0);
                    xPoints[1] = originPoint + new SKPoint(-2 * F, 0);
                    break;
                case OpticalType.bolga:
                    points_1[2] = originPoint + new SKPoint(-F, -2 * ObjectSize);
                    points_2[1] = originPoint + new SKPoint(0, -ObjectSize * F / (A + F));
                    points_2[2] = originPoint + new SKPoint(-2 * F, -ObjectSize * F / (A + F));
                    points_3[0] = objPoint;
                    points_3[1] = originPoint + new SKPoint(0, -2 * ObjectSize * F / (A + 2 * F));
                    points_4[2] = originPoint + new SKPoint(-2 * F, 2 * ObjectSize * F / A);

                    points_1v[0] = points_1[1];
                    points_1v[1] = originPoint + new SKPoint(F, 0);
                    points_2v[0] = points_2[1];
                    points_2v[1] = originPoint + new SKPoint(10 * F, -ObjectSize * F / (A + F));
                    points_3v[0] = points_3[1];
                    points_3v[1] = originPoint + new SKPoint(2 * F, 0);
                    points_4v[0] = points_4[1];
                    points_4v[1] = originPoint + new SKPoint(10 * F, -10 * ObjectSize * F / A);

                    xPoints[0] = originPoint + new SKPoint(F, 0);
                    xPoints[1] = originPoint + new SKPoint(2 * F, 0);
                    break;
                case OpticalType.bollen:
                    points_1[2] = originPoint + new SKPoint(10 * F, 9 * ObjectSize);
                    points_2[1] = originPoint + new SKPoint(0, ObjectSize * F / (A - F));
                    points_2[2] = originPoint + new SKPoint(10 * F, ObjectSize * F / (A - F));
                    points_4[2] = originPoint + new SKPoint(10 * F, 10 * ObjectSize * F / A);

                    if (A < F)
                    {
                        points_1v[0] = points_1[1];
                        points_1v[1] = originPoint + new SKPoint(-10 * F, -11 * ObjectSize);
                        points_2v[0] = points_2[1];
                        points_2v[1] = originPoint + new SKPoint(-10 * F, ObjectSize * F / (A - F));
                        points_4v[0] = points_4[1];
                        points_4v[1] = originPoint + new SKPoint(-10 * F, -10 * ObjectSize * F / A);
                    }

                    xPoints[0] = originPoint + new SKPoint(F, 0);
                    xPoints[1] = originPoint + new SKPoint(-F, 0);
                    break;
                case OpticalType.olen:
                    points_1[2] = originPoint + new SKPoint(2 * F, -3 * ObjectSize);
                    points_2[1] = originPoint + new SKPoint(0, -ObjectSize * F / (A + F));
                    points_2[2] = originPoint + new SKPoint(2 * F, -ObjectSize * F / (A + F));
                    points_4[2] = originPoint + new SKPoint(2 * F, 2 * ObjectSize * F / A);

                    points_1v[0] = points_1[1];
                    points_1v[1] = originPoint + new SKPoint(-F, 0);
                    points_2v[0] = points_2[1];
                    points_2v[1] = originPoint + new SKPoint(-2 * F, -ObjectSize * F / (A + F));

                    xPoints[0] = originPoint + new SKPoint(-F, 0);
                    xPoints[1] = originPoint + new SKPoint(F, 0);
                    break;
            }

            // 광축 표시
            canvas.DrawLine(0, Height / 2, Width, Height / 2, linePaint);
            // 초점 표시
            canvas.DrawPoints(SKPointMode.Points, xPoints, pointPaint);

            // 1번 광선 표시
            canvas.DrawPoints(SKPointMode.Polygon, points_1, new SKPaint() { StrokeWidth = 2, Color = SKColors.Orange });
            // 2번 광선 표시
            canvas.DrawPoints(SKPointMode.Polygon, points_2, new SKPaint() { StrokeWidth = 2, Color = SKColors.Green });
            // 3번 광선 표시
            canvas.DrawPoints(SKPointMode.Lines, points_3, new SKPaint() { StrokeWidth = 2, Color = SKColors.Blue });
            // 4번 광선 표시
            canvas.DrawPoints(SKPointMode.Polygon, points_4, new SKPaint() { StrokeWidth = 2, Color = SKColors.DeepPink });

            // 1번 광선 표시 (가상)
            canvas.DrawPoints(SKPointMode.Lines, points_1v, new SKPaint() { StrokeWidth = 2, Color = SKColors.Orange, PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 5f }, 20f) });
            // 2번 광선 표시 (가상)
            canvas.DrawPoints(SKPointMode.Lines, points_2v, new SKPaint() { StrokeWidth = 2, Color = SKColors.Green, PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 5f }, 20f) });
            // 3번 광선 표시 (가상)
            canvas.DrawPoints(SKPointMode.Lines, points_3v, new SKPaint() { StrokeWidth = 2, Color = SKColors.Blue, PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 5f }, 20f) });
            // 4번 광선 표시 (가상)
            canvas.DrawPoints(SKPointMode.Lines, points_4v, new SKPaint() { StrokeWidth = 2, Color = SKColors.DeepPink, PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 5f }, 20f) });

            // 광학 기기 표시
            canvas.DrawBitmap(opticalBitmap, originPoint + new SKPoint(-opticalBitmap.Width / 2, -opticalBitmap.Height / 2));

            if (objectBitmap != null)
            {
                // 물체 표시
                canvas.DrawBitmap(objectBitmap, SKRect.Create(originPoint + new SKPoint(-A - ObjectSize / objectBitmap.Height * objectBitmap.Width / 2, -ObjectSize), new SKSize(ObjectSize / objectBitmap.Height * objectBitmap.Width, ObjectSize)));

                // 상 표시
                if (Optical == OpticalType.oga || Optical == OpticalType.bolga)
                {
                    if (M > 0)
                        canvas.DrawBitmap(reverseBitmap, SKRect.Create(originPoint + new SKPoint(-B - ImageSize / reverseBitmap.Height * reverseBitmap.Width / 2, 0), new SKSize(ImageSize / reverseBitmap.Height * reverseBitmap.Width, ImageSize)), imagePaint);
                    else if (M < 0)
                        canvas.DrawBitmap(horizontalBitmap, SKRect.Create(originPoint + new SKPoint(-B - ImageSize / horizontalBitmap.Height * horizontalBitmap.Width / 2, -ImageSize), new SKSize(ImageSize / horizontalBitmap.Height * horizontalBitmap.Width, ImageSize)), imagePaint);
                }
                else if (Optical == OpticalType.bollen || Optical == OpticalType.olen)
                {
                    if (M > 0)
                        canvas.DrawBitmap(verticalBitmap, SKRect.Create(originPoint + new SKPoint(B - ImageSize / verticalBitmap.Height * verticalBitmap.Width / 2, 0), new SKSize(ImageSize / verticalBitmap.Height * verticalBitmap.Width, ImageSize)), imagePaint);
                    else if (M < 0)
                        canvas.DrawBitmap(objectBitmap, SKRect.Create(originPoint + new SKPoint(B - ImageSize / objectBitmap.Height * objectBitmap.Width / 2, -ImageSize), new SKSize(ImageSize / objectBitmap.Height * objectBitmap.Width, ImageSize)), imagePaint);
                }
            }
        }
    }
}
