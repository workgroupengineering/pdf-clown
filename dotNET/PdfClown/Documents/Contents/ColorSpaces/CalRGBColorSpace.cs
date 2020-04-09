/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Objects;

using System;
using System.Collections.Generic;
using SkiaSharp;
using PdfClown.Documents.Functions;

namespace PdfClown.Documents.Contents.ColorSpaces
{
    /**
      <summary>CIE-based ABC single-transformation-stage color space, where A, B, and C represent
      calibrated red, green and blue color values [PDF:1.6:4.5.4].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class CalRGBColorSpace : CalColorSpace
    {
        #region dynamic
        #region constructors
        //TODO:IMPL new element constructor!

        internal CalRGBColorSpace(PdfDirectObject baseObject) : base(baseObject)
        { }
        #endregion

        #region interface
        #region public
        public override object Clone(Document context)
        { throw new NotImplementedException(); }

        public override int ComponentCount => 3;

        public override Color DefaultColor => CalRGBColor.Default;

        public override double[] Gamma
        {
            get
            {
                PdfArray gamma = (PdfArray)Dictionary[PdfName.Gamma];
                return (gamma == null
                  ? new double[] { 1, 1, 1 }
                  : new double[] { ((IPdfNumber)gamma[0]).RawValue, ((IPdfNumber)gamma[1]).RawValue, ((IPdfNumber)gamma[2]).RawValue }
                  );
            }
        }

        public SKMatrix Matrix
        {
            get
            {
                PdfArray matrix = (PdfArray)Dictionary.Resolve(PdfName.Matrix);
                if (matrix == null)
                    return SKMatrix.MakeIdentity();
                else
                    return new SKMatrix
                    {
                        ScaleX = ((IPdfNumber)matrix[0]).FloatValue,
                        SkewY = ((IPdfNumber)matrix[1]).FloatValue,
                        SkewX = ((IPdfNumber)matrix[2]).FloatValue,
                        ScaleY = ((IPdfNumber)matrix[3]).FloatValue,
                        TransX = ((IPdfNumber)matrix[4]).FloatValue,
                        TransY = ((IPdfNumber)matrix[5]).FloatValue,
                        Persp2 = 1
                    };
            }
            set => Dictionary[PdfName.Matrix] =
                 new PdfArray(
                    PdfReal.Get(value.ScaleX),
                    PdfReal.Get(value.SkewY),
                    PdfReal.Get(value.SkewX),
                    PdfReal.Get(value.ScaleY),
                    PdfReal.Get(value.TransX),
                    PdfReal.Get(value.TransY)
                    );
        }

        public override Color GetColor(IList<PdfDirectObject> components, IContentContext context)
        { return new CalRGBColor(components); }

        public override bool IsSpaceColor(Color color)
        { return color is CalRGBColor; }

        public override SKColor GetSKColor(Color color, double? alpha = null)
        {
            // FIXME: temporary hack
            return SKColors.Black;
        }

        public override SKColor GetSKColor(double[] components, double? alpha = null)
        {
            // FIXME: temporary hack
            return SKColors.Black;
        }


        #endregion
        #endregion
        #endregion
    }
}