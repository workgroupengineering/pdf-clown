/*
  Copyright 2012-2015 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Files;
using PdfClown.Objects;

using System;
using System.Collections;
using System.Collections.Generic;

namespace PdfClown.Documents
{
    /**
      <summary>Page elements.</summary>
    */
    public abstract class PageElements<TItem> : Array<TItem>
        where TItem : PdfObjectWrapper<PdfDictionary>
    {
        private PdfPage page;

        public PageElements(PdfDirectObject baseObject, PdfPage page)
            : base(baseObject)
        {
            this.page = page;
        }

        public PageElements(IWrapper<TItem> itemWrapper, PdfDirectObject baseObject, PdfPage page)
            : base(itemWrapper, baseObject)
        {
            this.page = page;
        }

        public override void Add(TItem item)
        {
            DoAdd(item);
            base.Add(item);
        }

        public override object Clone(PdfDocument context)
        {
            throw new NotSupportedException();
        }

        public override void Insert(int index, TItem item)
        {
            DoAdd(item);
            base.Insert(index, item);
        }

        /**
          <summary>Gets the page associated to these elements.</summary>
        */
        public PdfPage Page => page;

        public override void RemoveAt(int index)
        {
            TItem @object = this[index];
            base.RemoveAt(index);
            DoRemove(@object);
        }

        public override bool Remove(TItem item)
        {
            if (!base.Remove(item))
                return false;

            DoRemove((TItem)item);
            return true;
        }

        protected internal virtual void DoAdd(TItem item)
        {
            // Link the element to its page!
            item.BaseDataObject[PdfName.P] = page.BaseObject;
        }

        protected virtual void DoRemove(TItem item)
        {
            // Unlink the element from its page!
            item?.BaseDataObject.Remove(PdfName.P);
        }
    }
}