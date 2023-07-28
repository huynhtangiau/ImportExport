
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using ImportExport.Core.Models;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Colorspace;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImportExport.CrossCutting.Utils.Helpers
{
    public static class PdfHelper
    {
        public static string ReadPdfContent(this string path)
        {
            var text = new StringBuilder();

            if (File.Exists(path))
            {
                var pdfDocument = new PdfDocument(new PdfReader(path));
                for (int pageIndex = 1; pageIndex <= pdfDocument.GetNumberOfPages(); pageIndex++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var page = pdfDocument.GetPage(pageIndex);
                    var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
            }
            return text.ToString();
        }
        public static string ReadPdfContent(this byte[] bytes)
        {
            var text = new StringBuilder();
            var pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(bytes)));
            for (int pageIndex = 1; pageIndex <= pdfDocument.GetNumberOfPages(); pageIndex++)
            {
                var strategy = new LocationTextExtractionStrategy();
                var page = pdfDocument.GetPage(pageIndex);
                var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);

                currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                text.Append(currentText);
            }
            return text.ToString();
        }
        public static PdfDocument GetPdfDocument(this byte[] bytes)
        {
            return new PdfDocument(new PdfReader(new MemoryStream(bytes)));
        }
        public static void AddTextToPdf(this string inputPdfPath, string outputPdfPath, string textToAdd, int x)
        {
            var pdfDocument = new PdfDocument(new PdfReader(inputPdfPath));
            var pdf = new PdfDocument(new PdfWriter(outputPdfPath));

            pdfDocument.CopyPagesTo(1, 1, pdf, 1);

            var d = new Document(pdf);

            var y = pdf.GetFirstPage().GetMediaBox().GetHeight() - 80;
            var p = new Paragraph(textToAdd).SetFontSize(14);
            p.SetFixedPosition(1, x, y, 200);
            d.Add(p);

            pdf.Close();
        }
        public static KeyValuePair<string, List<LocationModel>> FindLocations(this PdfDocument pdfDocument, string search, string key, string value)
        {
            var locationResult = new List<LocationModel>();
            for (int pageIndex = 1; pageIndex <= pdfDocument.GetNumberOfPages(); pageIndex++)
            {
                var page = pdfDocument.GetPage(pageIndex);
                var strategy = new RegexBasedLocationExtractionStrategy(search);
                var parser = new PdfCanvasProcessor(strategy);
                parser.ProcessPageContent(page);

                var locations = strategy.GetResultantLocations()
                    .Select(s =>
                    {
                        var rectangle = s.GetRectangle();
                        return new LocationModel()
                        {
                            Height = rectangle.GetHeight(),
                            Width = rectangle.GetWidth(),
                            PageIndex = pageIndex,
                            X = rectangle.GetX(),
                            Y = rectangle.GetY(),
                            Key = key,
                            Value = value
                        };
                    }).ToList();
                if (locations.Any())
                {
                    locationResult.AddRange(locations);
                } 
            }
            return new KeyValuePair<string, List<LocationModel>>(key, locationResult);
        }

        public static void WriteTextsByLocationToPdf(this PdfDocument pdfDocument, string outputPdfPath, Dictionary<string, List<LocationModel>> texts)
        {
            var pdf = new PdfDocument(new PdfWriter(outputPdfPath));

            pdfDocument.CopyPagesTo(1, pdfDocument.GetNumberOfPages(), pdf, 1);
            var items = texts.Values.SelectMany(s => s).ToList();


            for (int pageNum = 1; pageNum <= pdf.GetNumberOfPages(); pageNum++)
            {
                var pageItems = items.Where(q => q.PageIndex == pageNum).ToList();
                var page = pdf.GetPage(pageNum);
                var canvas = new PdfCanvas(page);
                foreach (var item in pageItems)
                {
                    var newLine = item.Y - item.Height;
                    var maxWidth = page.GetCropBox().GetWidth() - item.X;
                    var values = item.Value.EnumByNearestSpace(35);
                    var line = values.Count() + 1;
                    var heightLines = (item.Height * line) - 5;
                    canvas
                        .SetFillColorRgb(255, 255, 255)
                        .Fill()
                        .Rectangle(item.X - 5, item.Y - heightLines, maxWidth - 20, heightLines)
                        .SetStrokeColorRgb(255, 0, 0)
                        .FillStroke();
                    foreach (var value in values)
                    {
                        canvas.SaveState()
                               .BeginText()
                               .MoveTextWithLeading(item.X, newLine)
                               .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLDITALIC), 7)
                               .SetFillColorRgb(255, 0, 0)
                               .ShowText(value)
                               .EndText()
                        .RestoreState();
                        newLine = newLine - item.Height;
                    }
                }
                canvas.Release();

            }


            pdf.Close();
        }
        public static string ReadSignatureContent(this string path)
        {
            var sb = new StringBuilder();

            var reader = new PdfReader(path);
            var pdfDocument = new PdfDocument(reader);
            var signatureUtil = new SignatureUtil(pdfDocument);

            var acroForm = PdfAcroForm.GetAcroForm(pdfDocument, false);

            foreach (string name in signatureUtil.GetSignatureNames())
            {
                var signatorySignature = acroForm.GetField(name);
                var appearanceDic = signatorySignature.GetPdfObject().GetAsDictionary(PdfName.AP);

                var pdfStream = appearanceDic.GetAsStream(PdfName.N);

                var strategy = new LocationTextExtractionStrategy();
                var resourcesDic = pdfStream.GetAsDictionary(PdfName.Resources);
                var processor = new PdfCanvasProcessor(strategy);
                processor.ProcessContent(pdfStream.GetBytes(), new PdfResources(resourcesDic));
                sb.Append(strategy.GetResultantText());
            }
            return sb.ToString();
        }
        public static bool MergePDFs(this IEnumerable<string> fileNames, string targetPdf)
        {
            var pdf = new PdfDocument(new PdfWriter(targetPdf));
            var merger = new PdfMerger(pdf);
            foreach(var fileName in fileNames)
            {
                var sourcePdf = new PdfDocument(new PdfReader(fileName));
                merger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
                sourcePdf.Close();
            }
            pdf.Close();
            return true;
        }
    }
}
