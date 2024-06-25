/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Andreas Pinter (bug reporter [FIX:53], https://sourceforge.net/u/drunal/)

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
using PdfClown.Documents.Contents;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Objects;
using PdfClown.Documents.Interaction.Navigation;
using PdfClown.Documents.Interchange.Metadata;
using PdfClown.Objects;

using System;
using System.Collections.Generic;
using SkiaSharp;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Util.Math.Geom;

namespace PdfClown.Documents
{
    /// <summary>Document page [PDF:1.6:3.6.2].</summary>
    [PDF(VersionEnum.PDF10)]
    public class PdfPage : PdfObjectWrapper<PdfDictionary>, IContentContext
    {
        //NOTE: Inheritable attributes are NOT early-collected, as they are NOT part
        //of the explicit representation of a page. They are retrieved every time
        //clients call.

        /// <summary>Annotations tab order [PDF:1.6:3.6.2].</summary>
        [PDF(VersionEnum.PDF15)]
        public enum TabOrderEnum
        {
            /// <summary>Row order.</summary>
            Row,
            /// <summary>Column order.</summary>
            Column,
            /// <summary>Structure order.</summary>
            Structure
        };

        public static readonly ISet<PdfName> InheritableAttributeKeys;

        private static readonly Dictionary<TabOrderEnum, PdfName> TabOrderEnumCodes;
        private SKMatrix? rotateMatrix;
        private SKMatrix? invertRotateMatrix;
        private SKRect? box;
        internal int? index;
        private Stack<GraphicsState> states;

        static PdfPage()
        {
            InheritableAttributeKeys = new HashSet<PdfName>
            {
                PdfName.Resources,
                PdfName.MediaBox,
                PdfName.CropBox,
                PdfName.Rotate
            };

            TabOrderEnumCodes = new Dictionary<TabOrderEnum, PdfName>
            {
                [TabOrderEnum.Row] = PdfName.R,
                [TabOrderEnum.Column] = PdfName.C,
                [TabOrderEnum.Structure] = PdfName.S
            };
        }

        /// <summary>Gets the attribute value corresponding to the specified key, possibly recurring to
        /// its ancestor nodes in the page tree.</summary>
        /// <param name="pageObject">Page object.</param>
        /// <param name="key">Attribute key.</param>
        public static PdfDirectObject GetInheritableAttribute(PdfDictionary pageObject, PdfName key)
        {
            // NOTE: It moves upward until it finds the inherited attribute.
            PdfDictionary dictionary = pageObject;
            while (true)
            {
                if (dictionary.TryGetValue(key, out var entry)
                    && entry != null)
                    return entry;

                dictionary = dictionary.Get<PdfDictionary>(PdfName.Parent);
                if (dictionary == null)
                {
                    // Isn't the page attached to the page tree?
                    /* NOTE: This condition is illegal. */
                    //if (pageObject[PdfName.Parent] == null)
                    //    throw new Exception("Inheritable attributes unreachable: Page objects MUST be inserted into their document's Pages collection before being used.");

                    return null;
                }
            }
        }

        /// <summary>Gets the code corresponding to the given value.</summary>
        private static PdfName ToCode(TabOrderEnum value)
        {
            return TabOrderEnumCodes[value];
        }

        /// <summary>Gets the tab order corresponding to the given value.</summary>
        private static TabOrderEnum ToTabOrderEnum(string value)
        {
            if (value == null)
                return TabOrderEnum.Row;
            foreach (KeyValuePair<TabOrderEnum, PdfName> tabOrder in TabOrderEnumCodes)
            {
                if (string.Equals(tabOrder.Value.StringValue, value, StringComparison.Ordinal))
                    return tabOrder.Key;
            }
            return TabOrderEnum.Row;
        }

        /// <summary>Creates a new page within the specified document context, using the default size.
        /// </summary>
        /// <param name="context">Document where to place this page.</param>
        public PdfPage(PdfDocument context) : this(context, null)
        { }

        /// <summary>Creates a new page within the specified document context.</summary>
        /// <param name="context">Document where to place this page.</param>
        /// <param name="size">Page size. In case of <code>null</code>, uses the default SKSize.</param>
        public PdfPage(PdfDocument context, SKSize? size)
            : base(context, new PdfDictionary
            {
                { PdfName.Type,PdfName.Page },
                { PdfName.Contents,context.File.Register(new PdfStream()) }
            })
        {
            if (size.HasValue)
            { Size = size.Value; }
        }

        public PdfPage(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Gets/Sets the page's behavior in response to trigger events.</summary>
        [PDF(VersionEnum.PDF12)]
        public PageActions Actions
        {
            get => Wrap<PageActions>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.AA));
            set => BaseDataObject[PdfName.AA] = PdfObjectWrapper.GetBaseObject(value);
        }

        /// <summary>Gets/Sets the annotations associated to the page.</summary>
        public PageAnnotations Annotations
        {
            get => PageAnnotations.Wrap(BaseDataObject.GetOrCreate<PdfArray>(PdfName.Annots), this);
            set => BaseDataObject[PdfName.Annots] = PdfObjectWrapper.GetBaseObject(value);
        }

        /// <summary>Gets/Sets the extent of the page's meaningful content (including potential white space)
        /// as intended by the page's creator [PDF:1.7:10.10.1].</summary>
        /// <seealso cref="CropBox"/>
        [PDF(VersionEnum.PDF13)]
        public Rectangle ArtBox
        {
            //NOTE: The default value is the page's crop box.
            get => Wrap<Rectangle>(GetInheritableAttribute(PdfName.ArtBox)) ?? CropBox;
            set => BaseDataObject[PdfName.ArtBox] = value?.BaseDataObject;
        }

        /// <summary>Gets the page article beads.</summary>
        public PageArticleElements ArticleElements
        {
            get => PageArticleElements.Wrap(BaseDataObject.GetOrCreate<PdfArray>(PdfName.B), this);
        }

        /// <summary>Gets/Sets the region to which the contents of the page should be clipped when output
        /// in a production environment [PDF:1.7:10.10.1].</summary>
        /// <remarks>
        ///   <para>This may include any extra bleed area needed to accommodate the physical limitations of
        ///   cutting, folding, and trimming equipment. The actual printed page may include printing marks
        ///   that fall outside the bleed box.</para>
        /// </remarks>
        /// <seealso cref="CropBox"/>
        [PDF(VersionEnum.PDF13)]
        public Rectangle BleedBox
        {
            // NOTE: The default value is the page's crop box.
            get => Wrap<Rectangle>(GetInheritableAttribute(PdfName.BleedBox)) ?? CropBox;
            set => BaseDataObject[PdfName.BleedBox] = value?.BaseDataObject;
        }

        /// <summary>Gets/Sets the region to which the contents of the page are to be clipped (cropped)
        /// when displayed or printed [PDF:1.7:10.10.1].</summary>
        /// <remarks>
        ///   <para>Unlike the other boxes, the crop box has no defined meaning in terms of physical page
        ///   geometry or intended use; it merely imposes clipping on the page contents. However, in the
        ///   absence of additional information, the crop box determines how the page's contents are to be
        ///   positioned on the output medium.</para>
        /// </remarks>
        /// <seealso cref="Box"/>
        public Rectangle CropBox
        {
            //NOTE: The default value is the page's media box.
            get => Wrap<Rectangle>(GetInheritableAttribute(PdfName.CropBox)) ?? MediaBox;
            set => BaseDataObject[PdfName.CropBox] = value?.BaseDataObject;
        }

        /// <summary>Gets/Sets the page's display duration.</summary>
        /// <remarks>
        ///   <para>The page's display duration (also called its advance timing)
        ///   is the maximum length of time, in seconds, that the page is displayed
        ///   during presentations before the viewer application automatically advances
        ///   to the next page.</para>
        ///   <para>By default, the viewer does not advance automatically.</para>
        /// </remarks>
        [PDF(VersionEnum.PDF11)]
        public double Duration
        {
            get => BaseDataObject.GetDouble(PdfName.Dur);
            set => BaseDataObject.Set(PdfName.Dur, value > 0 ? value : null);
        }

        ///  <summary>Gets the index of this page.</summary>
        public int Index => index ??= CalcIndex();

        private int CalcIndex()
        {
            //NOTE: We'll scan sequentially each page-tree level above this page object
            //collecting page counts.At each level we'll scan the kids array from the
            //lower - indexed item to the ancestor of this page object at that level.

            var ancestorKidReference = (PdfReference)BaseObject;
            var parent = BaseDataObject.Get<PdfDictionary>(PdfName.Parent);
            var kids = parent.Get<PdfArray>(PdfName.Kids);
            if (kids == null)
                return 0;
            int index = 0;
            for (int i = 0; true; i++)
            {
                PdfReference kidReference = (PdfReference)kids[i];
                // Is the current-level counting complete?
                // NOTE: It's complete when it reaches the ancestor at this level.
                if (kidReference.Equals(ancestorKidReference)) // Ancestor node.
                {
                    // Does the current level correspond to the page-tree root node?
                    if (!parent.ContainsKey(PdfName.Parent))
                    {
                        // We reached the top: counting's finished.
                        return index;
                    }
                    // Set the ancestor at the next level!
                    ancestorKidReference = parent.Reference;
                    // Move up one level!
                    parent = parent.Get<PdfDictionary>(PdfName.Parent);
                    kids = parent.Get<PdfArray>(PdfName.Kids);
                    i = -1;
                }
                else // Intermediate node.
                {
                    PdfDictionary kid = (PdfDictionary)kidReference.DataObject;
                    if (PdfName.Page.Equals(kid.Get<PdfName>(PdfName.Type)))
                        index++;
                    else
                        index += kid.GetInt(PdfName.Count);
                }
            }
        }

        /// <summary>Gets the page number.</summary>
        public int Number => Index + 1;

        public TransparencyXObject Group => Wrap<TransparencyXObject>(BaseDataObject[PdfName.Group]);

        /// <summary>Gets/Sets the page size.</summary>
        public SKSize Size
        {
            get => Box.Size;
            set
            {
                SKRect box;
                try
                { box = Box; }
                catch
                { box = SKRect.Create(0, 0, 0, 0); }
                box.Size = value;
                Box = box;
            }
        }

        /// <summary>Gets/Sets the tab order to be used for annotations on the page.</summary>
        [PDF(VersionEnum.PDF15)]
        public TabOrderEnum TabOrder
        {
            get => ToTabOrderEnum(BaseDataObject.GetString(PdfName.Tabs));
            set => BaseDataObject[PdfName.Tabs] = ToCode(value);
        }

        /// <summary>Gets the transition effect to be used
        /// when displaying the page during presentations.</summary>
        [PDF(VersionEnum.PDF11)]
        public Transition Transition
        {
            get => Wrap<Transition>(BaseDataObject[PdfName.Trans]);
            set => BaseDataObject[PdfName.Trans] = PdfObjectWrapper.GetBaseObject(value);
        }

        /// <summary>Gets/Sets the intended dimensions of the finished page after trimming
        /// [PDF:1.7:10.10.1].</summary>
        /// <remarks>
        ///   <para>It may be smaller than the media box to allow for production-related content, such as
        ///   printing instructions, cut marks, or color bars.</para>
        /// </remarks>
        /// <seealso cref="CropBox"/>
        [PDF(VersionEnum.PDF13)]
        public Rectangle TrimBox
        {
            // NOTE: The default value is the page's crop box.
            get => Wrap<Rectangle>(GetInheritableAttribute(PdfName.TrimBox)) ?? CropBox;
            set => BaseDataObject[PdfName.TrimBox] = value?.BaseDataObject;
        }

        // NOTE: Mandatory.
        public Rectangle MediaBox
        {
            get => Wrap<Rectangle>(GetInheritableAttribute(PdfName.MediaBox));
            set => BaseDataObject[PdfName.MediaBox] = value?.BaseDataObject;
        }

        public SKRect Box
        {
            get => box ??= MediaBox?.ToSKRect() ?? SKRect.Empty;
            set => MediaBox = new Rectangle(value);
        }

        public SKRect RotatedBox => Box.RotateRect(Rotate);

        public ContentWrapper Contents => ContentWrapper.Wrap(BaseDataObject.GetOrCreate<PdfStream>(PdfName.Contents, false), this);

        public void Render(SKCanvas canvas, SKRect box, SKColor? clearColor = null)
        {
            var scanner = new ContentScanner(this, canvas, box, clearColor);
            scanner.Render();
        }

        public Resources Resources
        {
            get => Wrap<Resources>(GetInheritableAttribute(PdfName.Resources))
                    ?? Wrap<Resources>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.Resources));
        }

        public RotationEnum Rotation
        {
            get => RotationEnumExtension.Get((IPdfNumber)GetInheritableAttribute(PdfName.Rotate));
            set => BaseDataObject.Set(PdfName.Rotate, (int)value);
        }

        public int Rotate
        {
            get => ((IPdfNumber)GetInheritableAttribute(PdfName.Rotate))?.IntValue ?? 0;
            set
            {
                BaseDataObject.Set(PdfName.Rotate, value);
                box = null;
                rotateMatrix = null;
                invertRotateMatrix = null;
            }
        }

        public SKMatrix RotateMatrix
        {
            get => rotateMatrix ??= GraphicsState.GetRotationLeftBottomMatrix(Box, Rotate);
            set
            {
                rotateMatrix = value;
                invertRotateMatrix = null;
            }
        }

        public SKMatrix InvertRotateMatrix
        {
            get
            {
                if (invertRotateMatrix == null)
                {
                    RotateMatrix.TryInvert(out var invert);
                    invertRotateMatrix = invert;
                }
                return invertRotateMatrix.Value;
            }
        }

        public AppDataCollection AppData => AppDataCollection.Wrap(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.PieceInfo), this);

        public DateTime? ModificationDate => BaseDataObject.GetDate(PdfName.LastModified);

        public List<ITextString> Strings { get; } = new List<ITextString>();

        public Stack<GraphicsState> GetGraphicsStateContext() => states ??= new Stack<GraphicsState>();

        public AppData GetAppData(PdfName appName)
        {
            return AppData.Ensure(appName);
        }

        public void Touch(PdfName appName)
        {
            Touch(appName, DateTime.Now);
        }

        public void Touch(PdfName appName, DateTime modificationDate)
        {
            GetAppData(appName).ModificationDate = modificationDate;
            BaseDataObject.Set(PdfName.LastModified, modificationDate);
        }

        public ContentObject ToInlineObject(PrimitiveComposer composer)
        { throw new NotImplementedException(); }

        public XObject ToXObject(PdfDocument context)
        {
            FormXObject form;
            {
                form = new FormXObject(context, Box);
                form.Resources = (Resources)(
                  context == Document  // [FIX:53] Ambiguous context identity.
                    ? Resources // Same document: reuses the existing resources.
                    : Resources.Clone(context) // Alien document: clones the resources.
                  );

                // Body (contents).
                {
                    IByteStream formBody = form.BaseDataObject.Body;
                    PdfDataObject contentsDataObject = BaseDataObject.Resolve(PdfName.Contents);
                    if (contentsDataObject is PdfStream stream)
                    { formBody.Write(stream.Body); }
                    else if (contentsDataObject is PdfArray array)
                    {
                        foreach (var contentStreamObject in array)
                        { formBody.Write(((PdfStream)contentStreamObject.Resolve()).Body); }
                    }
                }
            }
            return form;
        }

        private PdfDirectObject GetInheritableAttribute(PdfName key)
        {
            return GetInheritableAttribute(BaseDataObject, key);
        }

    }
}