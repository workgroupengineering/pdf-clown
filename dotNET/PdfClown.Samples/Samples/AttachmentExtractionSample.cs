using PdfClown.Bytes;
using PdfClown.Documents;
using PdfClown.Documents.Files;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Files;
using PdfClown.Objects;

using System;
using System.Collections.Generic;
using System.IO;

namespace PdfClown.Samples.CLI
{
    /**
      <summary>This sample demonstrates how to extract attachments from a PDF document.</summary>
    */
    public class AttachmentExtractionSample : Sample
    {
        public override void Run()
        {
            // 1. Opening the PDF file...
            string filePath = PromptFileChoice("Please select a PDF file");
            using (var file = new PdfFile(filePath))
            {
                var document = file.Document;

                // 2. Extracting attachments...
                // 2.1. Embedded files (document level).
                foreach (KeyValuePair<PdfString, FileSpecification> entry in document.Names.EmbeddedFiles)
                { EvaluateDataFile(entry.Value); }

                // 2.2. File attachments (page level).
                foreach (var page in document.Pages)
                {
                    foreach (Annotation annotation in page.Annotations)
                    {
                        if (annotation is FileAttachment)
                        { EvaluateDataFile(((FileAttachment)annotation).DataFile); }
                    }
                }
            }
        }

        private void EvaluateDataFile(FileSpecification dataFile)
        {
            if (dataFile is FullFileSpecification)
            {
                EmbeddedFile embeddedFile = ((FullFileSpecification)dataFile).EmbeddedFile;
                if (embeddedFile != null)
                { ExportAttachment(embeddedFile.Data, dataFile.Path); }
            }
        }

        private void ExportAttachment(IInputStream data, string filename)
        {
            string outputPath = GetOutputPath(filename);
            FileStream outputStream;
            try
            { outputStream = new FileStream(outputPath, FileMode.CreateNew); }
            catch (Exception e)
            { throw new Exception(outputPath + " file couldn't be created.", e); }

            try
            {
                BinaryWriter writer = new BinaryWriter(outputStream);
                writer.Write(data.ToArray());
                writer.Close();
                outputStream.Close();
            }
            catch (Exception e)
            { throw new Exception(outputPath + " file writing has failed.", e); }

            Console.WriteLine("Output: " + outputPath);
        }
    }
}