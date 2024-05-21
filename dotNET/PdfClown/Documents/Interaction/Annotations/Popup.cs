/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Bytes;
using PdfClown.Documents;
using PdfClown.Documents.Contents.ColorSpaces;
using PdfClown.Objects;

using System;
using SkiaSharp;
using System.Collections.Generic;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.Documents.Contents.XObjects;

namespace PdfClown.Documents.Interaction.Annotations
{
    /**
      <summary>Pop-up annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It displays text in a pop-up window for entry and editing.
      It typically does not appear alone but is associated with a markup annotation,
      its parent annotation, and is used for editing the parent's text.</remarks>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class Popup : Annotation
    {
        private Markup parent;

        public Popup(PdfPage page, SKRect box, string text)
            : base(page, PdfName.Popup, box, text)
        { }

        public Popup(PdfDirectObject baseObject)
            : base(baseObject)
        { }


        //public override PdfPage Page
        //{
        //    get => Parent?.Page ?? base.Page;
        //    set
        //    {
        //        if (Parent != null)
        //        { parent.Page = value; }
        //        else
        //        { base.Page = value; }
        //    }
        //}

        public override DeviceColor Color
        {
            get => Parent?.Color ?? base.Color;
            set
            {
                if (Parent != null)
                { parent.Color = value; }
                else
                { base.Color = value; }
            }
        }

        /**
          <summary>Gets/Sets whether the annotation should initially be displayed open.</summary>
        */
        public bool IsOpen
        {
            get => BaseDataObject.GetBool(PdfName.Open, false);
            set
            {
                var oldValue = IsOpen;
                if (oldValue != value)
                {
                    BaseDataObject.SetBool(PdfName.Open, value);
                    OnPropertyChanged(oldValue, value);
                }
            }
        }

        /**
          <summary>Gets the markup associated with this annotation.</summary>
        */
        public Markup Parent
        {
            get => parent ??= (Markup)Annotation.Wrap(BaseDataObject[PdfName.Parent]);
            set
            {

                var oldValue = Parent;
                if (oldValue != value)
                {
                    BaseDataObject[PdfName.Parent] = value?.BaseObject;
                    if (value != null)
                    {
                        /*
                          NOTE: The markup annotation's properties override those of this pop-up annotation.
                        */
                        BaseDataObject.Remove(PdfName.Contents);
                        BaseDataObject.Remove(PdfName.M);
                        BaseDataObject.Remove(PdfName.C);
                    }
                    OnPropertyChanged(oldValue, value);
                }
            }
        }

        public override DateTime? ModificationDate
        {
            get => Parent?.ModificationDate ?? base.ModificationDate;
            set
            {
                if (Parent != null)
                { parent.ModificationDate = value; }
                else
                { base.ModificationDate = value; }
            }
        }

        public override string Contents
        {
            get => Parent?.Contents ?? base.Contents;
            set
            {
                if (Parent != null)
                { parent.Contents = value; }
                else
                { base.Contents = value; }
            }
        }

        public override IEnumerable<ControlPoint> GetControlPoints()
        {
            foreach (var cpBase in GetDefaultControlPoint())
            {
                yield return cpBase;
            }
        }

        protected override FormXObject GenerateAppearance()
        {
            return null;
        }
    }
}