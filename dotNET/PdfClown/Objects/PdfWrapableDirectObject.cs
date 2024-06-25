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

using System;

namespace PdfClown.Objects
{
    public abstract class PdfWrapableDirectObject : PdfDirectObject
    {
        protected internal IPdfObjectWrapper wrapper;
        protected internal IPdfObjectWrapper wrapper2;
        protected internal IPdfObjectWrapper wrapper3;

        protected PdfWrapableDirectObject()
        { }

        protected PdfWrapableDirectObject(PdfObjectStatus status)
            : base(status)
        { }

        public override IPdfObjectWrapper Wrapper
        {
            get => wrapper;
            internal set
            {
#if DEBUG
                if (wrapper != null && value != null)
                    throw new Exception($"Wrapper override. Existing:{wrapper}");
#endif
                wrapper = value;
            }
        }

        public override IPdfObjectWrapper Wrapper2
        {
            get => wrapper2;
            internal set
            {
#if DEBUG
                if (wrapper2 != null && value != null)
                    throw new Exception($"Wrapper2 override. Existing:{wrapper2}");
#endif
                wrapper2 = value;
            }
        }

        public override IPdfObjectWrapper Wrapper3
        {
            get => wrapper3;
            internal set
            {
#if DEBUG
                if (wrapper3 != null && value != null)
                    throw new Exception($"Wrapper3 override. Existing:{wrapper3}");
#endif
                wrapper3 = value;
            }
        }
    }
}