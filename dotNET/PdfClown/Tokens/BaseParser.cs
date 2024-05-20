/*
  Copyright 2011-2015 Stefano Chizzolini. http://www.pdfclown.org

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
using PdfClown.Objects;
using PdfClown.Util.Parsers;

using System;
using System.IO;

namespace PdfClown.Tokens
{
    /**
      <summary>Base PDF parser [PDF:1.7:3.2].</summary>
    */
    public class BaseParser : PostScriptParser
    {
        protected BaseParser(IInputStream stream) : base(stream)
        { }

        protected BaseParser(Memory<byte> data) : base(data)
        { }

        public override bool MoveNext()
        {
            bool moved;
            while (moved = base.MoveNext())
            {
                TokenTypeEnum tokenType = TokenType;
                if (tokenType == TokenTypeEnum.Comment)
                    continue; // Comments are ignored.

                if (tokenType == TokenTypeEnum.Literal)
                {
                    var bytes = BytesToken;
                    if (bytes.Length > 5)
                    {
                        Span<char> chars = stackalloc char[2];
                        Charset.ISO88591.GetChars(bytes.Slice(0, 2), chars);
                        if (MemoryExtensions.Equals(chars, Keyword.DatePrefix, StringComparison.Ordinal))
                        {
                            TokenType = TokenTypeEnum.Date;
                            Span<char> full = stackalloc char[bytes.Length];
                            Charset.ISO88591.GetChars(bytes, full);
                            //NOTE: Dates are a weak extension to the PostScript language.                            
                            DateToken = PdfDate.ToDate(full, out var date) ? date : null;
                        }
                    }
                }
                break;
            }
            return moved;
        }

        /**
          <summary>Parses the current PDF object [PDF:1.6:3.2].</summary>
        */
        public virtual PdfDataObject ParsePdfObject()
        {
            switch (TokenType)
            {
                case TokenTypeEnum.Integer:
                    return PdfInteger.Get(IntegerToken);
                case TokenTypeEnum.Name:
                    return PdfName.Get(CharsToken.ToString(), true);
                case TokenTypeEnum.DictionaryBegin:
                    {
                        var dictionary = new PdfDictionary();
                        dictionary.Updateable = false;
                        while (true)
                        {
                            // Key.
                            MoveNext(); if (TokenType == TokenTypeEnum.DictionaryEnd) break;
                            var key = (PdfName)ParsePdfObject();
                            // Value.
                            MoveNext(); if (TokenType == TokenTypeEnum.DictionaryEnd) break;
                            PdfDirectObject value = (PdfDirectObject)ParsePdfObject();
                            // Add the current entry to the dictionary!
                            if (dictionary.ContainsKey(key))
                            {
                                key = PdfName.Get(key.StringValue + "Dublicat", true);
                            }
                            dictionary[key] = value;
                        }
                        dictionary.Updateable = true;
                        return dictionary;
                    }
                case TokenTypeEnum.ArrayBegin:
                    {
                        var array = new PdfArray();
                        array.Updateable = false;
                        while (true)
                        {
                            // Value.
                            MoveNext(); if (TokenType == TokenTypeEnum.ArrayEnd) break;
                            // Add the current item to the array!
                            array.Add((PdfDirectObject)ParsePdfObject());
                        }
                        array.Updateable = true;
                        return array;
                    }
                case TokenTypeEnum.Date:
                    return PdfDate.Get(BytesToken.ToArray(), DateToken);
                case TokenTypeEnum.Literal:
                    return new PdfTextString(BytesToken.ToArray(), PdfString.SerializationModeEnum.Literal);
                case TokenTypeEnum.Hex:
                    return new PdfTextString(BytesToken.ToArray(), PdfString.SerializationModeEnum.Hex);
                case TokenTypeEnum.Real:
                    return PdfReal.Get(RealToken);
                case TokenTypeEnum.Boolean:
                    return PdfBoolean.Get(BooleanToken);
                case TokenTypeEnum.Null:
                    return null;
                default:
                    throw new PostScriptParseException($"Unknown type beginning: '{Token}'", this);
            }
        }

        /**
          <summary>Parses a PDF object after moving to the given token offset.</summary>
          <param name="offset">Number of tokens to skip before reaching the intended one.</param>
          <seealso cref="ParsePdfObject()"/>
        */
        public PdfDataObject ParsePdfObject(int offset)
        {
            MoveNext(offset);
            return ParsePdfObject();
        }
    }
}

