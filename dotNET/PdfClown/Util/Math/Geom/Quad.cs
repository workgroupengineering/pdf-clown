/*
  Copyright 2011-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PdfClown.Util.Math.Geom
{
    /// <summary>Quadrilateral shape.</summary>
    public struct Quad : IEquatable<Quad>
    {
        private static float Min(float lengthTopLeft, float lengthTopRight, float lengthBottomLeft, float lengthBottomRight)
        {
            var top = lengthTopLeft < lengthTopRight ? lengthTopLeft : lengthTopRight;
            var bottom = lengthBottomLeft < lengthBottomRight ? lengthBottomLeft : lengthBottomRight;
            return top < bottom ? top : bottom;
        }

        private static float Max(float lengthTopLeft, float lengthTopRight, float lengthBottomLeft, float lengthBottomRight)
        {
            var top = lengthTopLeft > lengthTopRight ? lengthTopLeft : lengthTopRight;
            var bottom = lengthBottomLeft > lengthBottomRight ? lengthBottomLeft : lengthBottomRight;
            return top > bottom ? top : bottom;
        }

        public static readonly Quad Empty = new Quad(SKPoint.Empty, SKPoint.Empty, SKPoint.Empty, SKPoint.Empty);

        public static bool operator ==(Quad value, Quad value2) => value.Equals(value2);

        public static bool operator !=(Quad value, Quad value2) => !value.Equals(value2);

        public static Quad Union(Quad value, Quad value2)
        {
            return value.Union(value2);
        }

        public static Quad Transform(Quad quad, ref SKMatrix matrix)
        {
            var temp = new Quad(quad);
            return temp.Transform(ref matrix);
        }

        private SKPoint point0;
        private SKPoint point1;
        private SKPoint point2;
        private SKPoint point3;

        public Quad(SKRect rectangle)
            : this(new SKPoint(rectangle.Left, rectangle.Top),
                  new SKPoint(rectangle.Right, rectangle.Top),
                  new SKPoint(rectangle.Right, rectangle.Bottom),
                  new SKPoint(rectangle.Left, rectangle.Bottom))
        { }

        public Quad(Quad quad)
            : this(quad.point0,
                  quad.point1,
                  quad.point2,
                  quad.point3)
        { }

        public Quad(SKPoint pointTopLeft, SKPoint pointTopRight, SKPoint pointBottomRight, SKPoint pointBottomLeft)
        {
            this.point0 = pointTopLeft;
            this.point1 = pointTopRight;
            this.point2 = pointBottomRight;
            this.point3 = pointBottomLeft;
        }

        public Quad(float left, float top, float right, float bottom)
        {
            this.point0 = new SKPoint(left, top);
            this.point1 = new SKPoint(right, top);
            this.point2 = new SKPoint(right, bottom);
            this.point3 = new SKPoint(left, bottom);
        }

        public bool IsEmpty => this.Equals(Empty);

        public SKPoint TopLeft => point0;

        public SKPoint TopRight => point1;

        public SKPoint BottomRight => point2;

        public SKPoint BottomLeft => point3;

        public SKPoint? Middle => SKLine.FindIntersection(new SKLine(point0, point2), new SKLine(point3, point1), false);

        public float Width => SKPoint.Distance(point0, point1);

        public float HorizontalLength => Right - Left;

        public float Height => SKPoint.Distance(point1, point2);

        public float VerticalLenght => Bottom - Top;

        public float Top => Min(point0.Y, point1.Y, point2.Y, point3.Y);

        public float Left => Min(point0.X, point1.X, point2.X, point3.X);

        public float Right => Max(point0.X, point1.X, point2.X, point3.X);

        public float Bottom => Max(point0.Y, point1.Y, point2.Y, point3.Y);

        public SKPoint[] GetPoints() => new SKPoint[4] { point0, point1, point2, point3 };

        public SKPath GetPath()
        {
            var points = new SKPoint[4] { point0, point1, point2, point3 };
            var path = new SKPath();//FillMode.Alternate
            path.AddPoly(points);
            return path;
        }

        public bool Contains(SKPoint p)
        {
            return (p - point0).Cross(point1 - point0) <= 0
                && (p - point1).Cross(point2 - point1) <= 0
                && (p - point2).Cross(point3 - point2) <= 0
                && (p - point3).Cross(point0 - point3) <= 0;
        }

        public bool Contains(SKPoint vectorTopLeft, SKPoint vectorTopRight, SKPoint vectorBottomRight, SKPoint vectorBottomLeft)
        {
            return vectorTopLeft.Cross(point1 - point0) <= 0
                && vectorTopRight.Cross(point2 - point1) <= 0
                && vectorBottomRight.Cross(point3 - point2) <= 0
                && vectorBottomLeft.Cross(point0 - point3) <= 0;
        }

        public bool Contains(float x, float y)
        {
            return Contains(new SKPoint(x, y));
        }

        public SKRect GetBounds()
        {
            var rect = new SKRect(point0.X, point0.Y, 0, 0);
            rect.Add(point1);
            rect.Add(point2);
            rect.Add(point3);
            return rect;
        }

        //public SKPathMeasure GetPathIterator()
        //{
        //    return new SKPathMeasure(Path);
        //}

        /// <summary>Expands the size of this quad stretching around its center.</summary>
        /// <param name="value">Expansion extent.</param>
        /// <returns>This quad.</returns>
        public Quad Inflate(float value)
        {
            return Inflate(value, value);
        }

        /// <summary>Expands the size of this quad stretching around its center.</summary>
        /// <param name="valueX">Expansion's horizontal extent.</param>
        /// <param name="valueY">Expansion's vertical extent.</param>
        /// <returns>This quad.</returns>
        public Quad Inflate(float valueX, float valueY)
        {
            SKRect oldBounds = GetBounds();
            var matrix = SKMatrix.CreateTranslation(oldBounds.MidX, oldBounds.MidY);
            matrix = matrix.PreConcat(SKMatrix.CreateScale(1 + valueX * 2 / oldBounds.Width, 1 + valueY * 2 / oldBounds.Height));
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(-(oldBounds.MidX), -(oldBounds.MidY)));
            return Transform(ref matrix);
        }

        public Quad Transform(ref SKMatrix matrix)
        {
            var points = new SKPoint[4] { point0, point1, point2, point3 };
            matrix.MapPoints(points, points);
            point0 = points[0];
            point1 = points[1];
            point2 = points[2];
            point3 = points[3];
            return this;
        }

        public bool IntersectsWith(Quad value)
        {
            return SKLine.FindIntersection(new SKLine(point0, point1), value, true) != null
                || SKLine.FindIntersection(new SKLine(point1, point2), value, true) != null
                || SKLine.FindIntersection(new SKLine(point2, point3), value, true) != null
                || SKLine.FindIntersection(new SKLine(point3, point0), value, true) != null;
        }

        public bool ContainsOrIntersect(Quad value)
        {
            return Contains(value)
                || value.Contains(this)
                || IntersectsWith(value);
        }

        public bool Contains(Quad value)
        {
            return Contains(value.point0)
                && Contains(value.point1)
                && Contains(value.point2)
                && Contains(value.point3);
        }

        public Quad Union(Quad value)
        {
            var points = new SKPoint[4] { value.point0, value.point1, value.point2, value.point3 };
            Add(points);
            return this;
        }

        public void Add(SKPoint[] points)
        {
            KeyValuePair<float, SKPoint>? maxTopLeft = null;
            KeyValuePair<float, SKPoint>? maxTopRight = null;
            KeyValuePair<float, SKPoint>? maxBottomRight = null;
            KeyValuePair<float, SKPoint>? maxBottomLeft = null;

            var topLeftToTopRight = point1 - point0;
            var topRightToBottomRight = point2 - point1;
            var bottomRightToBottomLeft = point3 - point2;
            var bottomLeftToTopLeft = point0 - point3;
            for (int i = 0; i < points.Length; i++)
            {
                var point = points[i];
                var vectorTopLeft = point - point0;
                var vectorTopRight = point - point1;
                var vectorBottomRight = point - point2;
                var vectorBottomLeft = point - point3;
                if (vectorTopLeft.Cross(topLeftToTopRight) <= 0
                    && vectorTopRight.Cross(topRightToBottomRight) <= 0
                    && vectorBottomRight.Cross(bottomRightToBottomLeft) <= 0
                    && vectorBottomLeft.Cross(bottomLeftToTopLeft) <= 0)
                {
                    continue;
                }
                var lengthTopLeft = vectorTopLeft.Length;
                var lengthTopRight = vectorTopRight.Length;
                var lengthBottomRight = vectorBottomRight.Length;
                var lengthBottomLeft = vectorBottomLeft.Length;
                float min = Min(lengthTopLeft, lengthTopRight, lengthBottomRight, lengthBottomLeft);
                if (min == lengthTopLeft)
                {
                    if (maxTopLeft == null)
                        maxTopLeft = new KeyValuePair<float, SKPoint>(min, point);
                    else if (min > maxTopLeft.Value.Key)
                        maxTopLeft = new KeyValuePair<float, SKPoint>(min, point);
                }
                else if (min == lengthTopRight)
                {
                    if (maxTopRight == null)
                        maxTopRight = new KeyValuePair<float, SKPoint>(min, point);
                    else if (min > maxTopRight.Value.Key)
                        maxTopRight = new KeyValuePair<float, SKPoint>(min, point);
                }
                else if (min == lengthBottomRight)
                {
                    if (maxBottomRight == null)
                        maxBottomRight = new KeyValuePair<float, SKPoint>(min, point);
                    else if (min > maxBottomRight.Value.Key)
                        maxBottomRight = new KeyValuePair<float, SKPoint>(min, point);
                }
                else if (min == lengthBottomLeft)
                {
                    if (maxBottomLeft == null)
                        maxBottomLeft = new KeyValuePair<float, SKPoint>(min, point);
                    else if (min > maxBottomLeft.Value.Key)
                        maxBottomLeft = new KeyValuePair<float, SKPoint>(min, point);
                }
                
            }
            if (maxTopLeft != null)
            {
                point0 = maxTopLeft.Value.Value;
            }
            if (maxTopRight != null)
            {
                point1 = maxTopRight.Value.Value;
            }
            if (maxBottomRight != null)
            {
                point2 = maxBottomRight.Value.Value;
            }
            if (maxBottomLeft != null)
            {
                point3 = maxBottomLeft.Value.Value;
            }
            
        }

        public void Add(SKPoint point)
        {
            var vectorTopLeft = point - point0;
            var vectorTopRight = point - point1;
            var vectorBottomRight = point - point2;
            var vectorBottomLeft = point - point3;
            if (Contains(vectorTopLeft, vectorTopRight, vectorBottomRight, vectorBottomLeft))
                return;
            var lengthTopLeft = vectorTopLeft.Length;
            var lengthTopRight = vectorTopRight.Length;
            var lengthBottomRight = vectorBottomRight.Length;
            var lengthBottomLeft = vectorBottomLeft.Length;
            var min = Min(lengthTopLeft, lengthTopRight, lengthBottomRight, lengthBottomLeft);
            if (min == lengthTopLeft)
            {
                point0 = point;
            }
            else if (min == lengthTopRight)
            {
                point1 = point;
            }
            else if (min == lengthBottomRight)
            {
                point2 = point;
            }
            else if (min == lengthBottomLeft)
            {
                point3 = point;
            }
            
            //if (point.X < pointTopLeft.X || point.X < pointBottomLeft.X)
            //{
            //    var newLine = new SKLine(point, pointTopLeft - pointBottomLeft);
            //    var newTopLeft = SKLine.FindIntersection(newLine, new SKLine(pointTopLeft, pointTopRight), false);
            //    var newBottomLeft = SKLine.FindIntersection(newLine, new SKLine(pointBottomLeft, pointBottomRight), false);
            //}
        }

        public override bool Equals(object other) => other is Quad quad ? Equals(quad) : false;

        public bool Equals(Quad other)
        {
            return point0.Equals(other.point0)
                && point1.Equals(other.point1)
                && point2.Equals(other.point2)
                && point3.Equals(other.point3);
        }

        public override string ToString()
        {
            return GetBounds().ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(point0, point1, point2, point3);
        }
    }
}