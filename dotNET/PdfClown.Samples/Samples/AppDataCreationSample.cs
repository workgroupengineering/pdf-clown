using PdfClown.Bytes;
using PdfClown.Documents;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Fonts;
using PdfClown.Documents.Interchange.Metadata;
using PdfClown.Objects;

using SkiaSharp;

namespace PdfClown.Samples.CLI
{
    /// <summary>This sample demonstrates how to decorate documents and contents with private
    /// application data (aka page-piece application data dictionary).</summary>
    public class AppDataCreationSample : Sample
    {
        private static PdfName MyAppName = PdfName.Get(typeof(AppDataCreationSample).Name);

        public override void Run()
        {
            // 1. Instantiate a new PDF file!
            var file = new PdfFile();
            var document = file.Document;

            // 2.1. Page-level private application data.
            {
                var page = new PdfPage(document);
                document.Pages.Add(page);

                AppData myAppData = page.GetAppData(MyAppName);
                /*
                  NOTE: Applications are free to define whatever structure their private data should have. In
                  this example, we chose a PdfDictionary populating it with arbitrary entries, including a
                  byte stream.
                */
                var myStream = new PdfStream(new ByteStream("This is just some random characters to feed the stream..."));
                myAppData.Data = new PdfDictionary(2)
                {
                    { PdfName.Get("MyPrivateEntry"), PdfBoolean.True },
                    { PdfName.Get("MyStreamEntry"), file.Register(myStream)}
                };

                // Add some (arbitrary) graphics content on the page!
                BlockComposer composer = new BlockComposer(new PrimitiveComposer(page));
                composer.BaseComposer.SetFont(FontType1.Load(document, FontName.TimesBold), 14);
                SKSize pageSize = page.Size;
                composer.Begin(SKRect.Create(50, 50, pageSize.Width - 100, pageSize.Height - 100), XAlignmentEnum.Left, YAlignmentEnum.Top);
                composer.ShowText("This page holds private application data (see PieceInfo entry in its dictionary).");
                composer.End();
                composer.BaseComposer.Flush();
            }

            // 2.2. Document-level private application data.
            {
                AppData myAppData = document.GetAppData(MyAppName);
                /*
                  NOTE: Applications are free to define whatever structure their private data should have. In
                  this example, we chose a PdfDictionary populating it with arbitrary entries.
                */
                myAppData.Data = new PdfDictionary()
                {
                    { PdfName.Get("MyPrivateDocEntry"), new PdfTextString("This is an arbitrary value") },
                    { PdfName.Get("AnotherPrivateEntry"), new PdfDictionary
                    {
                        { PdfName.Get("SubEntry"), 1287 },
                        { PdfName.Get("SomeData"), new PdfArray(2) { 282.773, 14.28378} }
                    }}
                };
            }

            // 3. Serialize the PDF file!
            Serialize(file, "Private application data", "editing private application data", "Page-Piece Dictionaries, private application data, metadata");
        }
    }
}
