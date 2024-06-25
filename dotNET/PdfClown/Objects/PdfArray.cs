/*
  Copyright 2006-2013 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Documents.Contents;
using PdfClown.Tokens;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using text = System.Text;

namespace PdfClown.Objects
{
    /// <summary>PDF array object, that is a one-dimensional collection of (possibly-heterogeneous)
    /// objects arranged sequentially [PDF:1.7:3.2.5].</summary>
    public sealed class PdfArray : PdfWrapableDirectObject, IList<PdfDirectObject>
    {
        private static readonly byte[] BeginArrayChunk = Encoding.Pdf.Encode(Keyword.BeginArray);
        private static readonly byte[] EndArrayChunk = Encoding.Pdf.Encode(Keyword.EndArray);

        public static bool SequenceEquals(PdfArray oldValue, PdfArray newValue)
        {
            if (oldValue == newValue)
                return true;
            return oldValue != null
                && newValue != null
                && oldValue.SequenceEqual(newValue);
        }

        internal List<PdfDirectObject> items;

        private PdfObject parent;
        private PdfObjectStatus status;
        private ContentWrapper contentsWrapper;

        public PdfArray() : this(10)
        { }

        public PdfArray(int capacity) : base(PdfObjectStatus.Updateable)
        {
            items = new List<PdfDirectObject>(capacity);
        }

        public PdfArray(params PdfDirectObject[] items)
            : this(items.Length)
        {
            Updateable = false;
            this.AddRange(items);
            Updateable = true;
        }

        public PdfArray(IEnumerable<PdfDirectObject> items)
            : this(items.Count())
        {
            Updateable = false;
            this.AddRange(items);
            Updateable = true;
        }

        public PdfArray(ICollection<int> items)
            : this(items.Count)
        {
            Updateable = false;
            this.AddRangeDirect(items.Select(x => PdfInteger.Get(x)));
            Updateable = true;
        }

        public PdfArray(ICollection<float> items)
            : this(items.Count)
        {
            Updateable = false;
            this.AddRangeDirect(items.Select(x => PdfReal.Get(x)));
            Updateable = true;
        }

        public PdfArray(ICollection<double> items)
            : this(items.Count)
        {
            Updateable = false;
            this.AddRangeDirect(items.Select(x => PdfReal.Get(x)));
            Updateable = true;
        }

        public PdfArray(ICollection<byte[]> source)
            : this(source.Count)
        {
            Updateable = false;
            AddRangeDirect(source.Select(p => PdfString.Get(p)));
            Updateable = true;
        }

        public PdfArray(ICollection<string> source)
            : this(source.Count)
        {
            Updateable = false;
            AddRangeDirect(source.Select(p => PdfString.Get(p)));
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

        public override ContentWrapper ContentsWrapper
        {
            get => contentsWrapper;
            internal set => contentsWrapper = value;
        }

        public override PdfObject Accept(IVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public override int CompareTo(PdfDirectObject obj)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object @object)
        {
            return base.Equals(@object)
              || (@object != null
                && @object.GetType().Equals(GetType())
                && ((PdfArray)@object).items.Equals(items));
        }

        public IPdfNumber GetNumber(int index) => (IPdfNumber)Resolve(index);

        public float GetFloat(int index, float def = 0) => ((IPdfNumber)Resolve(index))?.FloatValue ?? def;

        public float? GetNFloat(int index, float? def = null) => (Resolve(index) as IPdfNumber)?.FloatValue ?? def;

        public double GetDouble(int index, double def = 0) => ((IPdfNumber)Resolve(index))?.RawValue ?? def;

        public double? GetNDouble(int index, double? def = null) => (Resolve(index) as IPdfNumber)?.RawValue ?? def;

        public int GetInt(int index, int def = 0) => ((IPdfNumber)Resolve(index))?.IntValue ?? def;

        public int? GetNInt(int index, int? def = null) => (Resolve(index) as IPdfNumber)?.IntValue ?? def;

        public bool GetBool(int index, bool def = false) => ((PdfBoolean)Resolve(index))?.RawValue ?? def;

        public bool? GetNBool(int index, bool? def = null) => (Resolve(index) as PdfBoolean)?.RawValue ?? def;

        public string GetString(int index, string def = null) => ((IPdfString)Resolve(index))?.StringValue ?? def;

        public void Set(int index, bool? value) => Set(index, PdfBoolean.Get(value));

        public void Set(int index, float? value) => Set(index, PdfReal.Get(value));

        public void Set(int index, double? value) => Set(index, PdfReal.Get(value));

        public void Set(int index, int? value) => Set(index, PdfInteger.Get(value));

        public void Set(int index, long? value) => Set(index, PdfInteger.Get(value));

        public void Set(int index, bool value) => Set(index, PdfBoolean.Get(value));

        public void Set(int index, float value) => Set(index, PdfReal.Get(value));

        public void Set(int index, double value) => Set(index, PdfReal.Get(value));

        public void Set(int index, int value) => Set(index, PdfInteger.Get(value));

        public void Set(int index, long value) => Set(index, PdfInteger.Get(value));

        public void SetText(int index, string value) => Set(index, PdfTextString.Get(value));

        public void SetName(int index, string value) => Set(index, PdfName.Get(value));

        public void Add(bool? value) => Add(PdfBoolean.Get(value));

        public void Add(float? value) => Add(PdfReal.Get(value));

        public void Add(double? value) => Add(PdfReal.Get(value));

        public void Add(int? value) => Add(PdfInteger.Get(value));

        public void Add(long? value) => Add(PdfInteger.Get(value));

        public void Add(DateTime? value) => Add(PdfDate.Get(value));

        public void Add(bool value) => Add(PdfBoolean.Get(value));

        public void Add(float value) => Add(PdfReal.Get(value));

        public void Add(double value) => Add(PdfReal.Get(value));

        public void Add(int value) => Add(PdfInteger.Get(value));

        public void Add(long value) => Add(PdfInteger.Get(value));

        public void Add(string value) => Add(PdfTextString.Get(value));

        public void Add(byte[] value) => Add(PdfTextString.Get(value));

        public T Get<T>(int index) where T : PdfDataObject
        {
            var value = items[index];
            return value is T typedValue ? typedValue
                : value?.Resolve() is T resolved ? resolved : default;
        }

        public T Get<T>(int index, T defaultValue) where T : PdfDataObject
        {
            var value = items[index];
            return value is T typedValue ? typedValue
                : value?.Resolve() is T resolved ? resolved : defaultValue;
        }

        /// <summary>Gets the value corresponding to the given index, forcing its instantiation as a direct
        /// object in case of missing entry.</summary>
        /// <param name="index">Index of the item to return.</param>
        /// <param name="itemClass">Class to use for instantiating the item in case of missing entry.</param>
        public PdfDirectObject GetOrCreate<T>(int index) where T : PdfDataObject, new() => GetOrCreate<T>(index, true);

        /// <summary>Gets the value corresponding to the given index, forcing its instantiation in case
        /// of missing entry.</summary>
        /// <param name="index">Index of the item to return.</param>
        /// <param name="direct">Whether the item has to be instantiated directly within its container
        /// instead of being referenced through an indirect object.</param>
        public PdfDirectObject GetOrCreate<T>(int index, bool direct) where T : PdfDataObject, new()
        {
            PdfDirectObject item;
            if (index == Count
              || (item = this[index]) == null
              || !item.Resolve().GetType().Equals(typeof(T)))
            {
                //NOTE: The null-object placeholder MUST NOT perturb the existing structure; therefore:
                //   - it MUST be marked as virtual in order not to unnecessarily serialize it;
                //   - it MUST be put into this array without affecting its update status.
                try
                {
                    item = (PdfDirectObject)Include(direct
                      ? (PdfDataObject)new T()
                      : new PdfIndirectObject(File, new T(), new XRefEntry(0, 0)).Reference);
                    if (index == Count)
                    { items.Add(item); }
                    else if (item == null)
                    { items[index] = item; }
                    else
                    { items.Insert(index, item); }
                    item.Virtual = true;
                }
                catch (Exception e)
                { throw new Exception(typeof(T).Name + " failed to instantiate.", e); }
            }
            return item;
        }

        public override int GetHashCode() => items.GetHashCode();

        /// <summary>Gets the dereferenced value corresponding to the given index.</summary>
        /// <remarks>This method takes care to resolve the value returned by
        /// <see cref="this[int]">this[int]</see>.</remarks>
        /// <param name="index">Index of the item to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PdfDataObject Resolve(int index) => items[index]?.Resolve();

        /// <summary>Gets the dereferenced value corresponding to the given index, forcing its
        /// instantiation in case of missing entry.</summary>
        /// <remarks>This method takes care to resolve the value returned by
        /// <see cref="GetOrCreate<T>">Get<T></see>.</remarks>
        /// <param name="index">Index of the item to return.</param>
        public T Resolve<T>(int index) where T : PdfDataObject, new()
        {
            return (T)Resolve(GetOrCreate<T>(index));
        }

        public override PdfObject Swap(PdfObject other)
        {
            PdfArray otherArray = (PdfArray)other;
            var otherItems = otherArray.items;
            // Update the other!
            otherArray.items = items;
            otherArray.Update();
            // Update this one!
            items = otherItems;
            Update();
            return this;
        }

        public override string ToString()
        {
            var buffer = new text::StringBuilder();
            {
                // Begin.
                buffer.Append("[ ");
                // Elements.
                foreach (PdfDirectObject item in items)
                { buffer.Append(PdfDirectObject.ToString(item)).Append(" "); }
                // End.
                buffer.Append("]");
            }
            return buffer.ToString();
        }

        public override void WriteTo(IOutputStream stream, PdfFile context)
        {
            // Begin.
            stream.Write(BeginArrayChunk);
            // Elements.
            foreach (PdfDirectObject item in items)
            {
                if (item != null && item.Virtual)
                    continue;

                PdfDirectObject.WriteTo(stream, context, item); stream.Write(Chunk.Space);
            }
            // End.
            stream.Write(EndArrayChunk);
        }

        public int IndexOf(PdfDirectObject item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, PdfDirectObject item)
        {
            items.Insert(index, (PdfDirectObject)Include(item));
            Update();
        }

        public void RemoveAt(int index)
        {
            PdfDirectObject oldItem = items[index];
            items.RemoveAt(index);
            Exclude(oldItem);
            Update();
        }

        internal void SetDirect(int index, PdfDirectObject value)
        {
            PdfDirectObject oldItem = items[index];
            items[index] = value;
            Exclude(oldItem);
            Update();
        }

        public void Set<T>(int index, T value)
             where T : PdfDirectObject, IPdfSimpleObject
        {
            PdfDirectObject oldItem = items[index];
            items[index] = value;
            Exclude(oldItem);
            Update();
        }

        public PdfDirectObject this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => items[index];
            set
            {
                PdfDirectObject oldItem = items[index];
                items[index] = (PdfDirectObject)Include(value);
                Exclude(oldItem);
                Update();
            }
        }

        internal void AddDirect(PdfDirectObject item)
        {
            items.Add(item);
            Update();
        }

        public void Add<T>(T item) where T : PdfDirectObject, IPdfSimpleObject
        {
            items.Add(item);
            Update();
        }

        public void Add(PdfDirectObject item)
        {
            items.Add((PdfDirectObject)Include(item));
            Update();
        }

        internal void AddRangeDirect(IEnumerable<PdfDirectObject> source)
        {
            items.AddRange(source);
            Update();
        }

        public void AddRange(IEnumerable<PdfDirectObject> source)
        {
            items.AddRange(source.Select(x => (PdfDirectObject)Include(x)));
            Update();
        }

        public void Clear()
        {
            while (items.Count > 0)
            { RemoveAt(0); }
        }

        public bool Contains(PdfDirectObject item) => items.Contains(item);

        public void CopyTo(PdfDirectObject[] items, int index) => this.items.CopyTo(items, index);

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public int Capacity { get => items.Capacity; }

        public bool Remove(PdfDirectObject item)
        {
            if (!items.Remove(item))
                return false;

            Exclude((PdfDirectObject)item);
            Update();
            return true;
        }

        public List<PdfDirectObject>.Enumerator GetEnumerator() => items.GetEnumerator();

        IEnumerator<PdfDirectObject> IEnumerable<PdfDirectObject>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int[] ToIntArray()
        {
            var newArray = new int[Count];
            {
                for (int i = 0; i < Count; i++)
                {
                    newArray[i] = GetInt(i);
                }
            }
            return newArray;
        }

        public float[] ToFloatArray()
        {
            var newArray = new float[Count];
            {
                for (int i = 0; i < Count; i++)
                {
                    newArray[i] = GetFloat(i);
                }
            }
            return newArray;
        }

        public string[] ToStringArray()
        {
            var newArray = new string[Count];
            {
                for (int i = 0; i < Count; i++)
                {
                    newArray[i] = GetString(i);
                }
            }
            return newArray;
        }

        public List<string> ToStringList()
        {
            var newArray = new List<string>(Count);
            {
                for (int i = 0; i < Count; i++)
                {
                    newArray.Add(GetString(i));
                }
            }
            return newArray;
        }

        public PdfDirectObject TryGet(int i)
        {
            return i < Count ? items[i] : null;
        }
    }
}