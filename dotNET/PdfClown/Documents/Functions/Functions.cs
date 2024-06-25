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

namespace PdfClown.Documents.Functions
{
    ///<summary>List of 1-input functions combined in a Parent stitching function</see> [PDF:1.6:3.9.3].</summary>
    [PDF(VersionEnum.PDF13)]
    public sealed class Functions : Array<Function>
    {
        public class ItemWrapper : IEntryWrapper<Function>
        {
            public Function Wrap(PdfDirectObject baseObject) => Function.Wrap(baseObject);
        }

        private static readonly ItemWrapper Wrapper = new ItemWrapper();

        public static Functions Wrap(PdfDirectObject baseObject) => baseObject != null
                ? baseObject.Wrapper is Functions functions ? functions : new Functions(baseObject)
                : null;

        public Functions(PdfDirectObject baseObject) : base(Wrapper, baseObject)
        { }

        public override Object Clone(PdfDocument context)
        { return new NotImplementedException(); }


        public override void Insert(int index, Function value)
        {
            Validate(value);
            base.Insert(index, value);
        }

        public override Function this[int index]
        {
            get => base[index];
            set
            {
                Validate(value);
                base[index] = value;
            }
        }

        public override void Add(Function value)
        {
            Validate(value);
            base.Add(value);
        }

        ///<summary>Checks whether the specified function is valid for insertion.</summary>
        ///<param name="value">Function to validate.</param>
        private void Validate(Function value)
        {
            if (value.InputCount != 1)
                throw new ArgumentException("value parameter MUST be 1-input function.");
        }
    }
}