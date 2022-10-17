using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.CrossCutting.Utils.Helpers
{
    public static class PdfHelper
    {
        public static string ReadPdfContent(this string path)
        {
            StringBuilder text = new StringBuilder();

            if (System.IO.File.Exists(path))
            {
                PdfReader pdfReader = new PdfReader(path);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }
        public static void AddTextToPdf(this string inputPdfPath, string outputPdfPath, string textToAdd, System.Drawing.Point point)
        {
            //variables
            string pathin = inputPdfPath;
            string pathout = outputPdfPath;

            //create PdfReader object to read from the existing document
            using (PdfReader reader = new PdfReader(pathin))
            //create PdfStamper object to write to get the pages from reader 
            using (PdfStamper stamper = new PdfStamper(reader, new FileStream(pathout, FileMode.Create)))
            {
                //select two pages from the original document
                reader.SelectPages("1-2");

                //gettins the page size in order to substract from the iTextSharp coordinates
                var pageSize = reader.GetPageSize(1);

                // PdfContentByte from stamper to add content to the pages over the original content
                PdfContentByte pbover = stamper.GetOverContent(1);

                //add content to the page using ColumnText
                Font font = new Font();
                font.Size = 14;

                //setting up the X and Y coordinates of the document
                int x = point.X;
                int y = point.Y;

                y = (int)(pageSize.Height - y);

                ColumnText.ShowTextAligned(pbover, Element.ALIGN_CENTER, new Phrase(textToAdd, font), x, y, 0);
            }
        }
    }
}
