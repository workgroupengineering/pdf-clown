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
using PdfClown.Documents.Files;
using PdfClown.Tokens;

using System;

namespace PdfClown.Objects
{
    /// <summary>PDF stream object [PDF:1.6:3.2.7].</summary>
    public class PdfStream : PdfDataObject, IFileResource
    {
        private static readonly byte[] BeginStreamBodyChunk = Encoding.Pdf.Encode(Symbol.LineFeed + Keyword.BeginStream + Symbol.LineFeed);
        private static readonly byte[] EndStreamBodyChunk = Encoding.Pdf.Encode(Symbol.LineFeed + Keyword.EndStream);

        internal IInputStream body;
        internal PdfDictionary header;

        private PdfObject parent;
        private PdfObjectStatus status;

        /// <summary>Indicates whether {@link #body} has already been resolved and therefore contains the
        /// actual stream data.</summary>
        private bool bodyResolved;
        internal EncodeState encoded = EncodeState.None;

        public PdfStream() : this(new PdfDictionary(), new ByteStream())
        { }

        public PdfStream(PdfDictionary header) : this(header, new ByteStream())
        { }

        public PdfStream(IInputStream body) : this(new PdfDictionary(), body)
        { }

        public PdfStream(PdfDictionary header, IInputStream body)
            : base(PdfObjectStatus.Updateable)
        {
            this.header = (PdfDictionary)Include(header);
            SetStream(body);
        }

        public override PdfObject Accept(IVisitor visitor, object data) => visitor.Visit(this, data);


        public PdfDirectObject Filter
        {
            get => (PdfDirectObject)(header[PdfName.F] == null
                  ? header.Resolve(PdfName.Filter)
                  : header.Resolve(PdfName.FFilter));
            protected set => header[
                  header[PdfName.F] == null
                    ? PdfName.Filter
                    : PdfName.FFilter
                  ] = value;
        }

        /// <summary>Gets the stream header.</summary>
        public PdfDictionary Header => header;

