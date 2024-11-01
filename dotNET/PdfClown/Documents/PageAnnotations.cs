/*
  Copyright 2008-2013 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Objects;

using System;
using System.Collections.Generic;

namespace PdfClown.Documents
{
    /// <summary>Page annotations [PDF:1.6:3.6.2].</summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class PageAnnotations : PageElements<Annotation>
    {
        public class AnnotationWrapper : IEntryWrapper<Annotation>
        {
            public AnnotationWrapper(PdfPage page)
            {
                Page = page;
            }

            public PdfPage Page { get; }

            public Annotation Wrap(PdfDirectObject baseObject)
            {
                var annotation = Annotation.Wrap(baseObject);
                if (annotation != null)
                {
                    Page.Annotations.AddIndex(annotation);
                }
                return annotation;
            }
        }

        public static PageAnnotations Wrap(PdfDirectObject baseObject, PdfPage page) => baseObject != null
                ? baseObject.Wrapper as PageAnnotations ?? new PageAnnotations(baseObject, page)
                : null;


        private readonly Dictionary<string, Annotation> nameIndex = new Dictionary<string, Annotation>(StringComparer.Ordinal);

        internal PageAnnotations(PdfDirectObject baseObject, PdfPage page)
            : base(new AnnotationWrapper(page), baseObject, page)
        {
            RefreshCache();
        }

        public Annotation this[string name] => nameIndex.TryGetValue(name, out var annotation) ? annotation : null;

        private void RefreshCache()
        {
            for (int i = 0, length = Count; i < length; i++)
            {
                var item = this[i];
                if (item == null)
                {
                    RemoveAt(i);
                    length--;
                    i--;
                    continue;
                }
                //Recovery
                if (item is Markup markup
                    && markup.Popup != null
                    && !Contains(markup.Popup))
                {
                    Add(markup.Popup);
                }
                if (item is Popup popup
                       && popup.Parent != null
                       && !Contains(popup.Parent))
                {
                    Add(popup.Parent);
                }
                AddIndex(item);
            }
        }

        private void AddIndex(Annotation annotation)
        {
            if (annotation is Markup
                || annotation is Widget)
            {
                //Recovery
                if (annotation.Page == null)
                {
                    LinkPageNoUpdate(annotation);
                }

                if (string.IsNullOrEmpty(annotation.Name)
                    || (nameIndex.TryGetValue(annotation.Name, out var existing)
                    && existing != annotation))
                {
                    annotation.GenerateExistingName();
                }
                nameIndex[annotation.Name] = annotation;
            }
            else if(annotation.Page == null)
            {
                annotation.page = Page;
            }
        }

        public override void Add(Annotation item)
        {
            if (item.IsQueueRefreshAppearance)
            {
                item.RefreshAppearance();
            }
            AddIndex(item);
            base.Add(item);
        }

        public override void Insert(int index, Annotation item)
        {
            AddIndex(item);
            base.Insert(index, item);
        }

        public override bool Remove(Annotation item)
        {
            nameIndex.Remove(item.Name);
            return base.Remove(item);
        }

        public override void RemoveAt(int index)
        {
            var item = this[index];
            if (item != null)
                nameIndex.Remove(item.Name);
            base.RemoveAt(index);
        }
    }
}