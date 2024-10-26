/*
  Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Documents.Interchange.Metadata;
using PdfClown.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PdfClown.Objects
{
    public abstract class Dictionary<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TValue> 
        : PdfObjectWrapper<PdfDictionary>, IDictionary<PdfName, TValue>, IDictionary, IBiDictionary<PdfName, TValue>
        where TValue : PdfObjectWrapper
    {
        private IEntryWrapper<TValue> valueWrapper;

        protected Dictionary(PdfDocument context) 
            : this(context, EntryWrapper<TValue>.Default)
        { }

        protected Dictionary(PdfDocument context, IEntryWrapper<TValue> wrapper) 
            : base(context, new PdfDictionary())
        {
            valueWrapper = wrapper;
        }

        protected Dictionary(PdfDocument context, PdfDictionary dataObject) 
            : this(context, dataObject, EntryWrapper<TValue>.Default)
        { }

        protected Dictionary(PdfDocument context, PdfDictionary dataObject, IEntryWrapper<TValue> wrapper) : base(context, dataObject)
        {
            valueWrapper = wrapper;
        }

        public Dictionary(PdfDirectObject baseObject) : this(baseObject, EntryWrapper<TValue>.Default)
        { }

        public Dictionary(PdfDirectObject baseObject, IEntryWrapper<TValue> wrapper) : base(baseObject)
        {
            valueWrapper = wrapper;
        }

        public ICollection<PdfName> Keys => BaseDataObject.Keys;

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> values;
                {
                    // Get the low-level objects!
                    ICollection<PdfDirectObject> valueObjects = BaseDataObject.Values;
                    // Populating the high-level collection...
                    values = new List<TValue>(valueObjects.Count);
                    foreach (PdfDirectObject valueObject in valueObjects)
                    { values.Add(valueWrapper.Wrap(valueObject)); }
                }
                return values;
            }
        }

        public virtual int Count => BaseDataObject.Count;

        public bool IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        ICollection IDictionary.Keys => (ICollection)Keys;

        ICollection IDictionary.Values => (ICollection)Values;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => BaseDataObject;

        public object GetKey(object value) => value is TValue tValue ? GetKey(tValue) : default(PdfName);

        ///Gets the key associated to a given value.
        public PdfName GetKey(TValue value) => BaseDataObject.GetKey(value.BaseObject);

        public virtual void Add(PdfName key, TValue value) => BaseDataObject.Add(key, value.BaseObject);

        public virtual bool ContainsKey(PdfName key) => BaseDataObject.ContainsKey(key);

        public virtual bool Remove(PdfName key) => BaseDataObject.Remove(key);

        public virtual TValue this[PdfName key]
        {
            get => valueWrapper.Wrap(BaseDataObject[key]);
            set => BaseDataObject[key] = value.BaseObject;
        }

        object IDictionary.this[object key]
        {
            get => this[(PdfName)key];
            set => this[(PdfName)key] = (TValue)value;
        }

        object IBiDictionary.this[object key]
        {
            get => this[(PdfName)key];
            set => this[(PdfName)key] = (TValue)value;
        }

        public override Metadata Metadata
        {
            get => Dictionary is PdfDictionary dictionary ? Metadata.Wrap(dictionary.Get<PdfStream>(PdfName.Metadata)?.Reference) : null;
            set => base.Metadata = value;
        }

        public bool TryGetValue(PdfName key, out TValue value) => BaseDataObject.TryGetValue(key, out var bValue)
            ? (value = valueWrapper.Wrap(bValue)) != null
            : (value = default) == null;

        void ICollection<KeyValuePair<PdfName, TValue>>.Add(KeyValuePair<PdfName, TValue> entry) => Add(entry.Key, entry.Value);

        public virtual void Clear() => BaseDataObject.Clear();

        bool ICollection<KeyValuePair<PdfName, TValue>>.Contains(KeyValuePair<PdfName, TValue> entry) => entry.Value.BaseObject.Equals(BaseDataObject[entry.Key]);

        public void CopyTo(KeyValuePair<PdfName, TValue>[] entries, int index)
        {
            foreach (var entry in this)
            {
                entries[index++] = entry;
            }
        }
        
        public bool Remove(KeyValuePair<PdfName, TValue> entry) => BaseDataObject.Remove(new KeyValuePair<PdfName, PdfDirectObject>(entry.Key, entry.Value.BaseObject));

        public DictionaryEnurator GetEnumerator() => new DictionaryEnurator(this);

        IEnumerator<KeyValuePair<PdfName, TValue>> IEnumerable<KeyValuePair<PdfName, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => GetEnumerator();

        void IDictionary.Add(object key, object value) => Add((PdfName)key, (TValue)value);

        bool IDictionary.Contains(object key) => ContainsKey((PdfName)key);

        void IDictionary.Remove(object key) => Remove((PdfName)key);

        void ICollection.CopyTo(Array array, int index)
        {
            int lIndex = index;
            foreach (var entry in this)
            {
                array.SetValue(entry, lIndex++);
            }
        }

        public struct DictionaryEnurator : IEnumerator<KeyValuePair<PdfName, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
        {
            private PdfName key;
            private TValue value;
            private IEntryWrapper<TValue> valueWrapper;
            private Dictionary<PdfName, PdfDirectObject>.Enumerator enumer;

            internal DictionaryEnurator(Dictionary<TValue> dictionary)
            {
                key = null;
                value = null;
                valueWrapper = dictionary.valueWrapper;
                enumer = dictionary.BaseDataObject.GetEnumerator();
            }

            public DictionaryEntry Entry { get => new DictionaryEntry(key, value); }

            public object Key => Key;

            public object Value => Value;

            public KeyValuePair<PdfName, TValue> Current => new(key, value);

            object IEnumerator.Current => Entry;

            public bool MoveNext()
            {
                if (!enumer.MoveNext())
                {
                    key = null;
                    value = null;
                    return false;
                }
                var dictEntry = enumer.Current;
                if (PdfName.Metadata.Equals(dictEntry.Key))
                    return MoveNext();
                key = dictEntry.Key;
                value = valueWrapper.Wrap(dictEntry.Value);
                return true;
            }

            public void Reset() => ((IEnumerator)enumer).Reset();

            public void Dispose() => enumer.Dispose();

        }
    }
}