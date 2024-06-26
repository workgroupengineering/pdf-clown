using PdfClown.Documents;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Entities;
using PdfClown.Documents.Contents.Fonts;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Forms;
using PdfClown.Files;

using System;
using System.Collections.Generic;
using SkiaSharp;

namespace PdfClown.Samples.CLI
{
    /**
      <summary>This sample demonstrates how to inspect the AcroForm fields of a PDF document.</summary>
    */
    public class AcroFormParsingSample
      : Sample
    {
        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            string filePath = PromptFileChoice("Please select a PDF file");
            using (var file = new PdfFile(filePath))
            {
                PdfDocument document = file.Document;

                // 2. Get the acroform!
                Form form = document.Form;
                if (!form.Exists())
                { Console.WriteLine("\nNo acroform available (AcroForm dictionary not found)."); }
                else
                {
                    Console.WriteLine("\nIterating through the fields collection...\n");

                    // 3. Showing the acroform fields...
                    Dictionary<string, int> objCounters = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (Field field in form.Fields.Values)
                    {
                        Console.WriteLine("* Field '" + field.FullName + "' (" + field.BaseObject + ")");

                        string typeName = field.GetType().Name;
                        Console.WriteLine("    Type: " + typeName);
                        Console.WriteLine("    Value: " + field.Value);
                        Console.WriteLine("    Data: " + field.BaseDataObject.ToString());

                        int widgetIndex = 0;
                        foreach (Widget widget in field.Widgets)
                        {
                            Console.WriteLine("    Widget " + (++widgetIndex) + ":");
                            var widgetPage = widget.Page;
                            Console.WriteLine("      Page: " + (widgetPage == null ? "undefined" : widgetPage.Number + " (" + widgetPage.BaseObject + ")"));

                            SKRect widgetBox = widget.GetViewBounds();
                            Console.WriteLine("      Coordinates: {x:" + Math.Round(widgetBox.Left) + "; y:" + Math.Round(widgetBox.Top) + "; width:" + Math.Round(widgetBox.Width) + "; height:" + Math.Round(widgetBox.Height) + "}");
                        }

                        objCounters[typeName] = (objCounters.ContainsKey(typeName) ? objCounters[typeName] : 0) + 1;
                    }

                    int fieldCount = form.Fields.Count;
                    if (fieldCount == 0)
                    { Console.WriteLine("No field available."); }
                    else
                    {
                        Console.WriteLine("\nFields partial counts (grouped by type):");
                        foreach (KeyValuePair<string, int> entry in objCounters)
                        { Console.WriteLine(" " + entry.Key + ": " + entry.Value); }
                        Console.WriteLine("Fields total count: " + fieldCount);
                    }
                }
            }
        }
    }
}