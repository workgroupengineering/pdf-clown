using PdfClown.Documents;
using PdfClown.Documents.Contents;
using PdfClown.Documents.Contents.Fonts;
using PdfClown.Documents.Contents.Objects;

using System;

namespace PdfClown.Samples.CLI
{
    /// <summary>This sample demonstrates the low-level way to extract text from a PDF document.</summary>
    /// <remarks>In order to obtain richer information about the extracted text content,
    /// see the other available samples (<see cref="TextInfoExtractionSample"/>,
    /// <see cref="AdvancedTextExtractionSample"/>).</remarks>
    public class BasicTextExtractionSample : Sample
    {
        public override void Run()
        {
            // 1. Opening the PDF file...
            string filePath = PromptFileChoice("Please select a PDF file");
            using (var file = new PdfFile(filePath))
            {
                PdfDocument document = file.Document;

                // 2. Text extraction from the document pages.
                foreach (var page in document.Pages)
                {
                    if (!PromptNextPage(page, false))
                    {
                        Quit();
                        break;
                    }

                    // Wraps the page contents into a scanner.
                    Extract(new ContentScanner(page));
                }
            }
        }

        /// <summary>Scans a content level looking for text.</summary>
        // NOTE: Page contents are represented by a sequence of content objects,
        // possibly nested into multiple levels.
        private void Extract(ContentScanner level)
        {
            if (level == null)
                return;

            while (level.MoveNext())
            {
                ContentObject content = level.Current;
                if (content is ShowText showText)
                {
                    Font font = level.State.Font;
                    // Extract the current text chunk, decoding it!
                    Console.WriteLine(font.Decode(showText.Text));
                }
                else if (content is GraphicsText || content is ContainerObject)
                {
                    // Scan the inner level!
                    Extract(level.ChildLevel);
                }
            }
        }
    }
}