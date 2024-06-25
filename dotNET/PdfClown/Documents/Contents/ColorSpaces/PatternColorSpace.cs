/*
  Copyright 2010-2011 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents.Contents.Patterns;
using PdfClown.Documents.Contents.Patterns.Shadings;
using PdfClown.Objects;
using SkiaSharp;
using System;

namespace PdfClown.Documents.Contents.ColorSpaces
{
    /**
      <summary>Pattern color space [PDF:1.6:4.5.5].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class PatternColorSpace : SpecialColorSpace
    {
        /*
          NOTE: In case of no parameters, it may be specified directly (i.e. without being defined
          in the ColorSpace subdictionary of the contextual resource dictionary) [PDF:1.6:4.5.7].
        */
        //TODO:verify parameters!!!
        public static readonly PatternColorSpace Default = new PatternColorSpace(null);
        private ColorSpace underlineColorSpace;

        //TODO:IMPL new element constructor!

        internal PatternColorSpace(PdfDirectObject baseObject) : base(baseObject)
        { }

        public override int ComponentCount => 0;

        public override Color DefaultColor => Pattern.Default;

        /**
          <summary>Gets the color space in which the actual color of the <see cref="Patterns">pattern</see> is to be specified.</summary>
          <remarks>This feature is applicable to <see cref="TilingPattern">uncolored tiling patterns</see> only.</remarks>
        */
        public ColorSpace UnderlyingColorSpace
        {
            get => underlineColorSpace ??= BaseDataObject is PdfArray baseArrayObject && baseArrayObject.Count > 1
                ? ColorSpace.Wrap(baseArrayObject[1])
                : null;
        }

        public override Color GetColor(PdfArray components, IContentContext context)
        {
            Color pattern = context.Resources.Patterns[(PdfName)components[components.Count - 1]];
            if (pattern is TilingPattern tilingPattern)
            {
                if (tilingPattern.PaintType == TilingPaintTypeEnum.Uncolored)
                {
                    ColorSpace underlyingColorSpace = UnderlyingColorSpace;
                    if (underlyingColorSpace == null)
                        throw new ArgumentException("Uncolored tiling patterns not supported by this color space because no underlying color space has been defined.");

                    //TODO cache colorized
                    // Get the color to be used for colorizing the uncolored tiling pattern!
                    Color color = underlyingColorSpace.GetColor(components, context);
                    // Colorize the uncolored tiling pattern!
                    pattern = tilingPattern.Colorize(color);
                }
            }
            return pattern;
        }

        public override bool IsSpaceColor(Color color) => color is Pattern;

        public override SKColor GetSKColor(Color color, float? alpha = null)
        {
            // FIXME: Auto-generated method stub
            return SKColors.Black;
        }

        public override SKColor GetSKColor(ReadOnlySpan<float> components, float? alpha = null)
        {
            // FIXME: Auto-generated method stub
            return SKColors.Black;
        }

        public override SKPaint GetPaint(Color color, SKPaintStyle paintStyle, float? alpha = null, GraphicsState state = null)
        {
            if (color is IPattern pattern)
            {
                var paint = new SKPaint
                {
                    Shader = pattern.GetShader(state),
                    Style = paintStyle,
                    IsAntialias = true
                };

                if (pattern is Shading shading
                    && shading.BackgroundColor is Color backColor)
                {
                    paint.Color = backColor.GetSkColor(alpha);
                }
                return paint;
            }
            return new SKPaint
            {
                Color = GetSKColor(color),
                Style = paintStyle,
                IsAntialias = true,
            };
        }

        public override object Clone(PdfDocument context)
        { throw new NotImplementedException(); }
    }
}