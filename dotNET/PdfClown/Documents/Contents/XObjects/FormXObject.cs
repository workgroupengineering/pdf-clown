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

using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Fonts;
using PdfClown.Documents.Contents.Objects;
using PdfClown.Documents.Contents.Scanner;
using PdfClown.Documents.Interchange.Metadata;
using PdfClown.Objects;
using PdfClown.Util.Math;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PdfClown.Documents.Contents.XObjects
{
    /// <summary>Form external object [PDF:1.6:4.9].</summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class FormXObject : XObject, IContentContext
    {
        public static new FormXObject Wrap(PdfDirectObject baseObject)
        {
            if (baseObject == null)
                return null;
            if (baseObject.Wrapper is FormXObject formObject)
                return formObject;

            var header = (PdfStream)baseObject.Resolve();
            var subtype = header.Get<PdfName>(PdfName.Subtype);

            //NOTE: Sometimes the form stream's header misses the mandatory Subtype entry; therefore, here
            //we force integrity for convenience (otherwise, content resource allocation may fail, for
            //example in case of Acroform flattening).
            if (subtype == null && header.ContainsKey(PdfName.BBox))
            {
                header[PdfName.Subtype] = PdfName.Form;
            }
            else if (!subtype.Equals(PdfName.Form))
            {
                return null;
            }

            return new FormXObject(baseObject);
        }

        private SKPicture picture;
        private SKMatrix? matrix;
        private SKRect? box;
        private ContentWrapper contents;

        ///<summary>Creates a new form within the specified document context.</summary>
        ///<param name="context">Document where to place this form.</param>
        ///<param name="size">Form size.</param>
        public FormXObject(PdfDocument context, SKSize size)
            : this(context, SKRect.Create(size))
        { }

        ///<summary>Creates a new form within the specified document context.</summary>
        ///<param name="context">Document where to place this form.</param>
        ///<param name="box">Form box.</param>
        public FormXObject(PdfDocument context, SKRect box)
            : base(context)
        {
            BaseDataObject[PdfName.Subtype] = PdfName.Form;
            Box = box;
        }

        public FormXObject(PdfDirectObject baseObject)
            : base(baseObject)
        { }

        public override SKMatrix Matrix
        {
            //NOTE: Form-space-to-user-space matrix is identity [1 0 0 1 0 0] by default,
            //but may be adjusted by setting the matrix entry in the form dictionary [PDF:1.6:4.9].
            get => matrix ??= BaseDataObject.Get<PdfArray>(PdfName.Matrix)?.ToSkMatrix() ?? SKMatrix.Identity;
            set
            {
                matrix = value;
                BaseDataObject[PdfName.Matrix] = value.ToPdfArray();
            }
        }

        public TransparencyXObject Group => Wrap<TransparencyXObject>(BaseDataObject[PdfName.Group]);

        public override SKSize Size
        {
            get
            {
                var box = BBox;
                return new SKSize((float)box.Width, (float)box.Height);
            }
            set
            {
                var boxObject = BaseDataObject.Get<PdfArray>(PdfName.BBox);
                boxObject.Set(2, Math.Abs(value.Width) + boxObject.GetFloat(0));
                boxObject.Set(3, Math.Abs(value.Height) + boxObject.GetFloat(1));
                box = null;
            }
        }

        public Rectangle BBox
        {
            get => Wrap<Rectangle>(BaseDataObject.GetOrCreate<PdfArray>(PdfName.BBox));
            set
            {
                BaseDataObject[PdfName.BBox] = value?.BaseObject;
                box = null;
            }
        }

        public SKRect Box
        {
            get => box ??= BBox.ToSKRect();
            set
            {
                var newValue = value.Round();
                if (Box != newValue)
                {
                    box = newValue;
                    BBox.Update(newValue);
                }
            }
        }

        public ContentWrapper Contents => contents ??= new ContentWrapper(BaseObject);

        public Resources Resources
        {
            get => Wrap<Resources>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.Resources));
            set => BaseDataObject[PdfName.Resources] = PdfObjectWrapper.GetBaseObject(value);
        }

        public RotationEnum Rotation => RotationEnum.Downward;

        public int Rotate => 0;

        public SKMatrix RotateMatrix => SKMatrix.Identity;

        public SKMatrix TextMatrix => SKMatrix.Identity;

        public List<ITextBlock> TextBlocks { get; } = new List<ITextBlock>();

        public AppDataCollection AppData
        {
            get => AppDataCollection.Wrap(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.PieceInfo), this);
        }

        public DateTime? ModificationDate => BaseDataObject.GetNDate(PdfName.LastModified);

        public SKMatrix InitialMatrix { get; internal set; } = SKMatrix.Identity;

        IList<ContentObject> ICompositeObject.Contents => Contents;

        public ICompositeObject Parent { get => null; set { } }

        public void ReloadContents()
        {
            if (contents != null)
            {
                Contents.Load();
            }
            InvalidatePicture();
        }

        public SKPicture Render(ContentScanner resourceScanner, SKColor? clearColor = null)
        {
            if (picture != null)
                return picture;

            var box = Box;
            using var recorder = new SKPictureRecorder();
            using var canvas = recorder.BeginRecording(box);
            Render(canvas, box, clearColor, resourceScanner);
            return picture = recorder.EndRecording();
        }

        public void Render(SKCanvas canvas, SKRect box, SKColor? clearColor = null)
            => Render(canvas, box, clearColor, null);

        public void Render(SKCanvas canvas, SKRect box, SKColor? clearColor = null, ContentScanner resourseScanner = null)
        {
            ReloadContents();
            var scanner = new ContentScanner(this, canvas, box, clearColor)
            {
                ResourceParent = resourseScanner
            };
            scanner.Scan();
        }



        public AppData GetAppData(PdfName appName) => AppData.Ensure(appName);

        public void Touch(PdfName appName) => Touch(appName, DateTime.Now);

        public void Touch(PdfName appName, DateTime modificationDate)
        {
            GetAppData(appName).ModificationDate = modificationDate;
            BaseDataObject.Set(PdfName.LastModified, modificationDate);
        }

        public ContentObject ToInlineObject(PrimitiveComposer composer)
        {
            throw new NotImplementedException();
        }

        public XObject ToXObject(PdfDocument context) => (XObject)Clone(context);

        internal void InvalidatePicture()
        {
            picture?.Dispose();
            picture = null;
        }

        public PdfName GetDefaultFont(out Font defaultFont, FontName fontName = FontName.Helvetica)
        {
            // Retrieving the font to define the default appearance...
            PdfName defaultFontName = null;
            defaultFont = null;
            defaultFontName = null;
            {
                // Field fonts.
                FontResources normalAppearanceFonts = Resources.Fonts;
                foreach (KeyValuePair<PdfName, Font> entry in normalAppearanceFonts)
                {
                    if (!entry.Value.Symbolic)
                    {
                        defaultFont = entry.Value;
                        defaultFontName = entry.Key;
                        break;
                    }
                }
                if (defaultFontName == null)
                {
                    // Common fonts.
                    FontResources formFonts = Document.Form.Resources.Fonts;
                    foreach (KeyValuePair<PdfName, Font> entry in formFonts)
                    {
                        if (!entry.Value.Symbolic && !entry.Value.IsStandard14)
                        {
                            defaultFont = entry.Value;
                            defaultFontName = entry.Key;
                            break;
                        }
                    }
                    if (defaultFontName == null)
                    {
                        //TODO:manage name collision!
                        formFonts[defaultFontName = PdfName.Get("defaultTTF")] = defaultFont = FontType0.Load(Document, fontName);
                    }
                    normalAppearanceFonts[defaultFontName] = defaultFont;
                }
            }

            return defaultFontName;
        }
    }
}