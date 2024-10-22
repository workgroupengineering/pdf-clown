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

using PdfClown.Bytes;
using PdfClown.Tokens;
using PdfClown.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfClown.Objects
{
    /// <summary>PDF dictionary object [PDF:1.6:3.2.6].</summary>
    public sealed class PdfDictionary : PdfWrapableDirectObject, IDictionary<PdfName, PdfDirectObject>, IBiDictionary<PdfName, PdfDirectObject>
    {
        private static readonly byte[] BeginDictionaryChunk = BaseEncoding.Pdf.Encode(Keyword.BeginDictionary);
        private static readonly byte[] EndDictionaryChunk = BaseEncoding.Pdf.Encode(Keyword.EndDictionary);

        internal Dictionary<PdfName, PdfDirectObject> entries;

        private PdfObject parent;
        private PdfObjectStatus status;

        /// <summary>Creates a new empty dictionary object with the default initial capacity.</summary>
        public PdfDictionary() : this(4)
        { }

        /// <summary>Creates a new empty dictionary object with the specified initial capacity.</summary>
        /// <param name="capacity">Initial capacity.</param>
        public PdfDictionary(int capacity) : base(PdfObjectStatus.Updateable)
        {
            entries = new Dictionary<PdfName, PdfDirectObject>(capacity);
        }

        /// <summary>Creates a new dictionary object with the specified entries.</summary>
        /// <param name="keys">Entry keys to add to this dictionary.</param>
        /// <param name="values">Entry values to add to this dictionary; their position and number must
        /// match the <code>keys</code> argument.</param>
        public PdfDictionary(PdfName[] keys, PdfDirectObject[] values)
            : this(values.Length)
        {
            Updateable = false;
            for (int index = 0; index < values.Length; index++)
            { this[keys[index]] = values[index]; }
            Updateable = true;
        }

        /// <summary>Creates a new dictionary object with the specified entries.</summary>
        /// <param name="objects">Sequence of key/value-paired objects (where key is a <see
        /// cref="PdfName"/> and value is a <see cref="PdfDirectObject"/>).</param>
        public PdfDictionary(params PdfDirectObject[] objects)
            : this(objects.Length / 2)
        {
            Updateable = false;
            for (int index = 0; index < objects.Length;)
            { this[(PdfName)objects[index++]] = objects[index++]; }
            Updateable = true;
        }

        /// <summary>Creates a new dictionary object with the specified entries.</summary>
        /// <param name="entries">Map whose entries have to be added to this dictionary.</param>
        public PdfDictionary(IDictionary<PdfName, PdfDirectObject> entries)
            : this(entries.Count)
        {
            Updateable = false;
            foreach (KeyValuePair<PdfName, PdfDirectObject> entry in entries)
            { this[entry.Key] = (PdfDirectObject)Include(entry.Value); }
            Updateable = true;
        }

        public override PdfObject Parent
        {
            get => parent;
            internal set => parent = value;
        }

        public override PdfObjectStatus Status
        {
            get => status;
            protected internal set => status = value;
        }

        public Dictionary<PdfName, PdfDirectObject>.KeyCollection Keys => entries.Keys;

        ICollection<PdfName> IDictionary<PdfName, PdfDirectObject>.Keys => Keys;

        public Dictionary<PdfName, PdfDirectObject>.ValueCollection Values => entries.Values;

        ICollection<PdfDirectObject> IDictionary<PdfName, PdfDirectObject>.Values => Values;

        public int Count => entries.Count;

        public bool IsReadOnly => false;

        public override PdfObject Accept(IVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public override int CompareTo(PdfDirectObject obj)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(PdfName key)
            where T : PdfDataObject
        {
            return entries.TryGetValue(key, out var value)
                ? value is T typedValue ? typedValue : value?.Resolve() as T
                : default;
        }

        public T Get<T>(PdfName key, T deaultValue)
            where T : PdfDataObject
        {
            return entries.TryGetValue(key, out var value)
                ? value is T typedValue ? typedValue : (value?.Resolve() as T) ?? deaultValue
                : deaultValue;
        }

        /// <summary>Gets the value corresponding to the given key, forcing its instantiation as a direct
        /// object in case of missing entry.</summary>
        /// <param name="key">Key whose associated value is to be returned.</param>
        public PdfDirectObject GetOrCreate<T>(PdfName key) where T : PdfDataObject, new()
        {
            return GetOrCreate<T>(key, true);
        }

        /// <summary>Gets the value corresponding to the given key, forcing its instantiation in case of
        /// missing entry.</summary>
        /// <param name="key">Key whose associated value is to be returned.</param>
        /// <param name="direct">Whether the item has to be instantiated directly within its container
        /// instead of being referenced through an indirect object.</param>
        public PdfDirectObject GetOrCreate<T>(PdfName key, bool direct) where T : PdfDataObject, new()
        {
            PdfDirectObject value = this[key];
            if (value == null)
            {
                //NOTE: The null-object placeholder MUST NOT perturb the existing structure; therefore:
                //    - it MUST be marked as virtual in order not to unnecessarily serialize it;
                //    - it MUST be put into this dictionary without affecting its update status.
                try
                {
                    value = (PdfDirectObject)Include(direct
                      ? new T()
                      : new PdfIndirectObject(File, new T(), new XRefEntry(0, 0)).Reference);
                    entries[key] = value;
                    value.Virtual = true;
                }
                catch (Exception e)
                { throw new Exception(typeof(T).Name + " failed to instantiate.", e); }
            }
            return value;
        }

        public override bool Equals(object @object)
        {
            return base.Equals(@object)
              || (@object != null
                && @object.GetType().Equals(GetType())
                && ((PdfDictionary)@object).entries.Equals(entries));
        }

        public override int GetHashCode() => entries.GetHashCode();

        public object GetKey(object value) => value is PdfDirectObject tValue ? GetKey(tValue) : default(PdfName);

        /// <summary>Gets the key associated to the specified value.</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public PdfName GetKey(PdfDirectObject value)
        {
            // NOTE: Current PdfDictionary implementation doesn't support bidirectional maps, to say that
            //  the only currently-available way to retrieve a key from a value is to iterate the whole map
            //  (really poor performance!).
            foreach (KeyValuePair<PdfName, PdfDirectObject> entry in entries)
            {
                if (entry.Value.Equals(value))
                    return entry.Key;
            }
            return null;
        }

        public float GetFloat(PdfName key, float def = 0) => ((IPdfNumber)Resolve(key))?.FloatValue ?? def;

        public float? GetNFloat(PdfName key, float? def = null) => (Resolve(key) as IPdfNumber)?.FloatValue ?? def;

        public double GetDouble(PdfName key, double def = 0) => ((IPdfNumber)Resolve(key))?.DoubleValue ?? def;

        public double? GetNDouble(PdfName key, double? def = null) => (Resolve(key) as IPdfNumber)?.DoubleValue ?? def;

        public int GetInt(PdfName key, int def = 0) => ((IPdfNumber)Resolve(key))?.IntValue ?? def;

        public int? GetNInt(PdfName key, int? def = null) => (Resolve(key) as IPdfNumber)?.IntValue ?? def;

        public IPdfNumber GetNumber(PdfName key) => (IPdfNumber)Resolve(key);

        public bool GetBool(PdfName key, bool def = false) => Get<PdfBoolean>(key)?.RawValue ?? def;

        public bool? GetNBool(PdfName key, bool? def = null) => Get<PdfBoolean>(key)?.RawValue ?? def;

        public DateTime? GetDate(PdfName key) => Get<PdfDate>(key)?.DateValue;

        public DateTime? GetNDate(PdfName key) => Get<PdfDate>(key)?.DateValue ?? null;

        public Memory<byte> GetTextBytes(PdfName key) => ((PdfString)Resolve(key))?.RawValue ?? Memory<byte>.Empty;

        public string GetString(PdfName key, string def = null) => ((IPdfString)Resolve(key))?.StringValue ?? def;

        public void Set(PdfName key, double? value) => Set(key, PdfReal.Get(value));

        public void Set(PdfName key, float? value) => Set(key, PdfReal.Get(value));

        public void Set(PdfName key, int? value) => Set(key, PdfInteger.Get(value));

        public void Set(PdfName key, long? value) => Set(key, PdfInteger.Get(value));

        public void Set(PdfName key, bool? value) => Set(key, PdfBoolean.Get(value));

        public void Set(PdfName key, DateTime? value) => Set(key, PdfDate.Get(value));

        public void Set(PdfName key, double value) => Set(key, PdfReal.Get(value));

        public void Set(PdfName key, float value) => Set(key, PdfReal.Get(value));

        public void Set(PdfName key, int value) => Set(key, PdfInteger.Get(value));

        public void Set(PdfName key, long value) => Set(key, PdfInteger.Get(value));

        public void Set(PdfName key, bool value) => Set(key, PdfBoolean.Get(value));

        public void Set(PdfName key, Memory<byte> data) => Set(key, PdfString.Get(data));

        public void Set(PdfName key, string value) => Set(key, PdfString.Get(value));

        public void SetName(PdfName key, string value) => Set(key, PdfName.Get(value));

        public void SetText(PdfName key, string value) => Set(key, PdfTextString.Get(value));

        public void Add(PdfName key, double? value) => Add(key, PdfReal.Get(value));

        public void Add(PdfName key, float? value) => Add(key, PdfReal.Get(value));

        public void Add(PdfName key, int? value) => Add(key, PdfInteger.Get(value));

        public void Add(PdfName key, long? value) => Add(key, PdfInteger.Get(value));

        public void Add(PdfName key, bool? value) => Add(key, PdfBoolean.Get(value));

        public void Add(PdfName key, DateTime? value) => Add(key, PdfDate.Get(value));

        public void Add(PdfName key, double value) => Add(key, PdfReal.Get(value));

        public void Add(PdfName key, float value) => Add(key, PdfReal.Get(value));

        public void Add(PdfName key, int value) => Add(key, PdfInteger.Get(value));

        public void Add(PdfName key, long value) => Add(key, PdfInteger.Get(value));

        public void Add(PdfName key, bool value) => Add(key, PdfBoolean.Get(value));

        public void Add(PdfName key, Memory<byte> data) => Add(key, PdfString.Get(data));

        public void Add(PdfName key, string value) => Add(key, PdfString.Get(value));

        /// <summary>Gets the dereferenced value corresponding to the given key.</summary>
        /// <remarks>This method takes care to resolve the value returned by <see cref="this[PdfName]">
        /// this[PdfName]</see>.</remarks>
        /// <param name="key">Key whose associated value is to be returned.</param>
        /// <returns>null, if the map contains no mapping for this key.</returns>
        public PdfDataObject Resolve(PdfName key) => this[key]?.Resolve();

        /// <summary>Gets the dereferenced value corresponding to the given key, forcing its instantiation
        /// in case of missing entry.</summary>
        /// <remarks>This method takes care to resolve the value returned by <see cref="GetOrCreate(PdfName)"/>.
        /// </remarks>
        /// <param name="key">Key whose associated value is to be returned.</param>
        /// <returns>null, if the map contains no mapping for this key.</returns>
        public T Resolve<T>(PdfName key) where T : PdfDataObject, new() => (T)Resolve(GetOrCreate<T>(key));

        public override PdfObject Swap(PdfObject other)
        {
            var otherDictionary = (PdfDictionary)other;
            var otherEntries = otherDictionary.entries;
            // Update the other!
            otherDictionary.entries = this.entries;
            otherDictionary.Update();
            // Update this one!
            this.entries = otherEntries;
            this.Update();
            return this;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            {
                // Begin.
                buffer.Append("<< ");
                // Entries.
                foreach (KeyValuePair<PdfName, PdfDirectObject> entry in entries)
                {
                    // Entry...
                    // ...key.
                    buffer.Append(entry.Key.StringValue).Append(" ");
                    // ...value.
                    buffer.Append(PdfDirectObject.ToString(entry.Value)).Append(" ");
                }
                // End.
                buffer.Append(">>");
            }
            return buffer.ToString();
        }

        public override void WriteTo(IOutputStream stream, PdfFile context)
        {
            // Begin.
            stream.Write(BeginDictionaryChunk);
            // Entries.
            foreach (KeyValuePair<PdfName, PdfDirectObject> entry in entries)
            {
                PdfDirectObject value = entry.Value;
                if (value != null && value.Virtual)
                    continue;

                // Entry...
                // ...key.
                entry.Key.WriteTo(stream, context); stream.Write(Chunk.Space);
                // ...value.
                PdfDirectObject.WriteTo(stream, context, value); stream.Write(Chunk.Space);
            }
            // End.
            stream.Write(EndDictionaryChunk);
        }

        public void Add<T>(PdfName key, T value)
            where T : PdfDirectObject, IPdfSimpleObject
        {
            entries.Add(key, value);
            Update();
        }

        public void Add(PdfName key, PdfDirectObject value)
        {
            entries.Add(key, (PdfDirectObject)Include(value));
            Update();
        }

        public bool ContainsKey(PdfName key)
        {
            return entries.ContainsKey(key);
        }

        public bool Remove(PdfName key)
        {
            if (entries.Remove(key, out var oldValue))
            {
                Exclude(oldValue);
                Update();
                return true;
            }
            return false;
        }

        public void Set<T>(PdfName key, T value)
             where T : PdfDirectObject, IPdfSimpleObject
        {
            if (value == null)
            { Remove(key); }
            else
            {
                PdfDirectObject oldValue = this[key];
                entries[key] = value;
                Exclude(oldValue);
                Update();
            }
        }

        public PdfDirectObject this[PdfName key]
        {
            get => entries.TryGetValue(key, out var value) ? value : null;
            set
            {
                if (value == null)
                { Remove(key); }
                else
                {
                    PdfDirectObject oldValue = this[key];
                    entries[key] = (PdfDirectObject)Include(value);
                    Exclude(oldValue);
                    Update();
                }
            }
        }

        public bool TryGetValue(PdfName key, out PdfDirectObject value) => entries.TryGetValue(key, out value);

        void ICollection<KeyValuePair<PdfName, PdfDirectObject>>.Add(KeyValuePair<PdfName, PdfDirectObject> entry) => Add(entry.Key, entry.Value);

        public void Clear()
        {
            foreach (PdfName key in entries.Keys.ToList())
            {
                Remove(key);
            }
        }

        bool ICollection<KeyValuePair<PdfName, PdfDirectObject>>.Contains(KeyValuePair<PdfName, PdfDirectObject> entry)
        {
            return entries.TryGetValue(entry.Key, out var value) && value.Equals(entry.Value);
        }

        public void CopyTo(KeyValuePair<PdfName, PdfDirectObject>[] entries, int index)
        {
            foreach (var entry in this)
            {
                entries[index++] = entry;
            }
        }

        public bool Remove(KeyValuePair<PdfName, PdfDirectObject> entry)
        {
            if (entry.Value.Equals(this[entry.Key]))
                return Remove(entry.Key);
            else
                return false;
        }

        public Dictionary<PdfName, PdfDirectObject>.Enumerator GetEnumerator() => entries.GetEnumerator();

        IEnumerator<KeyValuePair<PdfName, PdfDirectObject>> IEnumerable<KeyValuePair<PdfName, PdfDirectObject>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}