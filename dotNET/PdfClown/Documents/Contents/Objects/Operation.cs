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
using PdfClown.Files;
using PdfClown.Objects;
using PdfClown.Tokens;

using System;
using System.Collections.Generic;
using System.Text;

namespace PdfClown.Documents.Contents.Objects
{
    /**
      <summary>Content stream instruction [PDF:1.6:3.7.1].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class Operation : ContentObject
    {
        private static readonly Dictionary<string, Func<PdfArray, Operation>> cache = new(128, StringComparer.Ordinal)
        {
            { SaveGraphicsState.OperatorKeyword, (operands) => SaveGraphicsState.Value },
            { SetFont.OperatorKeyword, (operands) => new SetFont(operands) },
            { SetStrokeColorBase.OperatorKeyword, (operands) => new SetStrokeColorBase(operands) },
            { SetStrokeColorExtended.OperatorKeyword, (operands) => new SetStrokeColorExtended(operands) },
            { SetStrokeColorSpace.OperatorKeyword, (operands) => new SetStrokeColorSpace(operands) },
            { SetFillColorBase.OperatorKeyword, (operands) => new SetFillColorBase(operands) },
            { SetFillColorExtended.OperatorKeyword, (operands) => new SetFillColorExtended(operands) },
            { SetFillColorSpace.OperatorKeyword, (operands) => new SetFillColorSpace(operands) },
            { SetDeviceGrayStrokeColor.OperatorKeyword, (operands) => new SetDeviceGrayStrokeColor(operands) },
            { SetDeviceGrayFillColor.OperatorKeyword, (operands) => new SetDeviceGrayFillColor(operands) },
            { SetDeviceRGBStrokeColor.OperatorKeyword, (operands) => new SetDeviceRGBStrokeColor(operands) },
            { SetDeviceRGBFillColor.OperatorKeyword, (operands) => new SetDeviceRGBFillColor(operands) },
            { SetDeviceCMYKStrokeColor.OperatorKeyword, (operands) => new SetDeviceCMYKStrokeColor(operands) },
            { SetDeviceCMYKFillColor.OperatorKeyword, (operands) => new SetDeviceCMYKFillColor(operands) },
            { RestoreGraphicsState.OperatorKeyword, (operands) => RestoreGraphicsState.Value },
            { BeginSubpath.OperatorKeyword, (operands) => new BeginSubpath(operands) },
            { CloseSubpath.OperatorKeyword, (operands) => CloseSubpath.Value },
            { PaintPath.CloseStrokeOperatorKeyword, (operands) => PaintPath.CloseStroke },
            { PaintPath.FillOperatorKeyword, (operands) => PaintPath.Fill },
            { PaintPath.FillCompatibleOperatorKeyword, (operands) => PaintPath.Fill },
            { PaintPath.FillEvenOddOperatorKeyword, (operands) => PaintPath.FillEvenOdd },
            { PaintPath.StrokeOperatorKeyword, (operands) => PaintPath.Stroke },
            { PaintPath.FillStrokeOperatorKeyword, (operands) => PaintPath.FillStroke },
            { PaintPath.FillStrokeEvenOddOperatorKeyword, (operands) => PaintPath.FillStrokeEvenOdd },
            { PaintPath.CloseFillStrokeOperatorKeyword, (operands) => PaintPath.CloseFillStroke },
            { PaintPath.CloseFillStrokeEvenOddOperatorKeyword, (operands) => PaintPath.CloseFillStrokeEvenOdd },
            { PaintPath.EndPathNoOpOperatorKeyword, (operands) => PaintPath.EndPathNoOp },
            { ModifyClipPathNonZero.OperatorKeyword, (operands) => ModifyClipPath.NonZero },
            { ModifyClipPathEvenOdd.OperatorKeyword, (operands) => ModifyClipPath.EvenOdd },
            { TranslateTextToNextLine.OperatorKeyword, (operands) => TranslateTextToNextLine.Value },
            { ShowSimpleText.OperatorKeyword, (operands) => new ShowSimpleText(operands) },
            { ShowTextToNextLineNoSpace.OperatorKeyword, (operands) => new ShowTextToNextLineNoSpace(operands) },
            { ShowTextToNextLineWithSpace.OperatorKeyword, (operands) => new ShowTextToNextLineWithSpace(operands) },
            { ShowAdjustedText.OperatorKeyword, (operands) => new ShowAdjustedText(operands) },
            { TranslateTextRelativeNoLead.OperatorKeyword, (operands) => new TranslateTextRelativeNoLead(operands) },
            { TranslateTextRelativeWithLead.OperatorKeyword, (operands) => new TranslateTextRelativeWithLead(operands) },
            { SetTextMatrix.OperatorKeyword, (operands) => new SetTextMatrix(operands) },
            { ModifyCTM.OperatorKeyword, (operands) => new ModifyCTM(operands) },
            { PaintXObject.OperatorKeyword, (operands) => new PaintXObject(operands) },
            { PaintShading.OperatorKeyword, (operands) => new PaintShading(operands) },
            { SetCharSpace.OperatorKeyword, (operands) => new SetCharSpace(operands) },
            { SetLineCap.OperatorKeyword, (operands) => new SetLineCap(operands) },
            { SetLineDash.OperatorKeyword, (operands) => new SetLineDash(operands) },
            { SetLineJoin.OperatorKeyword, (operands) => new SetLineJoin(operands) },
            { SetLineWidth.OperatorKeyword, (operands) => new SetLineWidth(operands) },
            { SetMiterLimit.OperatorKeyword, (operands) => new SetMiterLimit(operands) },
            { SetTextLead.OperatorKeyword, (operands) => new SetTextLead(operands) },
            { SetTextRise.OperatorKeyword, (operands) => new SetTextRise(operands) },
            { SetTextScale.OperatorKeyword, (operands) => new SetTextScale(operands) },
            { SetTextRenderMode.OperatorKeyword, (operands) => new SetTextRenderMode(operands) },
            { SetWordSpace.OperatorKeyword, (operands) => new SetWordSpace(operands) },
            { DrawLine.OperatorKeyword, (operands) => new DrawLine(operands) },
            { DrawRectangle.OperatorKeyword, (operands) => new DrawRectangle(operands) },
            { DrawCurve.InitialOperatorKeyword, (operands) => new DrawInitialCurve(operands) },
            { DrawCurve.FullOperatorKeyword, (operands) => new DrawFullCurve(operands) },
            { DrawCurve.FinalOperatorKeyword, (operands) => new DrawFinalCurve(operands) },
            { BeginText.OperatorKeyword, (operands) => BeginText.Value },
            { EndText.OperatorKeyword, (operands) => EndText.Value },
            { BeginNamedMarkedContent.SimpleOperatorKeyword, (operands) => new BeginNamedMarkedContent(operands) },
            { BeginPropertyListMarkedContent.PropertyListOperatorKeyword, (operands) => new BeginPropertyListMarkedContent(operands) },
            { EndMarkedContent.OperatorKeyword, (operands) => EndMarkedContent.Value },
            { MarkedNamedContentPoint.OperatorKeyword, (operands) => new MarkedNamedContentPoint(operands) },
            { MarkedPropertyListContentPoint.OperatorKeyword, (operands) => new MarkedPropertyListContentPoint(operands) },
            { BeginInlineImage.OperatorKeyword, (operands) => BeginInlineImage.Value },
            { EndInlineImage.OperatorKeyword, (operands) => EndInlineImage.Value },
            { ApplyExtGState.OperatorKeyword, (operands) => new ApplyExtGState(operands) },
            { CharProcWidth.OperatorKeyword, (operands) => new CharProcWidth(operands) },
            { CharProcBBox.OperatorKeyword, (operands) => new CharProcBBox(operands) },
            { Flatness.OperatorKeyword, (operands) => new Flatness(operands) },
            { BeginCompatibilityState.OperatorKeyword, (operands) => BeginCompatibilityState.Value },
            { EndCompatibilityState.OperatorKeyword, (operands) => EndCompatibilityState.Value },
        };
        /**
          <summary>Gets an operation.</summary>
          <param name="@operator">Operator.</param>
          <param name="operands">List of operands.</param>
        */
        public static Operation Get(string @operator, PdfArray operands)
        {
            if (string.IsNullOrEmpty(@operator))
                return null;
            var str = @operator.ToString();
            if (cache.TryGetValue(str, out var func))
                return func(operands);
            else // No explicit operation implementation available.
                return new GenericOperation(@operator, operands);
        }

