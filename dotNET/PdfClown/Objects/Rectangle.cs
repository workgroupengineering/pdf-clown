/*
  Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents;
using PdfClown.Files;

using System;
using SkiaSharp;
using System.Linq;

namespace PdfClown.Objects
{
    /// <summary>PDF rectangle object [PDF:1.6:3.8.4].</summary>
    /// <remarks>
    ///   <para>Rectangles are described by two diagonally-opposite corners. Corner pairs which don't
    ///   respect the canonical form (lower-left and upper-right) are automatically normalized to
    ///   provide a consistent representation.</para>
    ///   <para>Coordinates are expressed within the PDF coordinate space (lower-left origin and
    ///   positively-oriented axes).</para>
    /// </remarks>
    public sealed class Rectangle : PdfObjectWrapper<PdfArray>, IEquatable<Rectangle>
    {
        public static PdfArray Normalize(PdfArray array)
        {
            if (array.Count == 0)
            {
                array.AddRangeDirect(Enumerable.Repeat(PdfReal.Zero, 4));
            }
            else if (array.Count > 3)
            {
                if (array.GetDouble(0).CompareTo(array.GetDouble(2)) > 0)
                {
                    var leftCoordinate = array.GetNumber(2);
                    array[2] = (PdfDirectObject)array.GetNumber(0);
                    array[0] = (PdfDirectObject)leftCoordinate;
                }
                if (array.GetDouble(1).CompareTo(array.GetDouble(3)) > 0)
                {
                    var bottomCoordinate = array.GetNumber(3);
                    array[3] = (PdfDirectObject)array.GetNumber(1);
                    array[1] = (PdfDirectObject)bottomCoordinate;
                }
            }
            return array;
        }

        public Rectangle(SKRect rectangle)
            : this(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height)
        { }

        public Rectangle(SKPoint lowerLeft, SKPoint upperRight)
            : this(lowerLeft.X, upperRight.Y, upperRight.X - lowerLeft.X, upperRight.Y - lowerLeft.Y)
        { }

        public Rectangle(double left, double top, double width, double height)
            : this(new PdfArray(4)
              {
                  left, // Left (X).
                  top - height, // Bottom (Y).
                  left + width, // Right.
                  top // Top.
              })
        { }


        public Rectangle(PdfDirectObject baseObject)
            : base(Normalize((PdfArray)baseObject.Resolve()))
        { }

        public double Left
        {
            get => BaseDataObject.GetDouble(0);
            set => BaseDataObject.Set(0, value);
        }

        public double Bottom
        {
            get => BaseDataObject.GetDouble(1);
            set => BaseDataObject.Set(1, value);
        }

        public double Right
        {
            get => BaseDataObject.GetDouble(2);
            set => BaseDataObject.Set(2, value);
        }

        public double Top
        {
            get => BaseDataObject.GetDouble(3);
            set => BaseDataObject.Set(3, value);
        }

        public bool Equals(Rectangle other)
        {
            return Left.Equals(other.Left)
                && Bottom.Equals(other.Bottom)
                && Right.Equals(other.Right)
                && Top.Equals(other.Top);
        }

        public Rectangle Round() => Round(File?.Configuration?.RealPrecision ?? 5);

        public Rectangle Round(int precision)
        {
            Left = Math.Round(Left, precision);
            Bottom = Math.Round(Bottom, precision);
            Right = Math.Round(Right, precision);
            Top = Math.Round(Top, precision);
            return this;
        }

        public double Width
        {
            get => Right - Left;
            set => Right = Left + value;
        }

        public double Height
        {
            get => Top - Bottom;
            set => Bottom = Top - value;
        }

        public double X
        {
            get => Left;
            set => Left = value;
        }

        public double Y
        {
            get => Bottom;
            set => Bottom = value;
        }
    }
}