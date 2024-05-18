/*
  Copyright 2010-2011 Stefano Chizzolini. http://www.pdfclown.org

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

namespace PdfClown.Documents.Functions
{
    /**
      <summary>List of 1-input functions combined in a Parent stitching function</see> [PDF:1.6:3.9.3].</summary>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class Functions : PdfObjectWrapper<PdfArray>, IList<Function>
    {
        public static Functions Wrap(PdfDirectObject baseObject)
        {
            if (baseObject.Wrapper is Functions functions)
                return functions;
            return new Functions(baseObject);
        }       

        public Functions(PdfDirectObject baseObject) : base(baseObject)
        { }

        public override Object Clone(PdfDocument context)
        { return new NotImplementedException(); }

        public int IndexOf(Function value) => BaseDataObject.IndexOf(value.BaseObject);

        public void Insert(int index, Function value)
        {
            Validate(value);
            BaseDataObject.Insert(index, value.BaseObject);
        }

        public void RemoveAt(int index) => BaseDataObject.RemoveAt(index);

        public Function this[int index]
        {
            get => Function.Wrap(BaseDataObject[index]);
            set
            {
                Validate(value);
                BaseDataObject[index] = value.BaseObject;
            }
        }

        public void Add(Function value)
        {
            Validate(value);
            BaseDataObject.Add(value.BaseObject);
        }

        public void Clear() => BaseDataObject.Clear();

        public bool Contains(Function value) => BaseDataObject.Contains(value.BaseObject);

        public void CopyTo(Function[] values, int index)
        { throw new NotImplementedException(); }

        public int Count => BaseDataObject.Count;

        public bool IsReadOnly => false;

        public bool Remove(Function value) => BaseDataObject.Remove(value.BaseObject);

        IEnumerator<Function> IEnumerable<Function>.GetEnumerator()
        {
            for (int index = 0, length = Count; index < length; index++)
            { yield return this[index]; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return ((IEnumerable<Function>)this).GetEnumerator(); }

        /**
          <summary>Checks whether the specified function is valid for insertion.</summary>
          <param name="value">Function to validate.</param>
        */
        private void Validate(Function value)
        {
            if (value.InputCount != 1)
                throw new ArgumentException("value parameter MUST be 1-input function.");
        }
    }
}