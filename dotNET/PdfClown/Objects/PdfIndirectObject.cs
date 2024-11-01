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

using System;
using System.Text;

namespace PdfClown.Objects
{
    ///<summary>PDF indirect object [PDF:1.6:3.2.9].</summary>
    public class PdfIndirectObject : PdfObject, IPdfIndirectObject
    {
        private static readonly byte[] BeginIndirectObjectChunk = BaseEncoding.Pdf.Encode(Symbol.Space + Keyword.BeginIndirectObject + Symbol.LineFeed);
        private static readonly byte[] EndIndirectObjectChunk = BaseEncoding.Pdf.Encode(Symbol.LineFeed + Keyword.EndIndirectObject + Symbol.LineFeed);

        private PdfDataObject dataObject;
        private PdfFile file;
        private PdfReference reference;
        private XRefEntry xrefEntry;
        private PdfObjectStatus status;


        /// <param name="file">Associated file.</param>
        /// <param name="dataObject">
        ///   <para>Data object associated to the indirect object. It MUST be</para>
        ///   <list type="bullet">
        ///     <item><code>null</code>, if the indirect object is original or free.</item>
        ///     <item>NOT <code>null</code>, if the indirect object is new and in-use.</item>
        ///   </list>
        /// </param>
        /// <param name="xrefEntry">Cross-reference entry associated to the indirect object. If the
        ///   indirect object is new, its offset field MUST be set to 0.</param>
        internal PdfIndirectObject(PdfFile file, PdfDataObject dataObject, XRefEntry xrefEntry)
            : base(PdfObjectStatus.Updateable)
        {
            this.file = file;
            this.dataObject = Include(dataObject);
            this.xrefEntry = xrefEntry;

            IsOriginal = (xrefEntry.Offset >= 0);
            reference = new PdfReference(this);
        }

