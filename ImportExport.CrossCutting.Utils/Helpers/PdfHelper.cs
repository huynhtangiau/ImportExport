
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
        public static string WriteTextAfterFoundPosition(PdfReader pdfReader, string search)
        {
            var text = new StringBuilder();
            return text.ToString();
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