        public PdfDirectObject Parameters
        {
            get => (PdfDirectObject)(header[PdfName.F] == null
                  ? header.Resolve(PdfName.DecodeParms)
                  : header.Resolve(PdfName.FDecodeParms));
            protected set => header[
                  header[PdfName.F] == null
                    ? PdfName.DecodeParms
                    : PdfName.FDecodeParms
                  ] = value;
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

        [PDF(VersionEnum.PDF12)]
        public FileSpecification DataFile
        {
            get => FileSpecification.Wrap(header[PdfName.F]);
            set => SetDataFile(value, false);
        }

        public override IPdfObjectWrapper Wrapper
        {
            get => Header.Wrapper;
            internal set => Header.Wrapper = value;
        }

        public override IPdfObjectWrapper Wrapper2
        {
            get => Header.Wrapper2;
            internal set => Header.Wrapper2 = value;
        }
        public override IPdfObjectWrapper Wrapper3
        {
            get => Header.Wrapper3;
            internal set => Header.Wrapper3 = value;
        }

        public void SetStream(IInputStream value)
        {
            if (body == value)
                return;
            Updateable = false;
            if (body is IByteStream oldByteStream)
            {
                oldByteStream.OnChange -= OnChange;
            }
            body?.Dispose();
            body = value;
            if (body is IByteStream byteStream)
            {
                byteStream.OnChange += OnChange;
            }
            Updateable = true;
        }

        public void SetStreamAndRemoveFilters(IInputStream value)
        {
            SetStream(value);
            // The stream is free from encodings.
            header.Updateable = false;
            Filter = null;
            Parameters = null;
            header.Updateable = true;
        }

        void OnChange(object sender, EventArgs args) => Update();

        /// <summary>Gets the stream body for edit.</summary>
        
        public IByteStream GetOutputStream()
        {
            if (GetInputStreamNoDecode() is IByteStream buffer)
                return buffer;
            SetStreamAndRemoveFilters(buffer = new ByteStream());
            return buffer;
        }

        /// <summary>Gets the stream body.</summary>
        public IInputStream GetInputStreamNoDecode()
        {
            if (!bodyResolved)
            {
                // NOTE: In case of stream data from external file, a copy to the local buffer has to be done.
                if (DataFile is FileSpecification dataFile)
                {
                    SetStream(dataFile.GetInputStream());
                }
                bodyResolved = true;
            }
            body.Position = 0;
            return body;
        }

        /// <remarks>NOTE: Encoding filters are removed by default because they belong to a lower layer (token
        /// layer), so that it's appropriate and consistent to transparently keep the object layer
        /// unaware of such a facility.</remarks>
        public IInputStream GetInputStream()
        {
            GetInputStreamNoDecode();

            if (Filter is PdfDataObject filter) // Stream encoded.
            {
                SetStreamAndRemoveFilters(body.Decode(filter, Parameters, Header));
            }
            body.Position = 0;
            return body;
        }

        public IInputStream GetExtractedStream()
        {
            var buffer = GetInputStreamNoDecode();
            buffer = buffer.Decode(Filter, Parameters, Header);
            return buffer;
        }

        /// <param name="preserve">Indicates whether the data from the old data source substitutes the
        /// new one. This way data can be imported to/exported from local or preserved in case of external
        /// file location changed.</param>
        /// <seealso cref="DataFile"/>
        public void SetDataFile(FileSpecification value, bool preserve)
        {
            /*
              NOTE: If preserve argument is set to true, body's dirtiness MUST be forced in order to ensure
              data serialization to the new external location.

              Old data source | New data source | preserve | Action
              ----------------------------------------------------------------------------------------------
              local           | not null        | false     | A. Substitute local with new file.
              local           | not null        | true      | B. Export local to new file.
              external        | not null        | false     | C. Substitute old file with new file.
              external        | not null        | true      | D. Copy old file data to new file.
              local           | null            | (any)     | E. No action.
              external        | null            | false     | F. Empty local.
              external        | null            | true      | G. Import old file to local.
              ----------------------------------------------------------------------------------------------
            */
            FileSpecification oldDataFile = DataFile;
            PdfDirectObject dataFileObject = value?.BaseObject;
            if (value != null)
            {
                if (preserve)
                {
                    if (oldDataFile != null) // Case D (copy old file data to new file).
                    {
                        if (!bodyResolved)
                        {
                            // Transfer old file data to local!
                            GetInputStreamNoDecode(); // Ensures that external data is loaded as-is into the local buffer.
                        }
                    }
                    else // Case B (export local to new file).
                    {
                        // Transfer local settings to file!
                        header[PdfName.FFilter] = header[PdfName.Filter]; header.Remove(PdfName.Filter);
                        header[PdfName.FDecodeParms] = header[PdfName.DecodeParms]; header.Remove(PdfName.DecodeParms);

                        // Ensure local data represents actual data (otherwise it would be substituted by resolved file data)!
                        bodyResolved = true;
                    }
                    // Ensure local data has to be serialized to new file!
                    body.Dirty = true;
                }
                else // Case A/C (substitute local/old file with new file).
                {
                    if (body is IByteStream buffered)
                        // Dismiss local/old file data!
                        buffered.SetLength(0);
                    // Dismiss local/old file settings!
                    Filter = null;
                    Parameters = null;
                    // Ensure local data has to be loaded from new file!
                    bodyResolved = false;
                }
            }
            else
            {
                if (oldDataFile != null)
                {
                    if (preserve) // Case G (import old file to local).
                    {
                        // Transfer old file data to local!
                        GetInputStreamNoDecode(); // Ensures that external data is loaded as-is into the local buffer.
                                                  // Transfer old file settings to local!
                        header[PdfName.Filter] = header[PdfName.FFilter];
                        header.Remove(PdfName.FFilter);
                        header[PdfName.DecodeParms] = header[PdfName.FDecodeParms];
                        header.Remove(PdfName.FDecodeParms);
                    }
                    else // Case F (empty local).
                    {
                        if (body is IByteStream buffered)
                            // Dismiss old file data!
                            buffered.SetLength(0);
                        // Dismiss old file settings!
                        Filter = null;
                        Parameters = null;
                        // Ensure local data represents actual data (otherwise it would be substituted by resolved file data)!
                        bodyResolved = true;
                    }
                }
                else // E (no action).
                { /* NOOP */ }
            }
            header[PdfName.F] = dataFileObject;
        }

        public override PdfObject Swap(PdfObject other)
        {
            PdfStream otherStream = (PdfStream)other;
            PdfDictionary otherHeader = otherStream.header;
            var otherBody = otherStream.body;
            var otherBodyResolved = otherStream.bodyResolved;
            // Update the other!
            otherStream.header = header;
            otherStream.body = body;
            otherStream.bodyResolved = bodyResolved;
            otherStream.Update();
            // Update this one!
            header = otherHeader;
            body = otherBody;
            bodyResolved = otherBodyResolved;
            Update();
            return this;
        }

        public override void WriteTo(IOutputStream stream, PdfFile context)
        {
            // NOTE: The header is temporarily tweaked to accommodate serialization settings.
            header.Updateable = false;

            var bodyData = body;
            bool filterApplied = false;
            // NOTE: In case of external file, the body buffer has to be saved back only if the file was
            // actually resolved (that is brought into the body buffer) and modified.
            FileSpecification dataFile = DataFile;
            if (dataFile == null || (bodyResolved && body.Dirty))
            {
                // NOTE: In order to keep the contents of metadata streams visible as plain text to tools
                // that are not PDF-aware, no filter is applied to them [PDF:1.7:10.2.2].
                if (Filter == null
                   && context.Configuration.StreamFilterEnabled
                   && !PdfName.Metadata.Equals(header.Get<PdfName>(PdfName.Type))) // Filter needed.
                {
                    // Apply the filter to the stream!
                    Filter = PdfName.FlateDecode;
                    bodyData = body.Encode(Bytes.Filters.Filter.Get(PdfName.FlateDecode), null, Header);
                    filterApplied = true;
                }

                if (dataFile != null)
                {
                    try
                    {
                        using var dataFileOutputStream = dataFile.GetOutputStream();
                        bodyData.CopyTo(dataFileOutputStream);
                    }
                    catch (Exception e)
                    { throw new Exception("Data writing into " + dataFile.Path + " failed.", e); }
                }
            }
            if (dataFile != null)
            { bodyData = ByteStream.Empty; }

            // Set the encoded data length!
            header.Set(PdfName.Length, bodyData.Length);

            // 1. Header.
            header.WriteTo(stream, context);

            if (filterApplied)
            {
                // Restore actual header entries!
                header.Set(PdfName.Length, (int)body.Length);
                Filter = null;
            }

            // 2. Body.
            stream.Write(BeginStreamBodyChunk);
            stream.Write(bodyData);
            stream.Write(EndStreamBodyChunk);

            header.Updateable = true;
        }

    }

    public enum EncodeState
    {
        None,
        Encoded,
        Decoded,
        Decoding,
        Identity,
        SkipXRef,
        SkipMetadata,
    }
}
