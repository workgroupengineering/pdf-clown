using PdfClown.Documents;
using PdfClown.Documents.Contents.Scanner;
using PdfClown.Tools;
using System;
using System.Collections.Generic;

namespace PdfClown.Samples.CLI
{
    /// <summary>This sample demonstrates how to retrieve text content along with its graphic attributes
    /// (font, font size, text color, text rendering mode, text bounding box, and so on) from a PDF document;
    /// text is automatically sorted and aggregated.</summary>


    public class AdvancedTextExtractionSample : Sample
    {
        public override void Run()
        {
            // 1. Opening the PDF file...
            string filePath = PromptFileChoice("Please select a PDF file");
            using (var file = new PdfFile(filePath))
            {
                PdfDocument document = file.Document;

                // 2. Text extraction from the document pages.
                var extractor = new TextExtractor();
                foreach (var page in document.Pages)
                {
                    if (!PromptNextPage(page, false))
                    {
                        Quit();
                        break;
                    }

                    IList<ITextString> textStrings = extractor.Extract(page)[TextExtractor.DefaultArea];
                    foreach (ITextString textString in textStrings)
                    {
                        var textStringQuad = textString.Quad;
                        Console.WriteLine(
                          $"Text [x:{Math.Round(textStringQuad.MinX)},y:{Math.Round(textStringQuad.MinY)},w:{Math.Round(textStringQuad.Width)},h:{Math.Round(textStringQuad.Height)}]: {textString.Text}");
                    }
                }
            }
        }
    }
}