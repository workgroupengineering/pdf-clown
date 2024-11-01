/*
  Copyright 2006-2012 Stefano Chizzolini. http://www.pdfclown.org

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
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PdfClown.Documents.Contents.ColorSpaces
{
    /// <summary>Color space [PDF:1.6:4.5].</summary>
    public abstract class ColorSpace : PdfObjectWrapper<PdfDirectObject>
    {
        private static readonly Dictionary<PdfName, Func<PdfDirectObject, ColorSpace>> wrappers = new()
        {
            { PdfName.DeviceRGB, (pdfObject) => DeviceRGBColorSpace.Default },
            { PdfName.RGB, (pdfObject) => DeviceRGBColorSpace.DefaultShort },
            { PdfName.DeviceCMYK, (pdfObject) => DeviceCMYKColorSpace.Default },
            { PdfName.CMYK, (pdfObject) => DeviceCMYKColorSpace.DefaultShort },
            { PdfName.DeviceGray, (pdfObject) => DeviceGrayColorSpace.Default },
            { PdfName.G, (pdfObject) => DeviceGrayColorSpace.DefaultShort },
            { PdfName.CalRGB, (pdfObject) => new CalRGBColorSpace(pdfObject) },
            { PdfName.CalGray, (pdfObject) => new CalGrayColorSpace(pdfObject) },
            { PdfName.ICCBased, (pdfObject) => new ICCBasedColorSpace(pdfObject) },
            { PdfName.Lab, (pdfObject) => new LabColorSpace(pdfObject) },
            { PdfName.DeviceN, (pdfObject) => new DeviceNColorSpace(pdfObject) },
            { PdfName.Indexed, (pdfObject) => new IndexedColorSpace(pdfObject) },
            { PdfName.I, (pdfObject) => new IndexedColorSpace(pdfObject) },
            { PdfName.Pattern, (pdfObject) => new PatternColorSpace(pdfObject) },
            { PdfName.Separation, (pdfObject) => new SeparationColorSpace(pdfObject) },
        };

        /// <summary>Wraps the specified color space base object into a color space object.</summary>
        /// <param name="baseObject">Base object of a color space object.</param>
        /// <returns>Color space object corresponding to the base object.</returns>
        public static ColorSpace Wrap(PdfDirectObject baseObject)
        {
            if (baseObject == null)
                return null;
            if (baseObject.Wrapper is ColorSpace colorSpace)
                return colorSpace;
            // Get the data object corresponding to the color space!
            PdfDataObject baseDataObject = baseObject.Resolve();
            // NOTE: A color space is defined by an array object whose first element
            // is a name object identifying the color space family [PDF:1.6:4.5.2].
            // For families that do not require parameters, the color space CAN be
            // specified simply by the family name itself instead of an array.
            var name = baseDataObject is PdfArray array
              ? array.Get<PdfName>(0)
              : (PdfName)baseDataObject;
            if (wrappers.TryGetValue(name, out var func))
                return func(baseObject);
            return null;
            //throw new NotSupportedException("Color space " + name + " unknown.");
        }

        protected ColorSpace(PdfDocument context, PdfDirectObject baseDataObject)
            : base(context, baseDataObject)
        { }

        public ColorSpace(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Gets the number of components used to represent a color value.</summary>
        public abstract int ComponentCount
        {
            get;
        }

        /// <summary>Gets the initial color value within this color space.</summary>
        public abstract Color DefaultColor
        {
            get;
        }

        /// <summary>Gets the rendering representation of the specified color value.</summary>
        /// <param name="color">Color value to convert into an equivalent rendering representation.</param>
        public virtual SKPaint GetPaint(Color color, SKPaintStyle paintStyle, float? alpha = null, GraphicsState graphicsState = null)
        {            
            var skColor = GetSKColor(color, alpha);
            return new SKPaint
            {
                Color = skColor,
                Style = paintStyle,
                IsAntialias = true,
                BlendMode = SKBlendMode.SrcOver
            };
        }

        /// <summary>Gets the color value corresponding to the specified components
        /// interpreted according to this color space [PDF:1.6:4.5.1].</summary>
        /// <param name="components">Color components.</param>
        /// <param name="context">Content context.</param>
        public abstract Color GetColor(PdfArray components, IContentContext context);

        public SKColor GetSKColor(PdfArray components, IContentContext context, float? alpha = null) => GetSKColor(GetColor(components, context), alpha);

        public abstract SKColor GetSKColor(Color color, float? alpha = null);

        public abstract SKColor GetSKColor(ReadOnlySpan<float> components, float? alpha = null);

        public abstract bool IsSpaceColor(Color value);

        protected byte ToByte(double v)
        {
            return v > 255 ? (byte)255 : v < 0 ? (byte)0 : (byte)v;
        }
    }
}