        public override PdfObject Accept(IVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public override PdfDataObject Resolve() => DataObject;

        /// <summary>Adds the <see cref="DataObject">data object</see> to the specified object stream
        /// [PDF:1.6:3.4.6].</summary>
        /// <param name="objectStream">Target object stream.</param>
        public void Compress(ObjectStream objectStream)
        {
            // Remove from previous object stream!
            Uncompress();

            if (objectStream != null
               && IsCompressible())
            {
                // Add to the object stream!
                objectStream[xrefEntry.Number] = DataObject;
                // Update its xref entry!
                xrefEntry.Usage = XRefEntry.UsageEnum.InUseCompressed;
                xrefEntry.StreamNumber = objectStream.Reference.ObjectNumber;
                xrefEntry.Offset = XRefEntry.UndefinedOffset; // Internal object index unknown (to set on object stream serialization -- see ObjectStream).
            }
        }

        public override PdfIndirectObject Container => this;

        public override PdfFile File => file;

        // NOTE: As indirect objects are root objects, no parent can be associated.
        public override PdfObject Parent
        {
            get => null;
            internal set { }
        }

        public override PdfObjectStatus Status
        {
            get => status;
            protected internal set => status = value;
        }

        public override bool Updated
        {
            get => base.Updated;
            protected internal set
            {
                if (value && IsOriginal)
                {
                    // NOTE: It's expected that DropOriginal() is invoked by IndirectObjects indexer;
                    // such an action is delegated because clients may invoke directly the indexer skipping
                    // this method.
                    file.IndirectObjects.Update(this);
                }
                base.Updated = value;
            }
        }

        public override int GetHashCode() => reference.GetHashCode();

        /// <summary>Gets whether this object is compressed within an object stream [PDF:1.6:3.4.6].
        /// </summary>
        public bool IsCompressed() => xrefEntry.Usage == XRefEntry.UsageEnum.InUseCompressed;

        /// <summary>Gets whether this object can be compressed within an object stream [PDF:1.6:3.4.6].
        /// </summary>
        public bool IsCompressible()
        {
            return !IsCompressed()
              && IsInUse()
              && !(DataObject is PdfStream
                || dataObject is PdfInteger)
              && Reference.GenerationNumber == 0;
        }

        /// <summary>Gets whether this object contains a data object.</summary>
        public bool IsInUse() => (xrefEntry.Usage == XRefEntry.UsageEnum.InUse);

        /// <summary>Gets whether this object comes intact from an existing file.</summary>
        public bool IsOriginal
        {
            get => (Status & PdfObjectStatus.Original) != 0;
            internal set => Status = value ? (Status | PdfObjectStatus.Original) : (Status & ~PdfObjectStatus.Original);
        }

        public override PdfObject Swap(PdfObject other)
        {
            PdfIndirectObject otherObject = (PdfIndirectObject)other;
            PdfDataObject otherDataObject = otherObject.dataObject;
            // Update the other!
            otherObject.DataObject = dataObject;
            // Update this one!
            this.DataObject = otherDataObject;
            return this;
        }

        /// <summary>Removes the <see cref="DataObject">data object</see> from its object stream [PDF:1.6:3.4.6].</summary>
        public void Uncompress()
        {
            if (!IsCompressed())
                return;

            // Remove from its object stream!
            var oldObjectStream = (ObjectStream)file.IndirectObjects[xrefEntry.StreamNumber].DataObject;
            oldObjectStream.Remove(xrefEntry.Number);
            // Update its xref entry!
            xrefEntry.Usage = XRefEntry.UsageEnum.InUse;
            xrefEntry.StreamNumber = XRefEntry.UndefinedStreamNumber; // No object stream.
            xrefEntry.Offset = XRefEntry.UndefinedOffset; // Offset unknown (to set on file serialization -- see CompressedWriter).
        }

        public override void WriteTo(IOutputStream stream, PdfFile context)
        {
            // Header.
            stream.Write(reference.Id); stream.Write(BeginIndirectObjectChunk);
            // Body.
            DataObject.WriteTo(stream, context);
            // Tail.
            stream.Write(EndIndirectObjectChunk);
        }

        public XRefEntry XrefEntry => xrefEntry;

        public PdfDataObject DataObject
        {
            get
            {
                if (dataObject == null)
                {
                    switch (xrefEntry.Usage)
                    {
                        // Free entry (no data object at all).
                        case XRefEntry.UsageEnum.Free:
                            break;
                        // In-use entry (late-bound data object).
                        case XRefEntry.UsageEnum.InUse:
                            {
                                // Get the indirect data object!
                                dataObject = Include(file.Reader.Parser.ParsePdfObjectWithLock(xrefEntry));
                                break;
                            }
                        case XRefEntry.UsageEnum.InUseCompressed:
                            {
                                // Get the object stream where its data object is stored!
                                var objectStream = (ObjectStream)file.IndirectObjects[xrefEntry.StreamNumber].DataObject;
                                // Get the indirect data object!
                                dataObject = Include(objectStream[xrefEntry.Number]);
                                break;
                            }
                    }
                }
                return dataObject;
            }
            set
            {
                if (xrefEntry.Generation == XRefEntry.GenerationUnreusable)
                    throw new Exception("Unreusable entry.");

                Exclude(dataObject);
                dataObject = Include(value);
                xrefEntry.Usage = XRefEntry.UsageEnum.InUse;
                Update();
            }
        }

        public override bool Delete()
        {
            if (file != null)
            {
                // NOTE: It's expected that DropFile() is invoked by IndirectObjects.Remove() method;
                // such an action is delegated because clients may invoke directly Remove() method,
                // skipping this method.
                file.IndirectObjects.RemoveAt(xrefEntry.Number);
            }
            return true;
        }

        public override PdfIndirectObject IndirectObject => this;

        public override PdfReference Reference => reference;

        public override IPdfObjectWrapper Wrapper
        {
            get => DataObject?.Wrapper;
            internal set => DataObject.Wrapper = value;
        }

        public override IPdfObjectWrapper Wrapper2
        {
            get => DataObject?.Wrapper2;
            internal set => DataObject.Wrapper2 = value;
        }

        public override IPdfObjectWrapper Wrapper3
        {
            get => DataObject?.Wrapper3;
            internal set => DataObject.Wrapper3 = value;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            {
                // Header.
                buffer.Append(reference.Id).Append(" obj").Append(Symbol.LineFeed);
                // Body.
                buffer.Append(DataObject);
            }
            return buffer.ToString();
        }

        protected internal override bool Virtual
        {
            get => base.Virtual;
            set
            {
                if (Virtual && !value)
                {
                    //NOTE: When a virtual indirect object becomes concrete it must be registered.
                    file.IndirectObjects.AddVirtual(this);
                    base.Virtual = false;
                    Reference.Update();
                }
                else
                { base.Virtual = value; }
                dataObject.Virtual = Virtual;
            }
        }

        internal void DropFile()
        {
            Uncompress();
            file = null;
        }
    }
}