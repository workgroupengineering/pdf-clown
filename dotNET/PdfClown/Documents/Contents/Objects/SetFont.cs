/*
  Copyright 2007-2012 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents.Contents.Fonts;
using PdfClown.Objects;

namespace PdfClown.Documents.Contents.Objects
{
    /// <summary>'Set the text font' operation [PDF:1.6:5.2].</summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class SetFont : Operation, IResourceReference<Font>
    {
        public static readonly string OperatorKeyword = "Tf";

        public SetFont(PdfName name, double size)
            : base(OperatorKeyword, new PdfArray(2) { name, size })
        { }

        public SetFont(PdfArray operands)
            : base(OperatorKeyword, operands)
        { }

        /// <summary>Gets/Sets the font size to be set.</summary>
        public float Size
        {
            get => operands.GetFloat(1);
            set => operands.Set(1, value);
        }

        public PdfName Name
        {
            get => operands.Get<PdfName>(0);
            set => operands[0] = value;
        }

        /// <summary>Gets the <see cref="Font">font</see> resource to be set.</summary>
        /// <param name="scanner">Content context.</param>
        public Font GetResource(ContentScanner scanner)
        {
            var pscanner = scanner;
            Font font;
            while ((font = pscanner.Context.Resources.Fonts[Name]) == null
                && (pscanner = pscanner.ResourceParent) != null)
            { }
            return font;
        }

        public override void Scan(GraphicsState state)
        {
            state.Font = GetResource(state.Scanner);
            state.FontSize = Size;
        }
    }
}