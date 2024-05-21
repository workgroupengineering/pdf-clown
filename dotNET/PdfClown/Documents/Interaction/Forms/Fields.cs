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

using PdfClown.Bytes;
using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Files;
using PdfClown.Objects;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PdfClown.Documents.Interaction.Forms
{
    /**
      <summary>Interactive form fields [PDF:1.6:8.6.1].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class Fields : PdfObjectWrapper<PdfArray>, IDictionary<string, Field>, IEnumerable<Field>
    {
        private Dictionary<string, Field> cache;
        public Fields(PdfDocument context) : base(context, new PdfArray())
        { }

        public Fields(PdfDirectObject baseObject) : base(baseObject)
        { }

        private Dictionary<string, Field> Cache => cache ??= RefreshCache();

        public ICollection<string> Keys => Cache.Keys;

        public ICollection<Field> Values => Cache.Values;

        public int Count => Cache.Count;

        public bool IsReadOnly => false;

        public Field this[string key]
        {
            get => TryGetValue(key, out var field) ? field : null;
            set => Add(key, value);
        }

        private Dictionary<string, Field> RefreshCache()
        {
            var cache = new Dictionary<string, Field>();

            foreach (var field in RetrieveValues(BaseDataObject))
            {
                cache[field.FullName] = field;
            }
            return cache;
        }

        public void Add(Field value) => Add(value.FullName, value);

        public void Add(string key, Field value)
        {
            Cache[key] = value;
            var fieldObjects = GetHolderArray(value);
            fieldObjects.Add(value.BaseObject);
        }

        public bool ContainsKey(string key) => Cache.ContainsKey(key);

        public bool Remove(string key) => TryGetValue(key, out var field) && Remove(field);

        public bool Remove(Field field)
        {
            Cache.Remove(field.Name);
            var fieldObjects = GetHolderArray(field);
            return fieldObjects.Remove(field.BaseObject);
        }

        private PdfArray GetHolderArray(Field field)
        {
            PdfArray fieldObjects;
            {
                var fieldParentReference = field.BaseDataObject.Get<PdfReference>(PdfName.Parent);
                if (fieldParentReference == null)
                { fieldObjects = BaseDataObject; }
                else
                { fieldObjects = ((PdfDictionary)fieldParentReference.DataObject).Get<PdfArray>(PdfName.Kids); }
            }

            return fieldObjects;
        }

        public bool TryGetValue(string key, out Field value) => Cache.TryGetValue(key, out value);

        void ICollection<KeyValuePair<string, Field>>.Add(KeyValuePair<string, Field> entry) => Add(entry.Key, entry.Value);

        public void Clear()
        {
            Cache.Clear();
            BaseDataObject.Clear();
        }

        bool ICollection<KeyValuePair<string, Field>>.Contains(KeyValuePair<string, Field> entry) => Cache.Contains(entry);

        public void CopyTo(KeyValuePair<string, Field>[] entries, int index)
        { throw new NotImplementedException(); }

        public bool Remove(KeyValuePair<string, Field> entry) => throw new NotImplementedException();

        IEnumerator<KeyValuePair<string, Field>> IEnumerable<KeyValuePair<string, Field>>.GetEnumerator() => Cache.GetEnumerator();

        public IEnumerator<Field> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IList<Field> RetrieveValues(PdfArray fieldObjects)
        {
            var list = new List<Field>();
            RetrieveValues(fieldObjects, list);
            return list;
        }

        private void RetrieveValues(PdfArray fieldObjects, IList<Field> values)
        {
            foreach (PdfReference fieldReference in fieldObjects)
            {
                var kidReferences = ((PdfDictionary)fieldReference.DataObject).Get<PdfArray>(PdfName.Kids);
                PdfDictionary kidObject;
                if (kidReferences == null)
                { kidObject = null; }
                else
                { kidObject = (PdfDictionary)((PdfReference)kidReferences[0]).DataObject; }
                // Terminal field?
                if (kidObject == null // Merged single widget annotation.
                  || (!kidObject.ContainsKey(PdfName.FT) // Multiple widget annotations.
                    && kidObject.ContainsKey(PdfName.Subtype)
                    && kidObject[PdfName.Subtype].Equals(PdfName.Widget)))
                { values.Add(Field.Wrap(fieldReference)); }
                else // Non-terminal field.
                { RetrieveValues(kidReferences, values); }
            }
        }
    }
}