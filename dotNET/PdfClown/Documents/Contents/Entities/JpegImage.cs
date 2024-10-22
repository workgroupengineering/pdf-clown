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

using PdfClown.Bytes;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Objects;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Objects;
using PdfClown.Util.IO;
using System.IO;

namespace PdfClown.Documents.Contents.Entities
{
    /// <summary>JPEG image object [ISO 10918-1;JFIF:1.02].</summary>
    public sealed class JpegImage : Image
    {
        public JpegImage(Stream stream) : base(stream)
        { Load(); }

        public override ContentObject ToInlineObject(PrimitiveComposer composer)
        {
            return composer.Add(
              new GraphicsInlineImage(
                new InlineImageHeader(
                  new PdfArray
                  {
                      PdfName.W, Width,
                      PdfName.H, Height,
                      PdfName.CS, PdfName.RGB,
                      PdfName.BPC, BitsPerComponent,
                      PdfName.F, PdfName.DCT
                  }),
                new InlineImageBody(new ByteStream(Stream))));
        }

        public override XObject ToXObject(PdfDocument context)
        {
            return new ImageXObject(
              context,
              new PdfStream(
                new PdfDictionary(5)
                {
                  { PdfName.Width, Width },
                  { PdfName.Height, Height },
                  { PdfName.BitsPerComponent, BitsPerComponent },
                  { PdfName.ColorSpace, PdfName.DeviceRGB },
                  { PdfName.Filter, PdfName.DCTDecode }
               },
                new ByteStream(Stream)));
        }

        private void Load()
        {
            // NOTE: Big-endian data expected.
            Stream stream = Stream;
            BigEndianBinaryReader streamReader = new BigEndianBinaryReader(stream);

            int index = 4;
            stream.Seek(index, SeekOrigin.Begin);
            byte[] markerBytes = new byte[2];
            while (true)
            {
                index += streamReader.ReadUInt16();
                stream.Seek(index, SeekOrigin.Begin);

                stream.Read(markerBytes, 0, 2);
                index += 2;

                // Frame header?
                if (markerBytes[0] == 0xFF
                  && markerBytes[1] == 0xC0)
                {
                    stream.Seek(2, SeekOrigin.Current);
                    // Get the image bits per color component (sample precision)!
                    BitsPerComponent = stream.ReadByte();
                    // Get the image size!
                    Height = streamReader.ReadUInt16();
                    Width = streamReader.ReadUInt16();

                    break;
                }
            }
        }
    }
}