/*
 * https://github.com/apache/pdfbox
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
using System;
using System.Collections.Generic;

namespace PdfClown.Documents.Contents.Fonts.CCF
{
    /**
     * A CFF charset. A charset is an array of SIDs/CIDs for all glyphs in the font.
     *
     * todo: split this into two? CFFCharsetType1 and CFFCharsetCID ?
     *
     * @author John Hewson
     */
    public abstract class CFFCharset
    {
        public abstract bool IsCIDFont { get; }

        /**
         * Adds a new GID/SID/name combination to the charset.
         *
         * @param gid GID
         * @param sid SID
         */
        public abstract void AddSID(int gid, int sid, string name);

        /**
         * Adds a new GID/CID combination to the charset.
         *
         * @param gid GID
         * @param cid CID
         */
        public abstract void AddCID(int gid, int cid);

        /**
         * Returns the SID for a given GID. SIDs are internal to the font and are not public.
         *
         * @param sid GID
         * @return SID
         */
        public abstract int GetSIDForGID(int gid);

        /**
         * Returns the GID for the given SID. SIDs are internal to the font and are not public.
         *
         * @param sid SID
         * @return GID
         */
        public abstract int GetGIDForSID(int sid);

        /**
         * Returns the GID for a given CID. Returns 0 if the CID is missing.
         *
         * @param cid CID
         * @return GID
         */
        public abstract int GetGIDForCID(int cid);

        /**
         * Returns the SID for a given PostScript name, you would think this is not needed,
         * but some fonts have glyphs beyond their encoding with charset SID names.
         *
         * @param name PostScript glyph name
         * @return SID
         */
        public abstract int GetSID(string name);

        /**
         * Returns the PostScript glyph name for the given GID.
         *
         * @param gid GID
         * @return PostScript glyph name
         */
        public abstract string GetNameForGID(int gid);

        /**
         * Returns the CID for the given GID.
         *
         * @param gid GID
         * @return CID
         */
        public abstract int GetCIDForGID(int gid);
    }
}