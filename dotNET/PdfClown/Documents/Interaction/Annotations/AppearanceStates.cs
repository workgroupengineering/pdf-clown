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

using PdfClown.Documents.Contents.XObjects;
using PdfClown.Objects;
using PdfClown.Util.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PdfClown.Documents.Interaction.Annotations
{
    /// <summary>Appearance states [PDF:1.6:8.4.4].</summary>
    [PDF(VersionEnum.PDF12)]
    public sealed class AppearanceStates : PdfObjectWrapper2<PdfDataObject>, IDictionary<PdfName, FormXObject>
    {
        public static AppearanceStates Wrap(PdfName statesKey, Appearance appearance)
        {
            return new AppearanceStates(statesKey, appearance);
        }
        private Appearance appearance;

        private PdfName statesKey;

        public AppearanceStates(PdfName statesKey, Appearance appearance)
            : base(appearance.BaseDataObject[statesKey])
        {
            this.appearance = appearance;
            this.statesKey = statesKey;
        }

        public AppearanceStates(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Gets the appearance associated to these states.</summary>
        public Appearance Appearance => appearance;

        public override object Clone(PdfDocument context)
        { throw new NotImplementedException(); } // TODO: verify appearance reference.

        //TODO
        /**
          Gets the key associated to a given value.
        */
        //   public PdfName GetKey(
        //     FormXObject value
        //     )
        //   {return BaseDataObject.GetKey(value.BaseObject);}

        public void Add(PdfName key, FormXObject value)
        { EnsureDictionary()[key] = value.BaseObject; }

        public bool ContainsKey(PdfName key)
        {
            PdfDataObject baseDataObject = BaseDataObject;
            if (baseDataObject == null) // No state.
                return false;
            else if (baseDataObject is PdfStream) // Single state.
                return (key == null);
            else // Multiple state.
                return ((PdfDictionary)baseDataObject).ContainsKey(key);
        }

        public ICollection<PdfName> Keys => BaseDataObject is PdfStream ? new SingleItemCollection<PdfName>(null)
            : BaseDataObject is PdfDictionary dict ? dict.Keys : EmptyCollection<PdfName>.Default;

        public bool Remove(PdfName key)
        {
            PdfDataObject baseDataObject = BaseDataObject;
            if (baseDataObject == null) // No state.
                return false;
            else if (baseDataObject is PdfStream) // Single state.
            {
                if (key == null)
                {
                    BaseObject = null;
                    appearance.BaseDataObject.Remove(statesKey);
                    return true;
                }
                else // Invalid key.
                    return false;
            }
            else // Multiple state.
                return ((PdfDictionary)baseDataObject).Remove(key);
        }

        public FormXObject this[PdfName key]
        {
            get
            {
                PdfDataObject baseDataObject = BaseDataObject;
                if (baseDataObject == null) // No state.
                    return null;
                else if (key == null)
                {
                    if (baseDataObject is PdfStream) // Single state.
                        return FormXObject.Wrap(BaseObject);
                    else // Multiple state, but invalid key.
                        return null;
                }
                else // Multiple state.
                    return FormXObject.Wrap(((PdfDictionary)baseDataObject)[key]);
            }
            set
            {
                if (key == null) // Single state.
                {
                    BaseObject = value?.BaseObject;
                    appearance.BaseDataObject[statesKey] = BaseObject;
                }
                else // Multiple state.
                { EnsureDictionary()[key] = value.BaseObject; }
            }
        }

        public bool TryGetValue(PdfName key, out FormXObject value)
        {
            value = this[key];
            return (value != null || ContainsKey(key));
        }

        public ICollection<FormXObject> Values => throw new NotImplementedException();

        void ICollection<KeyValuePair<PdfName, FormXObject>>.Add(KeyValuePair<PdfName, FormXObject> entry) => Add(entry.Key, entry.Value);

        public void Clear() => EnsureDictionary().Clear();

        bool ICollection<KeyValuePair<PdfName, FormXObject>>.Contains(KeyValuePair<PdfName, FormXObject> entry)
        {
            PdfDataObject baseDataObject = BaseDataObject;
            if (baseDataObject == null) // No state.
                return false;
            else if (baseDataObject is PdfStream) // Single state.
                return entry.Value.BaseObject.Equals(BaseObject);
            else // Multiple state.
                return entry.Value.Equals(this[entry.Key]);
        }

        public void CopyTo(KeyValuePair<PdfName, FormXObject>[] entries, int index)
        {
            foreach (var entry in this)
            {
                entries[index++] = entry;
            }
        }

        public int Count
        {
            get
            {
                PdfDataObject baseDataObject = BaseDataObject;
                if (baseDataObject == null) // No state.
                    return 0;
                else if (baseDataObject is PdfStream) // Single state.
                    return 1;
                else // Multiple state.
                    return ((PdfDictionary)baseDataObject).Count;
            }
        }

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<PdfName, FormXObject> entry)
        { throw new NotImplementedException(); }

        IEnumerator<KeyValuePair<PdfName, FormXObject>> IEnumerable<KeyValuePair<PdfName, FormXObject>>.GetEnumerator()
        {
            PdfDataObject baseDataObject = BaseDataObject;
            if (baseDataObject == null) // No state.
            { /* NOOP. */ }
            else if (baseDataObject is PdfStream) // Single state.
            {
                yield return new KeyValuePair<PdfName, FormXObject>(
                  null,
                  FormXObject.Wrap(BaseObject));
            }
            else // Multiple state.
            {
                foreach (var entry in ((PdfDictionary)baseDataObject))
                {
                    yield return new KeyValuePair<PdfName, FormXObject>(
                      entry.Key,
                      FormXObject.Wrap(entry.Value));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<PdfName, FormXObject>>)this).GetEnumerator();

        private PdfDictionary EnsureDictionary()
        {
            PdfDataObject baseDataObject = BaseDataObject;
            if (baseDataObject is PdfStream stream)
            {
                // NOTE: Single states are erased as they have no valid key
                // to be consistently integrated within the dictionary.
                BaseObject = (PdfDirectObject)(baseDataObject = new PdfDictionary());
                appearance.BaseDataObject[statesKey] = (PdfDirectObject)baseDataObject;
            }
            else if (baseDataObject == null)
            {
                BaseObject = (PdfDirectObject)(baseDataObject = new PdfDictionary());
                appearance.BaseDataObject[statesKey] = (PdfDirectObject)baseDataObject;
            }
            return (PdfDictionary)baseDataObject;
        }
    }
}