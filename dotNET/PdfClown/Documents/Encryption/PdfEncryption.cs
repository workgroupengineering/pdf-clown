/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfClown.Objects;
using System;
using System.IO;

namespace PdfClown.Documents.Encryption
{

    /// <summary>
    /// This class is a specialized view of the encryption Dictionary of a PDF document.
    /// It contains a low level Dictionary (PdfDictionary) and provides the methods to
    /// manage its fields.
    /// </summary>
    /// <remarks>
    /// The available fields are the ones who are involved by standard security handler
    ///  and public key security handler.    
    /// @author Ben Litchfield
    /// @author Benoit Guillon
    /// </remarks>
    public class PdfEncryption : PdfObjectWrapper<PdfDictionary>
    {
        /// <summary>See PDF Reference 1.4 Table 3.13.</summary>
        public static readonly int VERSION0_UNDOCUMENTED_UNSUPPORTED = 0;
        /// <summary>See PDF Reference 1.4 Table 3.13.</summary>
        public static readonly int VERSION1_40_BIT_ALGORITHM = 1;
        /// <summary>See PDF Reference 1.4 Table 3.13.</summary>
        public static readonly int VERSION2_VARIABLE_LENGTH_ALGORITHM = 2;
        /// <summary>See PDF Reference 1.4 Table 3.13.</summary>
        public static readonly int VERSION3_UNPUBLISHED_ALGORITHM = 3;
        /// <summary>See PDF Reference 1.4 Table 3.13.</summary>
        public static readonly int VERSION4_SECURITY_HANDLER = 4;

        /// <summary>The default security handler.</summary>
        public static readonly string DEFAULT_NAME = "Standard";

        /// <summary>The default length for the encryption key.</summary>
        public static readonly int DEFAULT_LENGTH = 40;

        /// <summary>The default version, according to the PDF Reference.</summary>
        public static readonly int DEFAULT_VERSION = VERSION0_UNDOCUMENTED_UNSUPPORTED;

        private ISecurityHandler securityHandler;

        /// <summary>creates a new empty encryption Dictionary.</summary>
        /// <param name="context">File</param>
        public PdfEncryption(PdfFile context) : base(context, new PdfDictionary())
        {
        }

        /// <summary>creates a new encryption Dictionary from the low level Dictionary provided.</summary>
        /// <param name="baseObject">a PDF encryption Dictionary</param>
        public PdfEncryption(PdfDirectObject baseObject)
            : base(baseObject)
        {
            securityHandler = SecurityHandlerFactory.INSTANCE.NewSecurityHandlerForFilter(Filter);
        }

        /// <summary>Returns the security handler specified in the Dictionary's Filter entry.</summary>
        /// <value> a security handler instance</value>
        public ISecurityHandler SecurityHandler
        {
            get
            {
                if (securityHandler == null)
                {
                    throw new IOException("No security handler for filter " + Filter);
                }
                return securityHandler;
            }
            set => this.securityHandler = value;// TODO set Filter (currently this is done by the security handlers)
        }

        /// <summary>Returns true if the security handler specified in the Dictionary's Filter is available.</summary>
        /// <value> true if the security handler is available </value> 
        public bool HasSecurityHandler
        {
            get => securityHandler == null;
        }

        /// <summary>Get the name of the filter.</summary>
        /// <value>The filter name contained in this encryption Dictionary.</value>>
        public string Filter
        {
            get => Dictionary.GetString(PdfName.Filter);
            set => Dictionary.SetName(PdfName.Filter, value);

        }

        /// <summary>Get the name of the subfilter.</summary>
        /// <value>The subfilter name contained in this encryption Dictionary.</value>
        public string SubFilter
        {
            get => Dictionary.GetString(PdfName.SubFilter);
            set => Dictionary.SetName(PdfName.SubFilter, value);
        }

        /// <summary> This will return the V entry of the encryption Dictionary.<br></br>
        /// See PDF Reference 1.4 Table 3.13.  <br></br>
        /// <b>Note: This value is used to decrypt the pdf document.  If you change this when
        /// the document is encrypted then decryption will fail!.</b>
        /// </summary>
        /// <value>The encryption version to use.</value>
        public int Version
        {
            get => Dictionary.GetInt(PdfName.V, 0);
            set => Dictionary.Set(PdfName.V, value);
        }


        /// <summary>
        /// This will return the Length entry of the encryption Dictionary.<br></br>
        /// The length in <b>bits</b> for the encryption algorithm.  This will return a multiple of 8.
        /// </summary>
        /// <value>The length in bits for the encryption algorithm</value>
        public int Length
        {
            get => Dictionary.GetInt(PdfName.Length, 0);
            set => Dictionary.Set(PdfName.Length, value);
        }

