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

using PdfClown.Documents.Files;
using PdfClown.Documents.Interaction.Actions;
using PdfClown.Documents.Interaction.Navigation;
using PdfClown.Documents.Multimedia;
using PdfClown.Objects;
using PdfClown.Util;
using System;

namespace PdfClown.Documents
{
    /// <summary>Name dictionary [PDF:1.6:3.6.3].</summary>
    [PDF(VersionEnum.PDF12)]
    public sealed class Names : PdfObjectWrapper<PdfDictionary>, ICompositeDictionary<PdfString>
    {
        public Names(PdfDocument context) : base(context, new PdfDictionary())
        { }

        public Names(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Gets/Sets the named destinations.</summary>
        [PDF(VersionEnum.PDF12)]
        public NamedDestinations Destinations
        {
            get => Wrap<NamedDestinations>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.Dests, false));
            set => BaseDataObject[PdfName.Dests] = value.BaseObject;
        }

        /// <summary>Gets/Sets the named embedded files.</summary>
        [PDF(VersionEnum.PDF14)]
        public NamedEmbeddedFiles EmbeddedFiles
        {
            get => Wrap<NamedEmbeddedFiles>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.EmbeddedFiles, false));
            set => BaseDataObject[PdfName.EmbeddedFiles] = value.BaseObject;
        }

        /// <summary>Gets/Sets the named JavaScript actions.</summary>
        [PDF(VersionEnum.PDF13)]
        public NamedJavaScripts JavaScripts
        {
            get => Wrap<NamedJavaScripts>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.JavaScript, false));
            set => BaseDataObject[PdfName.JavaScript] = value.BaseObject;
        }

        /// <summary>Gets/Sets the named pages.</summary>
        [PDF(VersionEnum.PDF13)]
        public NamedPages Pages
        {
            get => Wrap<NamedPages>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.Pages, false));
            set => BaseDataObject[PdfName.Pages] = value.BaseObject;
        }

        /// <summary>Gets/Sets the named renditions.</summary>
        [PDF(VersionEnum.PDF15)]
        public NamedRenditions Renditions
        {
            get => Wrap<NamedRenditions>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.Renditions, false));
            set => BaseDataObject[PdfName.Renditions] = value.BaseObject;
        }

        public IBiDictionary Get(Type type)
        {
            if (typeof(Destination).IsAssignableFrom(type))
                return Destinations;
            else if (typeof(FileSpecification).IsAssignableFrom(type))
                return EmbeddedFiles;
            else if (typeof(JavaScript).IsAssignableFrom(type))
                return JavaScripts;
            else if (typeof(PdfPage).IsAssignableFrom(type))
                return Pages;
            else if (typeof(Rendition).IsAssignableFrom(type))
                return Renditions;
            else
                return null;
        }

        public T Get<T>(PdfString key) where T : PdfObjectWrapper
        {
            return (T)Get(typeof(T))?[key];
        }
    }
}