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
using PdfClown.Objects;
using PdfClown.Util;

using System;
using System.Collections;
using System.Collections.Generic;

namespace PdfClown.Documents.Interaction.Navigation
{
    /// <summary>Article bead [PDF:1.7:8.3.2].</summary>
    [PDF(VersionEnum.PDF11)]
    public sealed class ArticleElements : PdfObjectWrapper2<PdfDictionary>, IList<ArticleElement>
    {
        private sealed class ElementCounter : ElementEvaluator
        {
            public int Count => index + 1;
        }

        private class ElementEvaluator : IPredicate
        {
            // Current position.
            protected int index = -1;

            public virtual bool Evaluate(object @object)
            {
                index++;
                return false;
            }
        }

        private sealed class ElementGetter : ElementEvaluator
        {
            private PdfDictionary bead;
            private readonly int beadIndex;

            public ElementGetter(int beadIndex)
            { this.beadIndex = beadIndex; }

            public override bool Evaluate(object @object)
            {
                base.Evaluate(@object);
                if (index == beadIndex)
                {
                    bead = (PdfDictionary)@object;
                    return true;
                }
                return false;
            }

            public PdfDictionary Bead => bead;
        }

        private sealed class ElementIndexer : ElementEvaluator
        {
            private readonly PdfDictionary searchedBead;

            public ElementIndexer(PdfDictionary searchedBead)
            { this.searchedBead = searchedBead; }

            public override bool Evaluate(object @object)
            {
                base.Evaluate(@object);
                return @object.Equals(searchedBead);
            }

            public int Index => index;
        }

        private sealed class ElementListBuilder : ElementEvaluator
        {
            public IList<ArticleElement> elements = new List<ArticleElement>();

            public override bool Evaluate(object @object)
            {
                elements.Add(Wrap<ArticleElement>((PdfDirectObject)@object));
                return false;
            }

            public IList<ArticleElement> Elements => elements;
        }

        private class Enumerator : IEnumerator<ArticleElement>
        {
            private PdfDirectObject currentObject;
            private readonly PdfDirectObject firstObject;
            private PdfDirectObject nextObject;

            internal Enumerator(ArticleElements elements)
            { nextObject = firstObject = elements.BaseDataObject[PdfName.F]; }

            ArticleElement IEnumerator<ArticleElement>.Current => Wrap<ArticleElement>(currentObject);

            public object Current => ((IEnumerator<ArticleElement>)this).Current;

            public bool MoveNext()
            {
                if (nextObject == null)
                    return false;

                currentObject = nextObject;
                nextObject = ((PdfDictionary)currentObject.Resolve())[PdfName.N];
                if (nextObject == firstObject) // Looping back.
                { nextObject = null; }
                return true;
            }

            public void Reset()
            { throw new NotSupportedException(); }

            public void Dispose()
            { }
        }

        public ArticleElements(PdfDirectObject baseObject) : base(baseObject)
        { }

        public int IndexOf(ArticleElement @object)
        {
            if (@object == null)
                return -1; // NOTE: By definition, no bead can be null.

            ElementIndexer indexer = new ElementIndexer(@object.BaseDataObject);
            Iterate(indexer);
            return indexer.Index;
        }

        public void Insert(int index, ArticleElement @object)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException();

            ElementGetter getter = new ElementGetter(index);
            Iterate(getter);
            PdfDictionary bead = getter.Bead;
            if (bead == null)
            { Add(@object); }
            else
            { Link(@object.BaseDataObject, bead); }
        }

        public void RemoveAt(int index) => Unlink(this[index].BaseDataObject);

        public ArticleElement this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException();

                ElementGetter getter = new ElementGetter(index);
                Iterate(getter);
                PdfDictionary bead = getter.Bead;
                if (bead == null)
                    throw new ArgumentOutOfRangeException();

                return Wrap<ArticleElement>(bead.Reference);
            }
            set => throw new NotImplementedException();
        }

        public void Add(ArticleElement @object)
        {
            PdfDictionary itemBead = @object.BaseDataObject;
            PdfDictionary firstBead = FirstBead;
            if (firstBead != null) // Non-empty list.
            { Link(itemBead, firstBead); }
            else // Empty list.
            {
                FirstBead = itemBead;
                Link(itemBead, itemBead);
            }
        }

        public void Clear()
        { throw new NotImplementedException(); }

        public bool Contains(ArticleElement @object) => IndexOf(@object) >= 0;

        public void CopyTo(ArticleElement[] objects, int index)
        { throw new NotImplementedException(); }

        public int Count
        {
            get
            {
                ElementCounter counter = new ElementCounter();
                Iterate(counter);
                return counter.Count;
            }
        }

        public bool IsReadOnly => false;

        public bool Remove(ArticleElement @object)
        {
            if (!Contains(@object))
                return false;

            Unlink(@object.BaseDataObject);
            return true;
        }

        public IEnumerator<ArticleElement> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ArticleElement>)this).GetEnumerator();

        private PdfDictionary FirstBead
        {
            get => BaseDataObject.Get<PdfDictionary>(PdfName.F);
            set
            {
                PdfDictionary oldValue = FirstBead;
                BaseDataObject[PdfName.F] = PdfObject.Unresolve(value);
                if (value != null)
                { value[PdfName.T] = BaseObject; }
                if (oldValue != null)
                { oldValue.Remove(PdfName.T); }
            }
        }

        private void Iterate(IPredicate predicate)
        {
            PdfDictionary firstBead = FirstBead;
            PdfDictionary bead = firstBead;
            while (bead != null)
            {
                if (predicate.Evaluate(bead))
                    break;

                bead = bead.Get<PdfDictionary>(PdfName.N);
                if (bead == firstBead)
                    break;
            }
        }

        /**
          <summary>Links the given item.</summary>
        */
        private void Link(PdfDictionary item, PdfDictionary next)
        {
            var previous = next.Get<PdfDictionary>(PdfName.V);
            if (previous == null)
            { previous = next; }

            item[PdfName.N] = next.Reference;
            next[PdfName.V] = item.Reference;
            if (previous != item)
            {
                item[PdfName.V] = previous.Reference;
                previous[PdfName.N] = item.Reference;
            }
        }

        /**
          <summary>Unlinks the given item.</summary>
          <remarks>It assumes the item is contained in this list.</remarks>
        */
        private void Unlink(PdfDictionary item)
        {
            var prevBead = item.Get<PdfDictionary>(PdfName.V);
            item.Remove(PdfName.V);
            var nextBead = item.Get<PdfDictionary>(PdfName.N);
            item.Remove(PdfName.N);
            if (prevBead != item) // Still some elements.
            {
                prevBead[PdfName.N] = nextBead.Reference;
                nextBead[PdfName.V] = prevBead.Reference;
                if (item == FirstBead)
                { FirstBead = nextBead; }
            }
            else // No more elements.
            { FirstBead = null; }
        }
    }
}