        /// <summary>
        /// This will return the R entry of the encryption Dictionary.<br></br>
        /// See PDF Reference 1.4 Table 3.14.  <br></br>
        /// <b>Note: This value is used to decrypt the pdf document.  If you change this when
        /// the document is encrypted then decryption will fail!.</b>
        ///</summary> 
        /// <value> The encryption revision to use. </value>
        public int Revision
        {
            get => Dictionary.GetInt(PdfName.R, DEFAULT_VERSION);
            set => Dictionary.Set(PdfName.R, value);
        }

        /**
		 * This will get the O entry in the standard encryption Dictionary.
		 *
		 * @return A 32 byte array or null if there is no owner key.
		 *
		 * @throws IOException If there is an error accessing the data.
		 */
        /**
		* This will set the O entry in the standard encryption Dictionary.
		*
		* @param o A 32 byte array or null if there is no owner key.
		*
		* @throws IOException If there is an error setting the data.
		*/
        public Memory<byte> OwnerKey
        {
            get => Dictionary.GetTextBytes(PdfName.O);
            set => Dictionary.Set(PdfName.O, value);
        }


        /**
		 * This will get the U entry in the standard encryption Dictionary.
		 *
		 * @return A 32 byte array or null if there is no user key.
		 *
		 * @throws IOException If there is an error accessing the data.
		 */
        /**
		 * This will set the U entry in the standard encryption Dictionary.
		 *
		 * @param u A 32 byte array.
		 *
		 * @throws IOException If there is an error setting the data.
		 */
        public Memory<byte> UserKey
        {
            get => Dictionary.GetTextBytes(PdfName.U);
            set => Dictionary.Set(PdfName.U, value);
        }

        /**
		 * This will get the OE entry in the standard encryption Dictionary.
		 *
		 * @return A 32 byte array or null if there is no owner encryption key.
		 *
		 * @throws IOException If there is an error accessing the data.
		 */
        /**
		 * This will set the OE entry in the standard encryption Dictionary.
		 *
		 * @param oe A 32 byte array or null if there is no owner encryption key.
		 *
		 * @throws IOException If there is an error setting the data.
		 */
        public Memory<byte> OwnerEncryptionKey
        {
            get => Dictionary.GetTextBytes(PdfName.OE);
            set => Dictionary.Set(PdfName.OE, value);
        }

        /**
		 * This will get the UE entry in the standard encryption Dictionary.
		 *
		 * @return A 32 byte array or null if there is no user encryption key.
		 *
		 * @throws IOException If there is an error accessing the data.
		 */
        /**
		 * This will set the UE entry in the standard encryption Dictionary.
		 *
		 * @param ue A 32 byte array or null if there is no user encryption key.
		 *
		 * @throws IOException If there is an error setting the data.
		 */
        public Memory<byte> UserEncryptionKey
        {
            get => Dictionary.GetTextBytes(PdfName.UE);
            set => Dictionary.Set(PdfName.UE, value);
        }

        /**
		 * This will get the permissions bit mask.
		 *
		 * @return The permissions bit mask.
		 */
        /**
		 * This will set the permissions bit mask.
		 *
		 * @param permissions The new permissions bit mask
		 */
        public int Permissions
        {
            get => Dictionary.GetInt(PdfName.P, 0);
            set => Dictionary.Set(PdfName.P, value);
        }

        /**
		 * Will get the EncryptMetaData Dictionary info.
		 * 
		 * @return true if EncryptMetaData is explicitly set to false (the default is true)
		 */
        public bool IsEncryptMetaData
        {
            get
            {
                // default is true (see 7.6.3.2 Standard Encryption Dictionary PDF 32000-1:2008)
                return Dictionary.GetBool(PdfName.EncryptMetadata, true);
            }
        }

        /**
		 * This will set the Recipients field of the Dictionary. This field contains an array
		 * of string.
		 * @param recipients the array of bytes arrays to put in the Recipients field.
		 * @throws IOException If there is an error setting the data.
		 */
        public void SetRecipients(byte[][] recipients)
        {
            Dictionary[PdfName.Recipients] = new PdfArray(recipients);
            //array.setDirect(true);
        }

        /**
		 * Returns the number of recipients contained in the Recipients field of the Dictionary.
		 *
		 * @return the number of recipients contained in the Recipients field.
		 */
        public int RecipientsLength
        {
            get => Dictionary.Get<PdfArray>(PdfName.Recipients).Count;
        }
        /**
		 * returns the PdfString contained in the Recipients field at position i.
		 *
		 * @param i the position in the Recipients field array.
		 *
		 * @return a PdfString object containing information about the recipient number i.
		 */
        public PdfString GetRecipientStringAt(int i)
        {
            return Dictionary.Get<PdfArray>(PdfName.Recipients).Get<PdfString>(i);
        }

