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

using PdfClown.Documents;
using System;

namespace PdfClown.Objects
{
    public abstract class PdfObjectWrapper2 : PdfObjectWrapper
    {
        ///<summary>Instantiates an empty wrapper.</summary>
        protected PdfObjectWrapper2()
        { }

        ///<summary>Instantiates a wrapper from the specified base object.</summary>
        ///<param name="baseObject">PDF object backing this wrapper. MUST be a <see cref="PdfReference"/>
        ///every time available.</param>
        public PdfObjectWrapper2(PdfDirectObject baseObject)
        {
            BaseObject = baseObject;

            if (baseObject != null)
                baseObject.Wrapper2 = this;            
        }

        ///<summary>Gets a clone of the object, registered using the specified object cloner.</summary>
        public override object Clone(Cloner cloner)
        {
            var clone = (PdfObjectWrapper2)base.MemberwiseClone();
            clone.BaseObject = (PdfDirectObject)BaseObject.Clone(cloner);
            if (clone.BaseObject != null)
                clone.BaseObject.Wrapper2 = clone;

            return clone;
        }
    }

    public abstract class PdfObjectWrapper2<TDataObject> : PdfObjectWrapper2
      where TDataObject : PdfDataObject
    {
        ///<summary>Instantiates an empty wrapper.</summary>
        protected PdfObjectWrapper2()
        { }

        ///<summary>Instantiates a wrapper from the specified base object.</summary>
        ///<param name="baseObject">PDF object backing this wrapper. It MUST be a <see cref="PdfReference"/>
        ///every time available.</param>
        public PdfObjectWrapper2(PdfDirectObject baseObject) : base(baseObject)
        { }

        ///<summary>Instantiates a wrapper registering the specified base data object into the specified
        ///document context.</summary>
        ///<param name="context">Document context into which the specified data object has to be
        ///registered.</param>
        ///<param name="baseDataObject">PDF data object backing this wrapper.</param>
        ///<seealso cref="PdfObjectWrapper(PdfFile, PdfDataObject)"/>
        protected PdfObjectWrapper2(PdfDocument context, TDataObject baseDataObject)
            : this(context?.File, baseDataObject)
        { }

        ///<summary>Instantiates a wrapper registering the specified base data object into the specified
        ///file context.</summary>
        ///<param name="context">File context into which the specified data object has to be registered.
        ///</param>
        ///<param name="baseDataObject">PDF data object backing this wrapper.</param>
        ///<seealso cref="PdfObjectWrapper(PdfDocument, PdfDataObject)"/>
        protected PdfObjectWrapper2(PdfFile context, TDataObject baseDataObject)
            : this(context != null ? context.Register(baseDataObject) : (PdfDirectObject)(PdfDataObject)baseDataObject)
        { }

        ///<summary>Gets the underlying data object.</summary>
        public new TDataObject BaseDataObject => (TDataObject)PdfObject.Resolve(BaseObject);
    }
}