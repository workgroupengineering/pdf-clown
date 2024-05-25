/*
  Copyright 2009-2015 Stefano Chizzolini. http://www.pdfclown.org

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

using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfClown.Documents.Contents.Objects
{
    /**
      <summary>'Move to the next line and show a text string' operation [PDF:1.6:5.3.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class ShowTextToNextLine : ShowText
    {
        /**
          <param name="text">Text encoded using current font's encoding.</param>
        */
        public ShowTextToNextLine(string @operator, byte[] text)
            : base(@operator, new PdfByteString(text))
        { }

        /**
          <param name="text">Text encoded using current font's encoding.</param>
          <param name="wordSpace">Word spacing.</param>
          <param name="charSpace">Character spacing.</param>
        */
        public ShowTextToNextLine(string @operator, byte[] text, double wordSpace, double charSpace)
            : base(@operator, PdfReal.Get(wordSpace), PdfReal.Get(charSpace), new PdfByteString(text))
        { }

        public ShowTextToNextLine(string @operator, IList<PdfDirectObject> operands)
            : base(@operator, operands)
        { }

        /**
          <summary>Gets/Sets the character spacing.</summary>
        */
        public abstract float? CharSpace { get; set; }

        /**
          <summary>Gets/Sets the word spacing.</summary>
        */
        public abstract float? WordSpace { get; set; }

        protected abstract PdfString String { get; set; }

        public override Memory<byte> Text
        {
            get => String.RawValue;
            set => String = new PdfByteString(value);
        }

        public override IEnumerable<PdfDirectObject> Value
        {
            get => Enumerable.Repeat(String, 1);
            set => String = value.FirstOrDefault() as PdfString;
        }
    }

    public sealed class ShowTextToNextLineNoSpace : ShowTextToNextLine
    {
        /**
          <summary>Specifies no text state parameter
          (just uses the current settings).</summary>
        */
        public static readonly string OperatorKeyword = "'";


        /**
          <param name="text">Text encoded using current font's encoding.</param>
        */
        public ShowTextToNextLineNoSpace(byte[] text)
            : base(OperatorKeyword, text)
        { }

        public ShowTextToNextLineNoSpace(IList<PdfDirectObject> operands)
            : base(OperatorKeyword, operands)
        { }

        /**
          <summary>Gets/Sets the character spacing.</summary>
        */
        override public float? CharSpace
        {
            get => null;
            set { }
        }

        /**
          <summary>Gets/Sets the word spacing.</summary>
        */
        public override float? WordSpace
        {
            get => null;
            set { }
        }

        protected override PdfString String
        {
            get => (PdfString)operands[0];
            set => operands[0] = value;
        }

    }


    public sealed class ShowTextToNextLineWithSpace : ShowTextToNextLine
    {
        /**
         <summary>Specifies the word spacing and the character spacing
         (setting the corresponding parameters in the text state).</summary>
       */
        public static readonly string OperatorKeyword = "''";

        /**
          <param name="text">Text encoded using current font's encoding.</param>
          <param name="wordSpace">Word spacing.</param>
          <param name="charSpace">Character spacing.</param>
        */
        public ShowTextToNextLineWithSpace(byte[] text, double wordSpace, double charSpace)
            : base(OperatorKeyword, text, wordSpace, charSpace)
        { }

        public ShowTextToNextLineWithSpace(IList<PdfDirectObject> operands)
            : base(OperatorKeyword, operands)
        { }

        /**
          <summary>Gets/Sets the character spacing.</summary>
        */
        public override float? CharSpace
        {
            get => ((IPdfNumber)operands[1]).FloatValue;
            set
            {
                EnsureSpaceOperation();
                operands[1] = PdfReal.Get(value.Value);
            }
        }

        protected override PdfString String
        {
            get => (PdfString)operands[2];
            set => operands[2] = value;
        }

        /**
          <summary>Gets/Sets the word spacing.</summary>
        */
        public override float? WordSpace
        {
            get => ((IPdfNumber)operands[0]).FloatValue;
            set
            {
                EnsureSpaceOperation();
                operands[0] = PdfReal.Get(value.Value);
            }
        }

        private void EnsureSpaceOperation()
        {
            if (!OperatorKeyword.Equals(@operator, StringComparison.Ordinal))
            {
                @operator = OperatorKeyword;
                operands.Insert(0, PdfReal.Get(0));
                operands.Insert(1, PdfReal.Get(0));
            }
        }
    }
}