        /**
		 * Returns the standard crypt filter.
		 * 
		 * @return the standard crypt filter if available.
		 */
        /**
		 * Sets the standard crypt filter.
		 * 
		 * @param cryptFilterDictionary the standard crypt filter to set
		 */
        public PdfCryptFilterDictionary StdCryptFilterDictionary
        {
            get => GetCryptFilterDictionary(PdfName.StdCF);
            set =>
                //value.getCOSObject().setDirect(true); // PDFBOX-4436
                SetCryptFilterDictionary(PdfName.StdCF, value);
        }

        /**
		 * Returns the default crypt filter (for public-key security handler).
		 * 
		 * @return the default crypt filter if available.
		 */
        /**
		 * Sets the default crypt filter (for public-key security handler).
		 *
		 * @param defaultFilterDictionary the standard crypt filter to set
		 */
        public PdfCryptFilterDictionary DefaultCryptFilterDictionary
        {
            get => GetCryptFilterDictionary(PdfName.DefaultCryptFilter);
            set =>
                //value.getCOSObject().setDirect(true); // PDFBOX-4436
                SetCryptFilterDictionary(PdfName.DefaultCryptFilter, value);
        }

        /**
		 * Returns the crypt filter with the given name.
		 * 
		 * @param cryptFilterName the name of the crypt filter
		 * 
		 * @return the crypt filter with the given name if available
		 */
        public PdfCryptFilterDictionary GetCryptFilterDictionary(PdfName cryptFilterName)
        {
            // See CF in "Table 20 – Entries common to all encryption dictionaries"
            var baseObj = Dictionary.Get<PdfDictionary>(PdfName.CF);
            if (baseObj != null)
            {
                var base2 = baseObj.Get<PdfDictionary>(cryptFilterName);
                if (base2 != null)
                {
                    return Wrap<PdfCryptFilterDictionary>(base2);
                }
            }
            return null;
        }

        /**
		 * Sets the crypt filter with the given name.
		 * 
		 * @param cryptFilterName the name of the crypt filter
		 * @param cryptFilterDictionary the crypt filter to set
		 */
        public void SetCryptFilterDictionary(PdfName cryptFilterName, PdfCryptFilterDictionary cryptFilterDictionary)
        {
            var cfDictionary = Dictionary.Get<PdfDictionary>(PdfName.CF);
            if (cfDictionary == null)
            {
                cfDictionary = new PdfDictionary();
                Dictionary[PdfName.CF] = cfDictionary;
            }
            //cfDictionary.setDirect(true); // PDFBOX-4436 direct obj needed for Adobe Reader on Android
            cfDictionary[cryptFilterName] = cryptFilterDictionary.BaseDataObject;
        }

        /**
		 * Returns the name of the filter which is used for de/encrypting streams.
		 * Default value is "Identity".
		 * 
		 * @return the name of the filter
		 */
        /**
		 * Sets the name of the filter which is used for de/encrypting streams.
		 * 
		 * @param streamFilterName the name of the filter
		 */
        public PdfName StreamFilterName
        {
            get => Dictionary.Get<PdfName>(PdfName.StmF, PdfName.Identity);
            set => Dictionary[PdfName.StmF] = value;
        }

        /**
		 * Returns the name of the filter which is used for de/encrypting strings.
		 * Default value is "Identity".
		 * 
		 * @return the name of the filter
		 */
        /**
		 * Sets the name of the filter which is used for de/encrypting strings.
		 * 
		 * @param stringFilterName the name of the filter
		 */
        public PdfName StringFilterName
        {
            get => Dictionary.Get<PdfName>(PdfName.StrF, PdfName.Identity);
            set => Dictionary[PdfName.StrF] = value;
        }

        /**
		 * Get the Perms entry in the encryption Dictionary.
		 *
		 * @return A 16 byte array or null if there is no Perms entry.
		 *
		 * @throws IOException If there is an error accessing the data.
		 */
        /**
		 * Set the Perms entry in the encryption Dictionary.
		 *
		 * @param perms A 16 byte array.
		 *
		 * @throws IOException If there is an error setting the data.
		 */
        public Memory<byte> Perms
        {
            get => Dictionary.GetTextBytes(PdfName.Perms);
            set => Dictionary[PdfName.Perms] = new PdfString(value);
        }


        /**
		 * remove CF, StmF, and StrF entries. This is to be called if V is not 4 or 5.
		 */
        public void RemoveV45filters()
        {
            Dictionary[PdfName.CF] = null;
            Dictionary[PdfName.StmF] = null;
            Dictionary[PdfName.StrF] = null;
        }
    }
}