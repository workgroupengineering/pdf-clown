/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Bytes;
using SkiaSharp;

namespace PdfClown.Documents.Contents.Objects
{
    /**
      <summary>Clipping path operation [PDF:1.6:4.4.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class ModifyClipPath : Operation
    {
        public const string EvenOddOperatorKeyword = "W*";
        public const string NonZeroOperatorKeyword = "W";

        /**
          <summary>'Modify the current clipping path by intersecting it with the current path,
          using the even-odd rule to determine which regions lie inside the clipping path'
          operation.</summary>
        */
        public static readonly ModifyClipPath EvenOdd = new ModifyClipPath(EvenOddOperatorKeyword, WindModeEnum.EvenOdd);
        /**
          <summary>'Modify the current clipping path by intersecting it with the current path,
          using the nonzero winding number rule to determine which regions lie inside
          the clipping path' operation.</summary>
        */
        public static readonly ModifyClipPath NonZero = new ModifyClipPath(NonZeroOperatorKeyword, WindModeEnum.NonZero);

        private WindModeEnum clipMode;

        private ModifyClipPath(string @operator, WindModeEnum clipMode) : base(@operator)
        {
            this.clipMode = clipMode;
        }

        /**
          <summary>Gets the clipping rule.</summary>
        */
        public WindModeEnum ClipMode => clipMode;

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            var pathObject = scanner.RenderObject;
            if (pathObject != null)
            {
                pathObject.FillType = clipMode.ToSkia();
                scanner.RenderContext.ClipPath(pathObject, SKClipOperation.Intersect, true);
            }
        }
    }
}