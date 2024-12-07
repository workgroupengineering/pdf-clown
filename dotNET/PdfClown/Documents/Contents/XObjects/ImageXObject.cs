/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Documents.Contents.ColorSpaces;
using PdfClown.Objects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfClown.Documents.Contents.XObjects
{
    /// <summary>Image external object [PDF:1.6:4.8.4].</summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class ImageXObject : XObject, IImageObject
    {
        private ColorSpace colorSpace;
        private int? bitsPerComponent;
        private ImageXObject smask;
        private SKSize? size;
        private float[] decode;

        public ImageXObject(PdfDocument context, PdfStream baseDataObject) : base(context, baseDataObject)
        {
            //NOTE: It's caller responsability to adequately populate the stream
            //header and body in order to instantiate a valid object; header entries like
            //'Width', 'Height', 'ColorSpace', 'BitsPerComponent' MUST be defined
            //appropriately.
            baseDataObject[PdfName.Subtype] = PdfName.Image;
        }

        public ImageXObject(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Gets the number of bits per color component.</summary>
        public int BitsPerComponent => bitsPerComponent ??= BaseDataObject.GetInt(PdfName.BitsPerComponent, 8);

        /// <summary>Gets the color space in which samples are specified.</summary>
        public ColorSpace ColorSpace => colorSpace ??= ColorSpace.Wrap(BaseDataObject[PdfName.ColorSpace]);

        public IInputStream Data => Stream.GetInputStreamNoDecode();

        public PdfDirectObject Filter => Stream.Filter;

        public override SKMatrix Matrix
        {
            get
            {
                var size = Size;
                //NOTE: Image-space-to-user-space matrix is [1/w 0 0 1/h 0 0],
                //where w and h are the width and height of the image in samples [PDF:1.6:4.8.3].
                return new SKMatrix
                {
                    Values = new float[] { 1f / size.Width, 0, 0, 0, 1f / size.Height, 0, 0, 0, 1 }
                };
            }
            set
            {/* NOOP. */}
        }

        public PdfDirectObject Parameters => Stream.Parameters;

        public PdfDictionary Header => Stream;

        IDictionary<PdfName, PdfDirectObject> IImageObject.Header => Header;

        /// <summary>Gets the size of the image (in samples).</summary>
        public override SKSize Size
        {
            get => size ??= new SKSize(
                  BaseDataObject.GetInt(PdfName.Width),
                  BaseDataObject.GetInt(PdfName.Height));
            set => throw new NotSupportedException();
        }

        public SKImage Load(GraphicsState state)
        {
            if (Document.Cache.TryGetValue(Reference, out var existingBitmap))
            {
                return (SKImage)existingBitmap;
            }

            var image = BitmapLoader.Load(this, state);
            Document.Cache[Reference] = image;
            return image;
        }

        public IImageObject SMask
        {
            get => smask ??= Wrap<ImageXObject>(BaseDataObject[PdfName.SMask]);
            set => BaseDataObject[PdfName.SMask] = ((ImageXObject)value).BaseObject;
        }

        public PdfDirectObject Mask
        {
            get => BaseDataObject[PdfName.Mask];
            set => BaseDataObject[PdfName.Mask] = value;
        }

        public PdfStream Stream => BaseDataObject;

        public PdfArray Matte => BaseDataObject.Get<PdfArray>(PdfName.Matte);

        public float[] Decode
        {
            get => decode ??= BaseDataObject.Get<PdfArray>(PdfName.Decode)?.ToFloatArray();
            set => BaseDataObject[PdfName.Decode] = new PdfArray(value.Select(p => PdfInteger.Get((int)p)));
        }

        public bool ImageMask => BaseDataObject.GetBool(PdfName.ImageMask);

    }
}