        protected string @operator;
        protected PdfArray operands;

        protected Operation(string @operator)
        { this.@operator = @operator; }

        protected Operation(string @operator, PdfDirectObject operand)
        {
            this.@operator = @operator;

            operands = new PdfArray(1);
            operands.AddDirect(operand);
        }

        protected Operation(string @operator, params PdfDirectObject[] operands)
        {
            this.@operator = @operator;
            this.operands = new PdfArray(operands.Length);
            this.operands.AddRangeDirect(operands);
        }

        protected Operation(string @operator, PdfArray operands)
        {
            this.@operator = @operator;
            this.operands = operands;
        }

        public string Operator => @operator;

        public IList<PdfDirectObject> Operands => operands;

        public override string ToString()
        {
            var buffer = new StringBuilder();

            // Begin.
            buffer.Append('{');

            // Operator.
            buffer.Append(@operator);

            // Operands.
            if (operands != null)
            {
                buffer.Append(' ').Append('[');
                for (int i = 0, count = operands.Count; i < count; i++)
                {
                    if (i > 0)
                    { buffer.Append(',').Append(' '); }

                    buffer.Append(operands[i].ToString());
                }
                buffer.Append(']');
            }

            // End.
            buffer.Append('}');

            return buffer.ToString();
        }

        public override void WriteTo(IOutputStream stream, PdfDocument context)
        {
            if (operands != null)
            {
                var fileContext = context.File;
                foreach (var operand in operands)
                { operand.WriteTo(stream, fileContext); stream.Write(Chunk.Space); }
            }
            stream.Write(@operator); stream.Write(Chunk.LineFeed);
        }
    }
}