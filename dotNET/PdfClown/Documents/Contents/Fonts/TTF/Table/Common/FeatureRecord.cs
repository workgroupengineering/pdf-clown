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

namespace PdfClown.Documents.Contents.Fonts.TTF.Table.Common
{

    /**
     * This class models the
     * <a href="https://docs.microsoft.com/en-us/typography/opentype/spec/chapter2#features-and-lookups">Features and
     * lookups</a> in the Open Type layout common tables.
     * 
     * @author Palash Ray
     *
     */
    public class FeatureRecord
    {
        private readonly string featureTag;
        private readonly FeatureTable featureTable;

        public FeatureRecord(string featureTag, FeatureTable featureTable)
        {
            this.featureTag = featureTag;
            this.featureTable = featureTable;
        }

        public string FeatureTag
        {
            get => featureTag;
        }

        public FeatureTable FeatureTable
        {
            get => featureTable;
        }

        public override string ToString()
        {
            return $"FeatureRecord[featureTag={featureTag}]";
        }
    }
}