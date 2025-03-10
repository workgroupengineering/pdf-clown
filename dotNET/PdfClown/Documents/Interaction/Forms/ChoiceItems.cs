/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Objects;
using System.Collections.Generic;

namespace PdfClown.Documents.Interaction.Forms
{
    /// <summary>Field options [PDF:1.6:8.6.3].</summary>
    [PDF(VersionEnum.PDF12)]
    public sealed class ChoiceItems : ArrayWrapper<ChoiceItem>
    {
        public class ItemWrapper : IEntryWrapper<ChoiceItem>
        {
            private Dictionary<PdfDirectObject, ChoiceItem> cache = new();
            internal ChoiceItems items;

            public ItemWrapper(ChoiceItems items) => this.items = items;

            public ChoiceItem Wrap(PdfDirectObject baseObject) => baseObject?.Resolve() is PdfDirectObject dataObject
                    ? cache.TryGetValue(baseObject, out var cached) ? cached : cache[baseObject] = new ChoiceItem(baseObject, items)
                    : null;
        }

        public ChoiceItems(PdfDocument context)
            : base(context, new PdfArrayImpl(), new ItemWrapper(null))
        {
            ((ItemWrapper)itemWrapper).items = this;
        }

        public ChoiceItems(PdfDirectObject baseObject)
            : base(baseObject, new ItemWrapper(null))
        {
            ((ItemWrapper)itemWrapper).items = this;
        }

        public ChoiceItem Add(string value)
        {
            var item = new ChoiceItem(value);
            Add(item);
            return item;
        }

        public ChoiceItem Insert(int index, string value)
        {
            var item = new ChoiceItem(value);
            Insert(index, item);
            return item;
        }

        public override void Insert(int index, ChoiceItem value)
        {
            base.Insert(index, value);
            value.Items = this;
        }

        public override ChoiceItem this[int index]
        {
            get => base[index];
            set
            {
                base[index] = value;
                value.Items = this;
            }
        }

        public override void Add(ChoiceItem value)
        {
            base.Add(value);
            value.Items = this;
        }

    }
}