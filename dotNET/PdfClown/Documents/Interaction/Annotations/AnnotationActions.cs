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

using PdfClown.Documents.Interaction.Actions;
using PdfClown.Objects;

using system = System;

namespace PdfClown.Documents.Interaction.Annotations
{
    /// <summary>Annotation actions [PDF:1.6:8.5.2].</summary>
    [PDF(VersionEnum.PDF12)]
    public abstract class AnnotationActions : Dictionary<Action>
    {
        public class ValueWrapper : IEntryWrapper<Action>
        {
            public Action Wrap(PdfDirectObject baseObject) => Action.Wrap(baseObject);
        }

        private static readonly ValueWrapper Wrapper = new ValueWrapper();

        private Annotation parent;

        public AnnotationActions(Annotation parent) : base(parent.Document, Wrapper)
        { this.parent = parent; }

        public AnnotationActions(Annotation parent, PdfDirectObject baseObject) : base(baseObject, Wrapper)
        { this.parent = parent; }

        public override object Clone(PdfDocument context)
        { throw new system::NotImplementedException(); } // TODO: verify parent reference.

        /// <summary>Gets/Sets the action to be performed when the annotation is activated.</summary>
        public Action OnActivate
        {
            get => parent.Action;
            set => parent.Action = value;
        }

        /// <summary>Gets/Sets the action to be performed when the cursor enters the annotation's active area.</summary>
        public Action OnEnter
        {
            get => this[PdfName.E];
            set => this[PdfName.E] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the cursor exits the annotation's active area.</summary>
        public Action OnExit
        {
            get => this[PdfName.X];
            set => this[PdfName.X] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the mouse button is pressed
        /// inside the annotation's active area.</summary>
        public Action OnMouseDown
        {
            get => this[PdfName.D];
            set => this[PdfName.D] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the mouse button is released
        /// inside the annotation's active area.</summary>
        public Action OnMouseUp
        {
            get => this[PdfName.U];
            set => this[PdfName.U] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the page containing the annotation is closed.</summary>
        public Action OnPageClose
        {
            get => this[PdfName.PC];
            set => this[PdfName.PC] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the page containing the annotation
        /// is no longer visible in the viewer application's user interface.</summary>
        public Action OnPageInvisible
        {
            get => this[PdfName.PI];
            set => this[PdfName.PI] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the page containing the annotation is opened.</summary>
        public Action OnPageOpen
        {
            get => this[PdfName.PO];
            set => this[PdfName.PO] = value;
        }

        /// <summary>Gets/Sets the action to be performed when the page containing the annotation
        /// becomes visible in the viewer application's user interface.</summary>
        public Action OnPageVisible
        {
            get => this[PdfName.PV];
            set => this[PdfName.PV] = value;
        }


        public override bool ContainsKey(PdfName key)
        {
            return base.ContainsKey(key)
              || (PdfName.A.Equals(key) && parent.BaseDataObject.ContainsKey(key));
        }

        public override bool Remove(PdfName key)
        {
            if (PdfName.A.Equals(key) && parent.BaseDataObject.ContainsKey(key))
            {
                OnActivate = null;
                return true;
            }
            else
                return base.Remove(key);
        }

        public override void Clear()
        {
            base.Clear();
            OnActivate = null;
        }

        public override int Count => base.Count + (parent.BaseDataObject.ContainsKey(PdfName.A) ? 1 : 0);

        public Annotation Parent { get => parent; set => parent = value; }
    }
}