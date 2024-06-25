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

using System;
using System.Collections.Generic;

namespace PdfClown.Documents.Interaction.Actions
{
    ///<summary>Action to be performed by the viewer application [PDF:1.6:8.5].</summary>
    [PDF(VersionEnum.PDF11)]
    public class Action : PdfObjectWrapper<PdfDictionary>, ITextDisplayable
    {
        private static readonly Dictionary<PdfName, Func<PdfDirectObject, Action>> nCache = new(4)
        {
             { PdfName.NextPage, (baseObject) => new GoToNextPage(baseObject) },
             { PdfName.PrevPage, (baseObject) => new GoToPreviousPage(baseObject) },
             { PdfName.FirstPage, (baseObject) => new GoToFirstPage(baseObject) },
             { PdfName.LastPage, (baseObject) => new GoToLastPage(baseObject) },
        };

        private static readonly Dictionary<PdfName, Func<PdfDirectObject, Action>> sCache = new(32)
        {
            { PdfName.GoTo, (baseObject) => new GoToLocal(baseObject) },
            { PdfName.GoToR, (baseObject) => new GoToRemote(baseObject) },
            { PdfName.GoToE, (baseObject) => new GoToEmbedded(baseObject) },
            { PdfName.Launch, (baseObject) => new Launch(baseObject) },
            { PdfName.Thread, (baseObject) => new GoToThread(baseObject) },
            { PdfName.URI, (baseObject) => new GoToURI(baseObject) },
            { PdfName.Sound, (baseObject) => new PlaySound(baseObject) },
            { PdfName.Movie, (baseObject) => new PlayMovie(baseObject) },
            { PdfName.Hide, (baseObject) => new ToggleVisibility(baseObject) },
            { PdfName.SubmitForm, (baseObject) => new SubmitForm(baseObject) },
            { PdfName.ResetForm, (baseObject) => new ResetForm(baseObject) },
            { PdfName.ImportData, (baseObject) => new ImportData(baseObject) },
            { PdfName.JavaScript, (baseObject) => new JavaScript(baseObject) },
            { PdfName.SetOCGState, (baseObject) => new SetLayerState(baseObject) },
            { PdfName.Rendition, (baseObject) => new Render(baseObject) },
            { PdfName.Trans, (baseObject) => new DoTransition(baseObject) },
            { PdfName.GoTo3DView, (baseObject) => new GoTo3dView(baseObject) },
            { PdfName.Named, (baseObject) =>
            {
                var dataObject = (PdfDictionary)baseObject.Resolve();
                var actionName = dataObject.Get<PdfName>(PdfName.N);
                return nCache.TryGetValue(actionName, out var func)
                    ? func(baseObject)
                    : new NamedAction(baseObject);
            }},
        };

        ///<summary>Wraps an action base object into an action object.</summary>
        ///<param name="baseObject">Action base object.</param>
        ///<returns>Action object associated to the base object.</returns>
        public static Action Wrap(PdfDirectObject baseObject)
        {
            if (baseObject == null)
                return null;
            if (baseObject.Wrapper is Action action)
                return action;

            var dataObject = (PdfDictionary)baseObject.Resolve();
            var actionType = dataObject.Get<PdfName>(PdfName.S);
            if (actionType == null
              || (dataObject.ContainsKey(PdfName.Type)
                  && !PdfName.Action.Equals(dataObject.Get<PdfName>(PdfName.Type))))
                return null;

            return sCache.TryGetValue(actionType, out var func)
                ? func(baseObject)
                : new Action(baseObject);
        }

        ///<summary>Creates a new action within the given document context.</summary>
        protected Action(PdfDocument context, PdfName actionType) : base(
            context,
            new PdfDictionary(2)
            {
                { PdfName.Type, PdfName.Action },
                { PdfName.S, actionType }
            })
        { }

        public Action(PdfDirectObject baseObject) : base(baseObject)
        { }

        ///<summary>Gets/Sets the actions to be performed after the current one.</summary>
        [PDF(VersionEnum.PDF12)]
        public ChainedActions Actions
        {
            get => ChainedActions.Wrap(BaseDataObject[PdfName.Next], this);
            set => BaseDataObject[PdfName.Next] = value.BaseObject;
        }

        public virtual string GetDisplayName() => string.Empty;
    }
}