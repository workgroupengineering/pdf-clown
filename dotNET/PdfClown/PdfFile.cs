/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Kasper Fabaech Brandt (patch contributor [FIX:45], http://sourceforge.net/u/kasperfb/)

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
using PdfClown.Documents.Encryption;
using PdfClown.Files;
using PdfClown.Objects;
using PdfClown.Tokens;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace PdfClown
{
    /// <summary>PDF file representation.</summary>
    public sealed class PdfFile : IDisposable
    {
        private sealed class ImplicitContainer : PdfIndirectObject
        {
            public ImplicitContainer(PdfFile file, PdfDataObject dataObject)
                : base(file, dataObject, new XRefEntry(int.MinValue, int.MinValue))
            { }
        }

        private static readonly Random hashCodeGenerator = new();

        private FileConfiguration configuration;
        private readonly PdfDocument document;
        private readonly int hashCode = hashCodeGenerator.Next();
        private readonly IndirectObjects indirectObjects;
        private string path;
        private Reader reader;
        private readonly PdfDictionary trailer;
        private readonly PdfVersion version;
        private Cloner cloner;
        public readonly ManualResetEventSlim LockObject = new(true);

        public PdfFile()
        {
            Initialize();

            version = PdfVersion.Get(VersionEnum.PDF14);
            trailer = PrepareTrailer(new PdfDictionary());
            indirectObjects = new IndirectObjects(this, null);
            document = new PdfDocument(this);
        }

        public PdfFile(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            this.path = path;
        }

        public PdfFile(byte[] data) : this((IInputStream)new ByteStream(data))
        { }

        public PdfFile(Stream stream) : this((IInputStream)new StreamContainer(stream))
        {
            //if (stream is FileStream fileStream)
            //    path = fileStream.Name;
        }

        public PdfFile(IInputStream stream)
        {
            Initialize();

            reader = new Reader(stream, this);
            try // [FIX:45] File constructor didn't dispose reader on error.
            {
                var info = reader.ReadInfo();
                version = info.Version;
                trailer = PrepareTrailer(info.Trailer);
                indirectObjects = new IndirectObjects(this, info.XrefEntries);

                reader.PrepareDecryption();

                var documentReference = trailer[PdfName.Root];
                if (documentReference.Resolve() is PdfDictionary)
                {
                    document = PdfObjectWrapper.Wrap<PdfDocument>(documentReference);
                }
                else
                {
                    foreach (var inderectObject in indirectObjects)
                    {
                        var entry = inderectObject.Resolve();
                        if (entry is PdfDictionary entryDictionary
                            && entryDictionary[PdfName.Pages] != null)
                        {
                            document = PdfObjectWrapper.Wrap<PdfDocument>(entry.Reference);
                        }
                    }
                }
                Configuration.XRefMode = PdfName.XRef.Equals(trailer.Get<PdfName>(PdfName.Type))
                  ? XRefModeEnum.Compressed
                  : XRefModeEnum.Plain;
            }
            catch (Exception)
            {
                reader.Dispose();
                throw;
            }
        }

        ~PdfFile()
        {
            Dispose(false);
        }

        /// <summary>Gets the file configuration.</summary>
        public FileConfiguration Configuration => configuration;

        /// <summary>Gets the high-level representation of the file content.</summary>
        public PdfDocument Document => document;

        public PdfEncryption Encryption
        {
            get => PdfObjectWrapper.Wrap<PdfEncryption>(trailer[PdfName.Encrypt]);
            set => trailer[PdfName.Encrypt] = value?.BaseDataObject;
        }

        /// <summary>Gets the identifier of this file.</summary>
        public FileIdentifier ID
        {
            get => PdfObjectWrapper.Wrap<FileIdentifier>(Trailer[PdfName.ID]);
            set => Trailer[PdfName.ID] = value.BaseDataObject;
        }

        /// <summary>Gets the indirect objects collection.</summary>
        public IndirectObjects IndirectObjects => indirectObjects;

        /// <summary>Gets/Sets the file path.</summary>
        public string Path
        {
            get => path;
            set => path = value;
        }

        /// <summary>Gets the data reader backing this file.</summary>
        /// <returns><code>null</code> in case of newly-created file.</returns>
        public Reader Reader => reader;


        /// <summary>Gets the file trailer.</summary>
        public PdfDictionary Trailer => trailer;

        /// <summary>Gets whether the initial state of this file has been modified.</summary>
        public bool Updated => indirectObjects.ModifiedObjects.Count > 0;

        /// <summary>Gets the file header version [PDF:1.6:3.4.1].</summary>
        /// <remarks>This property represents just the original file version; to get the actual version,
        /// use the <see cref="PdfDocument.Version">Document.Version</see> method.
        /// </remarks>
        public PdfVersion Version => version;

        /// <summary>Registers an <b>internal data object</b>.</summary>
        public PdfReference Register(PdfDataObject obj)
        {
            return indirectObjects.Add(obj).Reference;
        }

        /// <summary>Serializes the file to the current file-system path using the 
        /// <see cref="SerializationModeEnum.Incremental">incremental for signatures</see> or 
        /// <see cref="SerializationModeEnum.Standard">standard serialization mode</see>.</summary>
        public void Save() => Save(Document.HasSignatures ? SerializationModeEnum.Incremental : SerializationModeEnum.Standard);

        /// <summary>Serializes the file to the current file-system path.</summary>
        /// <param name="mode">Serialization mode.</param>
        public void Save(SerializationModeEnum mode)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("No valid source path available.");

            // NOTE: The document file cannot be directly overwritten as it's locked for reading by the
            // open stream; its update is therefore delayed to its disposal, when the temporary file will
            // overwrite it (see Dispose() method).
            Save(TempPath, mode);
            CompleatSave();
        }

        /// <summary>Serializes the file to the specified file system path using the 
        /// <see cref="SerializationModeEnum.Incremental">incremental for signatures</see> or 
        /// <see cref="SerializationModeEnum.Standard">standard serialization mode</see>.</summary>
        /// <param name="path">Target path.</param>
        public void Save(string path) => Save(path, Document.HasSignatures ? SerializationModeEnum.Incremental : SerializationModeEnum.Standard);

        /// <summary>Serializes the file to the specified file system path .</summary>
        /// <param name="path">Target path.</param>
        /// <param name="mode">Serialization mode.</param>
        public void Save(string path, SerializationModeEnum mode)
        {
            using (var outputStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                Save((IOutputStream)new StreamContainer(outputStream), mode);
            }
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
        }

        /// <summary>Serializes the file to the specified stream using the 
        /// <see cref="SerializationModeEnum.Incremental">incremental for signatures</see> or 
        /// <see cref="SerializationModeEnum.Standard">standard serialization mode</see>.</summary>
        /// <param name="path">Target path.</param>
        public void Save(Stream stream) => Save(stream, Document.HasSignatures ? SerializationModeEnum.Incremental : SerializationModeEnum.Standard);

        /// <summary>Serializes the file to the specified stream.</summary>
        /// <remarks>It's caller responsibility to close the stream after this method ends.</remarks>
        /// <param name="stream">Target stream.</param>
        /// <param name="mode">Serialization mode.</param>
        public void Save(Stream stream, SerializationModeEnum mode) => Save((IOutputStream)new StreamContainer(stream), mode);

        /// <summary>Serializes the file to the specified stream.</summary>
        /// <remarks>It's caller responsibility to close the stream after this method ends.</remarks>
        /// <param name="stream">Target stream.</param>
        /// <param name="mode">Serialization mode.</param>
        public void Save(IOutputStream stream, SerializationModeEnum mode)
        {
            var information = Document.Information;
            if (Reader == null)
            {
                information.CreationDate = DateTime.Now;
                try
                {
                    string assemblyTitle = Assembly.GetExecutingAssembly().GetName().Name;
                    string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    information.Producer = assemblyTitle + " " + assemblyVersion;
                }
                catch
                {/* NOOP */}
            }
            else
            { information.ModificationDate = DateTime.Now; }

            var writer = Writer.Get(this, stream);
            writer.Write(mode);
        }

        /// <summary>Unregisters an internal object.</summary>
        public void Unregister(PdfReference reference)
        {
            indirectObjects.RemoveAt(reference.ObjectNumber);
        }

        /// <summary>Gets/Sets the default cloner.</summary>
        public Cloner Cloner
        {
            get => cloner ??= new Cloner(this);
            set => cloner = value;
        }

        public bool IsDisposed => reader == null;

        public override int GetHashCode()
        {
            return hashCode;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                    CompleatSave();
                }
                Document?.ClearCache();
                LockObject?.Dispose();
            }
        }

        private void CompleatSave()
        {
            //NOTE: If the temporary file exists (see Save() method), it must overwrite the document file.
            if (File.Exists(TempPath))
            {
                File.Delete(path);
                File.Move(TempPath, path);
            }
        }

        private void Initialize()
        {
            configuration = new FileConfiguration(this);
        }

        private PdfDictionary PrepareTrailer(PdfDictionary trailer)
        {
            return (PdfDictionary)new ImplicitContainer(this, trailer).DataObject;
        }

        private string TempPath => path == null ? null : $"{path}.tmp";

    }
}