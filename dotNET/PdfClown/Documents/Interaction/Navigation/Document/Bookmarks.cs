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

using PdfClown.Objects;

using System;
using System.Collections;
using System.Collections.Generic;

namespace PdfClown.Documents.Interaction.Navigation
{
    /// <summary>Collection of bookmarks [PDF:1.6:8.2.2].</summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class Bookmarks : PdfObjectWrapper2<PdfDictionary>, IList<Bookmark>
    {
        public Bookmarks(PdfDocument context)
            : base(context, new PdfDictionary
            {
                { PdfName.Type, PdfName.Outlines },
                { PdfName.Count, PdfInteger.Default },
            })
        { }

        public Bookmarks(PdfDirectObject baseObject) : base(baseObject)
        { }

        public int IndexOf(Bookmark bookmark)
        { throw new NotImplementedException(); }

        public void Insert(int index, Bookmark bookmark)
        { throw new NotImplementedException(); }

        public void RemoveAt(int index)
        { throw new NotImplementedException(); }

        public Bookmark this[int index]
        {
            get
            {
                var bookmarkObject = BaseDataObject.Get<PdfReference>(PdfName.First);
                while (index > 0)
                {
                    bookmarkObject = ((PdfDictionary)bookmarkObject.DataObject).Get<PdfReference>(PdfName.Next);
                    // Did we go past the collection range?
                    if (bookmarkObject == null)
                        throw new ArgumentOutOfRangeException();

                    index--;
                }

                return Wrap<Bookmark>(bookmarkObject);
            }
            set => throw new NotImplementedException();
        }

        public void Add(Bookmark bookmark)
        {
            // NOTE: Bookmarks imported from alien PDF files MUST be cloned
            // before being added.
            bookmark.BaseDataObject[PdfName.Parent] = BaseObject;

            PdfInteger countObject = EnsureCountObject();
            // Is it the first bookmark?
            if (countObject.RawValue == 0) // First bookmark.
            {
                BaseDataObject[PdfName.Last]
                  = BaseDataObject[PdfName.First]
                  = bookmark.BaseObject;
                BaseDataObject.Set(PdfName.Count, countObject.IntValue + 1);
            }
            else // Non-first bookmark.
            {
                var oldLastBookmarkReference = BaseDataObject.Get<PdfReference>(PdfName.Last);
                BaseDataObject[PdfName.Last] // Added bookmark is the last in the collection...
                  = ((PdfDictionary)oldLastBookmarkReference.DataObject)[PdfName.Next] // ...and the next of the previously-last bookmark.
                  = bookmark.BaseObject;
                bookmark.BaseDataObject[PdfName.Prev] = oldLastBookmarkReference;

                // NOTE: The Count entry is a relative number (whose sign represents
                // the node open state).
                BaseDataObject.Set(PdfName.Count, countObject.IntValue + Math.Sign(countObject.IntValue));
            }
        }

        public void Clear()
        { throw new NotImplementedException(); }

        public bool Contains(Bookmark bookmark)
        { throw new NotImplementedException(); }

        public void CopyTo(Bookmark[] bookmarks, int index)
        { throw new NotImplementedException(); }

        /// <summary>
        /// NOTE: The Count entry may be absent [PDF:1.6:8.2.2].
        /// </summary>
        public int Count => BaseDataObject.GetInt(PdfName.Count);

        public bool IsReadOnly => false;

        public bool Remove(Bookmark bookmark)
        { throw new NotImplementedException(); }

        IEnumerator<Bookmark> IEnumerable<Bookmark>.GetEnumerator()
        {
            PdfDirectObject bookmarkObject = BaseDataObject[PdfName.First];
            if (bookmarkObject == null)
                yield break;

            do
            {
                yield return Wrap<Bookmark>(bookmarkObject);

                bookmarkObject = ((PdfDictionary)bookmarkObject.Resolve())[PdfName.Next];
            } while (bookmarkObject != null);
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return ((IEnumerable<Bookmark>)this).GetEnumerator(); }

        /// <summary>Gets the count object, forcing its creation if it doesn't
        /// exist.</summary>
        private PdfInteger EnsureCountObject()
        {
            /*
              NOTE: The Count entry may be absent [PDF:1.6:8.2.2].
            */
            PdfInteger countObject = BaseDataObject.Get<PdfInteger>(PdfName.Count);
            if (countObject == null)
            { BaseDataObject[PdfName.Count] = countObject = PdfInteger.Default; }

            return countObject;
        }
    }
}