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

using PdfClown.Documents;
using PdfClown.Documents.Interchange.Metadata;
using PdfClown.Util;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PdfClown.Objects
{

    /// <summary>Base high-level representation of a weakly-typed PDF object.</summary>
    public abstract class PdfObjectWrapper : IPdfObjectWrapper
    {
        /// <summary>Gets the PDF object backing the specified wrapper.</summary>
        /// <param name="wrapper">Object to extract the base from.</param>
        public static PdfDirectObject GetBaseObject(PdfObjectWrapper wrapper)
        {
            return wrapper?.BaseObject;
        }

        public static PdfDictionary TryGetDictionary(PdfDataObject baseDataObject)
        {
            if (baseDataObject is PdfDictionary dictionary)
                return dictionary;
            else if (baseDataObject is PdfStream stream)
                return stream.Header;
            else
                return null;
        }

        public static T Wrap<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(PdfDirectObject baseObject)
            where T : IPdfObjectWrapper
        {
            return baseObject != null
                ? (baseObject.Wrapper is T exist ? exist
                  : (T)Activator.CreateInstance(typeof(T), baseObject))
                : default(T);
        }

        public static T Wrap2<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(PdfDirectObject baseObject)
            where T : PdfObjectWrapper2
        {
            return baseObject != null
                  ? (baseObject.Wrapper2 is T exist ? exist
                    : (T)Activator.CreateInstance(typeof(T), baseObject))
                  : default(T);
        }

        public static T Wrap3<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(PdfDirectObject baseObject)
            where T : PdfObjectWrapper3
        {
            return baseObject != null
                  ? (baseObject.Wrapper3 is T exist ? exist
                    : (T)Activator.CreateInstance(typeof(T), baseObject))
                  : default(T);
        }

        private PdfDirectObject baseObject;

        /// <summary>Instantiates an empty wrapper.</summary>
        protected PdfObjectWrapper()
        { }

        /// <summary>Instantiates a wrapper from the specified base object.</summary>
        /// <param name="baseObject">PDF object backing this wrapper. MUST be a <see cref="PdfReference"/>
        /// every time available.</param>
        public PdfObjectWrapper(PdfDirectObject baseObject)
        {
            BaseObject = baseObject;

            if (baseObject != null)
                baseObject.Wrapper = this;
        }

        /// <summary>Gets the indirect object containing the base object.</summary>
        public PdfIndirectObject Container => baseObject.Container;

        /// <summary>Gets the indirect object containing the base data object.</summary>
        public PdfIndirectObject DataContainer => baseObject.DataContainer;

        /// <summary>Gets/Sets the metadata associated to this object.</summary>
        /// <returns><code>null</code>, if base data object's type isn't suitable (only
        /// <see cref="PdfDictionary"/> and <see cref="PdfStream"/> objects are allowed).</returns>
        /// <throws>NotSupportedException If base data object's type isn't suitable (only
        /// <see cref="PdfDictionary"/> and <see cref="PdfStream"/> objects are allowed).</throws>
        public virtual Metadata Metadata
        {
            get => Dictionary is PdfDictionary dictionary ? Metadata.Wrap(dictionary.GetOrCreate<PdfStream>(PdfName.Metadata, false)) : null;
            set
            {
                PdfDictionary dictionary = Dictionary;
                if (dictionary == null)
                    throw new NotSupportedException("Metadata can be attached only to PdfDictionary/PdfStream base data objects.");

                dictionary[PdfName.Metadata] = PdfObjectWrapper.GetBaseObject(value);
            }
        }

        protected virtual PdfDictionary Dictionary =>
            BaseDataObject switch
            {
                PdfDictionary dictionary => dictionary,
                PdfStream stream => stream.Header,
                _ => null
            };

        /// <summary>Removes the object from its document context.</summary>
        /// <remarks>Only indirect objects can be removed through this method; direct objects have to be
        /// explicitly removed from their parent object. The object is no more usable after this method
        /// returns.</remarks>
        /// <returns>Whether the object was removed from its document context.</returns>
        public virtual bool Delete() => baseObject.Delete();

        /// <summary>Gets the document context.</summary>
        public PdfDocument Document => File?.Document;

        /// <summary>Gets the file context.</summary>
        public PdfFile File => baseObject.File;

        public virtual PdfDirectObject BaseObject
        {
            get => baseObject;
            protected set => baseObject = value;
        }

        /// <summary>Gets the underlying data object.</summary>
        public PdfDataObject BaseDataObject => PdfObject.Resolve(BaseObject);

        /// <summary>Gets whether the underlying data object is concrete.</summary>
        public bool Exists() => !BaseDataObject.Virtual;

        /// <summary>Gets a clone of the object, registered inside the specified document context using
        /// the default object cloner.</summary>
        public virtual object Clone(PdfDocument context) => Clone(context.File.Cloner);

        /// <summary>Gets a clone of the object, registered using the specified object cloner.</summary>
        public virtual object Clone(Cloner cloner)
        {
            PdfObjectWrapper clone = (PdfObjectWrapper)base.MemberwiseClone();
            clone.BaseObject = (PdfDirectObject)BaseObject.Clone(cloner);
            if (clone.BaseObject != null)
            {
                clone.BaseObject.Wrapper = null;
                clone.BaseObject.Wrapper = clone;
            }
            return clone;
        }

        public override bool Equals(object other)
        {
            return other != null
              && other.GetType().Equals(GetType())
              && ((PdfObjectWrapper)other).baseObject.Equals(baseObject);
        }

        public override int GetHashCode() => baseObject.GetHashCode();

        public override string ToString()
        {
            return $"{GetType().Name} {{{(BaseObject is PdfReference ? (PdfObject)BaseObject.DataContainer : BaseObject)}}}";
        }

        /// <summary>Checks whether the specified feature is compatible with the
        ///   <see cref="PdfDocument.Version">document's conformance version</see>.</summary>
        /// <param name="feature">Entity whose compatibility has to be checked. Supported types:
        ///   <list type="bullet">
        ///     <item><see cref="VersionEnum"/></item>
        ///   </list>
        /// </param>
        internal void CheckCompatibility(VersionEnum feature)
        {
            // TODO: Caching!
            var compatibilityMode = Document.Configuration.CompatibilityMode;
            if (compatibilityMode == CompatibilityModeEnum.Passthrough) // No check required.
                return;

            //if (feature is Enum)
            //{
            //    Type enumType = feature.GetType();
            //    if (enumType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0)
            //    {
            //        int featureEnumValues = Convert.ToInt32(feature);
            //        var featureEnumItems = new List<Enum>();
            //        foreach (int enumValue in Enum.GetValues(enumType))
            //        {
            //            if ((featureEnumValues & enumValue) == enumValue)
            //            { featureEnumItems.Add((Enum)Enum.ToObject(enumType, enumValue)); }
            //        }
            //        if (featureEnumItems.Count > 1)
            //        { feature = featureEnumItems; }
            //    }
            //}
            //if (feature is ICollection)
            //{
            //    foreach (Object featureItem in (ICollection)feature)
            //    { CheckCompatibility(featureItem); }
            //    return;
            //}

            var featureVersion = PdfVersion.Get(feature);
            //if (feature is VersionEnum) // Explicit version.
            //{ featureVersion = ((VersionEnum)feature).GetVersion(); }
            //else // Implicit version (element annotation).
            //{
            //    PDFAttribute annotation;
            //    {
            //        if (feature is string) // Property name.
            //        { feature = GetType().GetProperty((string)feature); }
            //        else if (feature is Enum) // Enum constant.
            //        { feature = feature.GetType().GetField(feature.ToString()); }
            //        if (!(feature is MemberInfo))
            //            throw new ArgumentException("Feature type '" + feature.GetType().Name + "' not supported.");
            //        while (true)
            //        {
            //            var annotations = ((MemberInfo)feature).GetCustomAttributes<PDFAttribute>(true);
            //            if (annotations.Any())
            //            {
            //                annotation = annotations.FirstOrDefault();
            //                break;
            //            }
            //            feature = ((MemberInfo)feature).DeclaringType;
            //            if (feature == null) // Element hierarchy walk complete.
            //                return; // NOTE: As no annotation is available, we assume the feature has no specific compatibility requirements.
            //        }
            //    }
            //    featureVersion = annotation.Value.GetVersion();
            //}
            // Is the feature version compatible?
            if (Document.Version.CompareTo(featureVersion) >= 0)
                return;

            // The feature version is NOT compatible: how to solve the conflict?
            switch (compatibilityMode)
            {
                case CompatibilityModeEnum.Loose: // Accepts the feature version.
                                                  // Synchronize the document version!
                    Document.Version = featureVersion;
                    break;
                case CompatibilityModeEnum.Strict: // Refuses the feature version.
                                                   // Throw a violation to the document version!
                    throw new Exception("Incompatible feature (version " + featureVersion + " was required against document version " + Document.Version);
                default:
                    throw new NotImplementedException("Unhandled compatibility mode: " + compatibilityMode);
            }
        }

        /// <summary>Retrieves the name possibly associated to this object, walking through the document's
        /// name dictionary.</summary>
        protected virtual PdfString RetrieveName()
        {
            return Document.Names.Get(GetType()) is IBiDictionary biDictionary 
                ? biDictionary.GetKey(this) as PdfString 
                : null;
        }

        ///<summary>Retrieves the object name, if available; otherwise, behaves like
        ///<see cref="PdfObjectWrapper.BaseObject"/>.</summary>
        protected PdfDirectObject RetrieveNamedBaseObject()
        {
            return RetrieveName() ?? BaseObject;
        }
    }

    /// <summary>High-level representation of a strongly-typed PDF object.</summary>
    /// <remarks>
    ///  <para>Specialized objects don't inherit directly from their low-level counterparts (e.g.
    ///    <see cref="PdfClown.Documents.Contents.ContentWrapper">Contents</see> extends <see
    ///    cref="PdfStream">PdfStream</see>, <see
    ///    cref="Pages">Pages</see> extends <see
    ///    cref="PdfArray">PdfArray</see> and so on) because there's no plain
    ///    one-to-one mapping between primitive PDF types and specialized instances: the
    ///    <code>Content</code> entry of <code>Page</code> dictionaries may be a simple reference to a
    ///    <code>PdfStream</code> or a <code>PdfArray</code> of references to <code>PdfStream</code>s,
    ///    <code>Pages</code> collections may be spread across a B-tree instead of a flat
    ///    <code>PdfArray</code> and so on.
    ///  </para>
    ///  <para>So, in order to hide all these annoying inner workings, I chose to adopt a composition
    ///    pattern instead of the apparently-reasonable (but actually awkward!) inheritance pattern.
    ///    Nonetheless, users can navigate through the low-level structure getting the <see
    ///    cref="BaseDataObject">BaseDataObject</see> backing this object.
    ///  </para>
    /// </remarks>
    public abstract class PdfObjectWrapper<TDataObject> : PdfObjectWrapper
      where TDataObject : PdfDataObject
    {
        /// <summary>Instantiates an empty wrapper.</summary>
        protected PdfObjectWrapper()
        { }

        /// <summary>Instantiates a wrapper from the specified base object.</summary>
        /// <param name="baseObject">PDF object backing this wrapper. It MUST be a <see cref="PdfReference"/>
        /// every time available.</param>
        public PdfObjectWrapper(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Instantiates a wrapper registering the specified base data object into the specified
        /// document context.</summary>
        /// <param name="context">Document context into which the specified data object has to be
        /// registered.</param>
        /// <param name="baseDataObject">PDF data object backing this wrapper.</param>
        /// <seealso cref="PdfObjectWrapper(PdfFile, PdfDataObject)"/>
        protected PdfObjectWrapper(PdfDocument context, TDataObject baseDataObject)
            : this(context?.File, baseDataObject)
        { }

        /// <summary>Instantiates a wrapper registering the specified base data object into the specified
        /// file context.</summary>
        /// <param name="context">File context into which the specified data object has to be registered.
        /// </param>
        /// <param name="baseDataObject">PDF data object backing this wrapper.</param>
        /// <seealso cref="PdfObjectWrapper(PdfDocument, PdfDataObject)"/>
        protected PdfObjectWrapper(PdfFile context, TDataObject baseDataObject)
            : this(context != null ? context.Register(baseDataObject) : (PdfDirectObject)(PdfDataObject)baseDataObject)
        { }

        ///<summary>Gets the underlying data object.</summary>
        public new TDataObject BaseDataObject => (TDataObject)BaseObject?.Resolve();
    }
}