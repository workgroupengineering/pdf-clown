/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Objects;
using PdfClown.Util.Math;
using SkiaSharp;

namespace PdfClown.Documents.Contents.Objects
{
    /// <summary>'Append a straight line segment from the current point' operation [PDF:1.6:4.4.1].</summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class DrawLine : Operation
    {
        public static readonly string OperatorKeyword = "l";

        /// <param name="point">Final endpoint.</param>
        public DrawLine(SKPoint point) : this(point.X, point.Y)
        { }

        /// <param name="pointX">Final endpoint X.</param>
        /// <param name="pointY">Final endpoint Y.</param>
        public DrawLine(double pointX, double pointY)
            : base(OperatorKeyword, new PdfArrayImpl(2) { pointX, pointY })
        { }

        public DrawLine(PdfArray operands) : base(OperatorKeyword, operands)
        { }

        /// <summary>Gets/Sets the final endpoint.</summary>
        public SKPoint Point
        {
            get => operands.ToSKPoint();
            set
            {
                operands.Set(0, value.X);
                operands.Set(1, value.Y);
            }
        }

        public override void Scan(GraphicsState state) => state.Scanner.Path?.LineTo(Point);
    }
}