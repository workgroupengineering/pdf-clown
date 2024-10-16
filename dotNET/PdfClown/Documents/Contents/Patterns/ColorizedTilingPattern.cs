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

using PdfClown.Objects;

using System;
using SkiaSharp;
using System.Collections.Generic;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Objects;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Documents.Interchange.Metadata;
using PdfClown.Documents.Contents.ColorSpaces;

namespace PdfClown.Documents.Contents.Patterns
{
    /// <summary>Uncolored tiling pattern ("stencil") associated to a color.</summary>
    public sealed class ColorizedTilingPattern : Color, IPattern
    {
        private readonly TilingPattern tiling;
        private readonly Color color;

        internal ColorizedTilingPattern(TilingPattern tiling, Color color)
            : base(null)
        {
            this.tiling = tiling;
            this.color = color;
        }

        /// <summary>Gets the color applied to the stencil.</summary>
        public Color Color => color;

        public override ColorSpace ColorSpace => tiling.ColorSpace;

        public override PdfArray Components => tiling.Components;

        public override PdfDirectObject BaseObject { get => tiling.BaseObject; protected set => base.BaseObject = value; }

        public SKMatrix Matrix
        {
            get => tiling.Matrix;
            set => tiling.Matrix = value;
        }        

        public SKShader GetShader(GraphicsState state)
        {
            var shader = tiling.GetShader(state);
            return SKShader.CreateCompose(SKShader.CreateColor(color.GetSkColor()), shader, SKBlendMode.DstIn);
        }
        
    }
}