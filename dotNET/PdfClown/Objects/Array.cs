/*
  Copyright 2011-2013 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PdfClown.Objects
{
    /**
      <summary>Collection of sequentially-arranged object wrappers.</summary>
    */
    public class Array<TItem> : PdfObjectWrapper<PdfArray>, IList<TItem>
        where TItem : IPdfObjectWrapper
    {
        /**
          <summary>Item instancer.</summary>
        */
        public interface IWrapper<T> where T : TItem
        {
            T Wrap(PdfDirectObject baseObject);
        }

        private class DefaultWrapper<T> : IWrapper<T> where T : TItem
        {
            internal DefaultWrapper()
            { }

            public T Wrap(PdfDirectObject baseObject)
            {
                return PdfObjectWrapper.Wrap<T>(baseObject);
            }
        }

        protected IWrapper<TItem> itemWrapper;

        /**
          <summary>Wraps a new base array using the default wrapper for wrapping its items.</summary>
          <param name="context">Document context.</param>
        */
        public Array(PdfDocument context)
            : this(context, new PdfArray())
        { }

        /**
          <summary>Wraps a new base array using the specified wrapper for wrapping its items.</summary>
          <param name="context">Document context.</param>
          <param name="itemWrapper">Item wrapper.</param>
        */
        public Array(PdfDocument context, IWrapper<TItem> itemWrapper)
            : this(context, itemWrapper, new PdfArray())
        { }

        /**
          <summary>Wraps the specified base array using the default wrapper for wrapping its items.</summary>
          <param name="context">Document context.</param>
          <param name="baseDataObject">Base array.</param>
        */
        public Array(PdfDocument context, PdfArray baseDataObject)
            : this(context, new DefaultWrapper<TItem>(), baseDataObject)
        { }

        /**
          <summary>Wraps the specified base array using the specified wrapper for wrapping its items.</summary>
          <param name="context">Document context.</param>
          <param name="itemWrapper">Item wrapper.</param>
          <param name="baseDataObject">Base array.</param>
        */
        public Array(PdfDocument context, IWrapper<TItem> itemWrapper, PdfArray baseDataObject)
            : base(context, baseDataObject)
        {
            this.itemWrapper = itemWrapper;
        }

        /**
          <summary>Wraps an existing base array using the default wrapper for wrapping its items.</summary>
          <param name="baseObject">Base array. MUST be a <see cref="PdfReference">reference</see>
          everytime available.</param>
        */
        public Array(PdfDirectObject baseObject)
            : this(new DefaultWrapper<TItem>(), baseObject)
        { }

        /**
          <summary>Wraps an existing base array using the specified wrapper for wrapping its items.</summary>
          <param name="itemWrapper">Item wrapper.</param>
          <param name="baseObject">Base array. MUST be a <see cref="PdfReference">reference</see>
          everytime available.</param>
        */
        public Array(IWrapper<TItem> itemWrapper, PdfDirectObject baseObject) : base(baseObject)
        {
            this.itemWrapper = itemWrapper;
        }

        public virtual int IndexOf(TItem item) => BaseDataObject.IndexOf(item.BaseObject);

        public virtual void Insert(int index, TItem item) => BaseDataObject.Insert(index, item.BaseObject);

        public virtual void RemoveAt(int index) => BaseDataObject.RemoveAt(index);

        public virtual TItem this[int index]
        {
            get => itemWrapper.Wrap(BaseDataObject[index]);
            set => BaseDataObject[index] = value.BaseObject;
        }

        public virtual void Add(TItem item) => BaseDataObject.Add(item.BaseObject);

        public virtual void Clear()
        {
            int index = Count;
            while (index-- > 0)
            { RemoveAt(index); }
        }

        public virtual bool Contains(TItem item) => BaseDataObject.Contains(item.BaseObject);

        public virtual void CopyTo(TItem[] items, int index)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = this[index + i];
            }
        }

        public virtual int Count => BaseDataObject.Count;

        public virtual bool IsReadOnly => false;

        public virtual bool Remove(TItem item) => BaseDataObject.Remove(item.BaseObject);

        public virtual IEnumerator<TItem> GetEnumerator()
        {
            for (int index = 0, length = Count; index < length; index++)
            { yield return this[index]; }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}