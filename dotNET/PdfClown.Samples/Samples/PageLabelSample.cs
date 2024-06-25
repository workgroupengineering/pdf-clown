using PdfClown.Documents;
using PdfClown.Documents.Interaction.Navigation;
using PdfClown.Objects;

using System;
using System.Collections.Generic;

namespace PdfClown.Samples.CLI
{
    /// <summary>This sample demonstrates how to define, read and modify page labels.</summary>
    public class PageLabelSample : Sample
    {
        public override void Run()
        {
            string outputFilePath;
            {
                // 1. Opening the PDF file...
                string filePath = PromptFileChoice("Please select a PDF file");
                using (var file = new PdfFile(filePath))
                {
                    PdfDocument document = file.Document;

                    // 2. Defining the page labels...
                    PageLabels pageLabels = document.PageLabels;
                    pageLabels.Clear();
                    /*
                      NOTE: This sample applies labels to arbitrary page ranges: no sensible connection with their
                      actual content has therefore to be expected.
                    */
                    int pageCount = document.Pages.Count;
                    pageLabels[PdfInteger.Get(0)] = new PageLabel(document, "Introduction ", PageLabel.NumberStyleEnum.UCaseRomanNumber, 5);
                    if (pageCount > 3)
                    { pageLabels[PdfInteger.Get(3)] = new PageLabel(document, PageLabel.NumberStyleEnum.UCaseLetter); }
                    if (pageCount > 6)
                    { pageLabels[PdfInteger.Get(6)] = new PageLabel(document, "Contents ", PageLabel.NumberStyleEnum.ArabicNumber, 0); }

                    // 3. Serialize the PDF file!
                    outputFilePath = Serialize(file, "Page labelling", "labelling a document's pages", "page labels");
                }
            }

            {
                using (var file = new PdfFile(outputFilePath))
                {
                    foreach (KeyValuePair<PdfInteger, PageLabel> entry in file.Document.PageLabels)
                    {
                        Console.WriteLine("Page label " + entry.Value.BaseObject);
                        Console.WriteLine("    Initial page: " + (entry.Key.IntValue + 1));
                        Console.WriteLine("    Prefix: " + (entry.Value.Prefix));
                        Console.WriteLine("    Number style: " + (entry.Value.NumberStyle));
                        Console.WriteLine("    Number base: " + (entry.Value.NumberBase));
                    }
                }
            }
        }
    }
}