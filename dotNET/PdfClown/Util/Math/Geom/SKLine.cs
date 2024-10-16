using SkiaSharp;

namespace PdfClown.Util.Math.Geom
{
    //https://gamedev.stackexchange.com/a/111106
    public struct SKLine
    {
        public SKPoint a;
        public SKPoint b;

        public SKLine(SKPoint a, SKPoint b)
        {
            this.a = a;
            this.b = b;
        }

        public SKPoint Vector => a - b;

        public SKPoint NormalVector => SKPoint.Normalize(Vector);

        public static SKPoint? FindIntersection(SKLine a, SKLine b, bool segment)
            => FindIntersection(a.a, a.b, b.a, b.b, segment);
        public static SKPoint? FindIntersection(SKPoint aa, SKPoint ab, SKPoint ba, SKPoint bb, bool segment)
        {
            float x1 = aa.X;
            float y1 = aa.Y;
            float x2 = ab.X;
            float y2 = ab.Y;

            float x3 = ba.X;
            float y3 = ba.Y;
            float x4 = bb.X;
            float y4 = bb.Y;

            float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0)
                return null;
            if (segment)
            {
                float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
                if (t < 0 || t > 1)
                    return null;
                float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denominator;
                if (u < 0 || u > 1)
                    return null;
            }
            float xNominator = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
            float yNominator = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);

            float px = xNominator / denominator;
            float py = yNominator / denominator;

            return new SKPoint(px, py);

        }

        public static SKPoint? FindIntersection(SKLine a, Quad q, bool segment)
        {
            return FindIntersection(a, q.Point0, q.Point1, q.Point2, q.Point3, segment);
        }

        public static SKPoint? FindIntersection(SKLine a, SKPoint c0, SKPoint c1, SKPoint c2, SKPoint c3, bool segment)
        {
            return FindIntersection(a, new SKLine(c0, c1), segment) ??
                FindIntersection(a, new SKLine(c1, c2), segment) ??
                FindIntersection(a, new SKLine(c2, c3), segment) ??
                FindIntersection(a, new SKLine(c3, c0), segment);
        }

    }
}
