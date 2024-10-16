/*
  Copyright 2007-2015 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents.Contents.ColorSpaces;
using PdfClown.Objects;
using System.Linq;

namespace PdfClown.Documents.Contents.Objects
{
    /// <summary>'Set the color to use for nonstroking operations' operation [PDF:1.6:4.5.7].</summary>
    [PDF(VersionEnum.PDF12)]
    public abstract class SetFillColor : SetColor
    {
        public SetFillColor(string @operator, PdfArray operands)
            : base(@operator, operands)
        { }

        protected SetFillColor(string @operator, Color value)
            : base(@operator, value.Components)
        { }

        /// <param name="operator">Graphics operator.</param>
        /// <param name="name">Name of the color resource entry (see <see cref="Patterns"/>).</param>
        protected SetFillColor(string @operator, PdfName name)
            : this(@operator, name, null)
        { }

        /// <param name="operator">Graphics operator.</param>
        /// <param name="name">Name of the color resource entry (see <see cref="Patterns"/>).</param>
        /// <param name="underlyingColor">Color used to colorize the pattern.</param>
        protected SetFillColor(string @operator, PdfName name, Color underlyingColor)
            : base(@operator, new PdfArray(underlyingColor?.Components ?? Enumerable.Empty<PdfDirectObject>()))
        {
            operands.AddDirect(name);
        }

        public override void Scan(GraphicsState state)
        {
            state.FillColor = state.FillColorSpace.GetColor(operands, state.Scanner.Context);
        }
    }
}