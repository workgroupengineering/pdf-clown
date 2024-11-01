﻿/*
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

using PdfClown.Documents.Functions;
using PdfClown.Objects;
using PdfClown.Util.Math;
using SkiaSharp;
using System;

namespace PdfClown.Documents.Contents.Patterns.Shadings
{
    public class AxialShading : Shading
    {
        private SKPoint[] coords;
        private float[] domain;
        private bool[] extend;
        private SKRect? box;

        internal AxialShading(PdfDirectObject baseObject) : base(baseObject)
        { }
        public AxialShading()
        {
            ShadingType = 2;
        }

        public override SKRect Box
        {
            get => box ??= Dictionary.Get<PdfArray>(PdfName.BBox)?.ToSKRect() ?? new SKRect(Coords[0].X, Coords[0].Y, Coords[1].X, Coords[1].Y).Standardized;
        }

        public SKPoint[] Coords
        {
            get => coords ??= Dictionary.Resolve(PdfName.Coords) is PdfArray array
                ? new SKPoint[]
                {
                    new SKPoint(array.GetFloat(0), array.GetFloat(1)),
                    new SKPoint(array.GetFloat(2), array.GetFloat(3))
                }
                : null;
            set
            {
                coords = value;
                Dictionary[PdfName.Domain] = new PdfArray(4)
                {
                    value[0].X, value[0].Y,
                    value[1].X, value[1].Y
                };
            }
        }

        public float[] Domain
        {
            get => domain ??= Dictionary.Resolve(PdfName.Domain) is PdfArray array
                    ? new float[] { array.GetFloat(0), array.GetFloat(1) }
                    : new float[] { 0F, 1F };
            set
            {
                domain = value;
                Dictionary[PdfName.Domain] = new PdfArray(2) { value[0], value[1] };
            }
        }

        public bool[] Extend
        {
            get => extend ??= Dictionary.Resolve(PdfName.Extend) is PdfArray array
                    ? new bool[] { array.GetBool(0), array.GetBool(1) }
                : new bool[] { false, false };
            set
            {
                extend = value;
                Dictionary[PdfName.Domain] = new PdfArray(2)
                {
                    value[0],
                    value[1]
                };
            }
        }

        public override SKShader GetShader(SKMatrix sKMatrix, GraphicsState state)
        {
            var coords = Coords;
            var colorSpace = ColorSpace;
            var compCount = colorSpace.ComponentCount;
            var colors = new SKColor[2];
            //var background = Background;
            var domain = Domain;
            Span<float> components = stackalloc float[compCount];
            for (int i = 0; i < domain.Length; i++)
            {
                components[0] = domain[i];
                var result = Function.Calculate(components);
                colors[i] = colorSpace.GetSKColor(result, null);
                components.Clear();
            }
            var mode = Extend[0] && Extend[1] ? SKShaderTileMode.Clamp
                : Extend[0] && !Extend[1] ? SKShaderTileMode.Mirror
                : !Extend[0] && Extend[1] ? SKShaderTileMode.Mirror
                : SKShaderTileMode.Decal;
            //var matrix = CalculateMatrix(sKMatrix, state);
            return SKShader.CreateLinearGradient(coords[0], coords[1], colors, domain, mode, sKMatrix);
        }

        public override SKMatrix CalculateMatrix(SKMatrix skMatrix, GraphicsState state)
        {
            var box = Box;
            box = skMatrix.MapRect(box);
            var pathRect = state.Scanner.Path?.Bounds ?? state.Scanner.Canvas.LocalClipBounds;
            pathRect = state.Ctm.MapRect(pathRect);

            var scale = box.Height != 0
                ? pathRect.Height / box.Height
                : pathRect.Width / box.Width;
            var scaleMAtrix = SKMatrix.CreateScale(scale, scale);
            var mBox = scaleMAtrix.MapRect(box);
            if (!mBox.IntersectsWith(pathRect))
            {
                //    scaleMAtrix = scaleMAtrix.PostConcat(SKMatrix.CreateTranslation(pathRect.Left - mBox.Left, pathRect.Top - mBox.Top));
            }
            return scaleMAtrix.PreConcat(skMatrix);
        }
    }
}
