/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Documents.Contents.ColorSpaces;
using PdfClown.Documents.Interaction;
using actions = PdfClown.Documents.Interaction.Actions;
using PdfClown.Files;
using PdfClown.Objects;

using System;

namespace PdfClown.Documents.Multimedia
{
    /**
      <summary>Media player rules [PDF:1.7:9.1.6].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class MediaPlayers : PdfObjectWrapper<PdfDictionary>
    {
        public MediaPlayers(PdfDocument context) : base(context, new PdfDictionary())
        { }

        public MediaPlayers(PdfDirectObject baseObject) : base(baseObject)
        { }

        /**
          <summary>Gets/Sets a set of players, any of which may be used in playing the associated media object.
          </summary>
          <remarks>This collection is ignored if <see cref="RequiredPlayers"/> is non-empty.</remarks>
        */
        public Array<MediaPlayer> AllowedPlayers
        {
            get => Wrap<Array<MediaPlayer>>(BaseDataObject.GetOrCreate<PdfArray>(PdfName.A));
            set => BaseDataObject[PdfName.A] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets a set of players that must NOT be used in playing the associated media object.
          </summary>
          <remarks>This collection takes priority over <see cref="RequiredPlayers"/>.</remarks>
        */
        public Array<MediaPlayer> ForbiddenPlayers
        {
            get => Wrap<Array<MediaPlayer>>(BaseDataObject.GetOrCreate<PdfArray>(PdfName.NU));
            set => BaseDataObject[PdfName.NU] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets a set of players, one of which must be used in playing the associated media object.
          </summary>
        */
        public Array<MediaPlayer> RequiredPlayers
        {
            get => Wrap<Array<MediaPlayer>>(BaseDataObject.GetOrCreate<PdfArray>(PdfName.MU));
            set => BaseDataObject[PdfName.MU] = PdfObjectWrapper.GetBaseObject(value);
        }
    }
}