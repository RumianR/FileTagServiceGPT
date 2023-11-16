using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using IronOcr;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using OpenAIApp.Common;

namespace OpenAIApp.Helpers.Files
{
    public static class PdfHelper
    {
        public static async Task<FileMetadata> GetFileMetadataAsync(string url, int maxPages = int.MaxValue)
        {
            string tempFilePath = System.IO.Path.GetTempFileName();
            string extractedText = string.Empty;
            long lengthOfFileInBytes = 0;
            int numberOfPages = 0;

            try
            {
                // Download the PDF file to a temporary file asynchronously
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fs);
                        lengthOfFileInBytes = fs.Length;
                    }
                }

                // Extract text from the PDF file
                extractedText = ExtractTextFromPdf(tempFilePath, maxPages);
                numberOfPages = GetNumberOfPages(tempFilePath);

            }
            finally
            {
                // Clean up: Delete the temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return new FileMetadata
            {
                FileContentText = extractedText,
                FileLengthInBytes = lengthOfFileInBytes,
                NumberOfPages = numberOfPages
            };
        }

        public static string ExtractTextFromPdf(string filePath, int maxPages)
        {
            //using (PdfReader reader = new PdfReader(filePath))
            //{
            //    StringWriter output = new StringWriter();

            //    int numberOfPages = Math.Min(reader.NumberOfPages, maxPages);
            //    for (int i = 1; i <= numberOfPages; i++)
            //    {
            //        output.WriteLine(
            //            PdfTextExtractor.GetTextFromPage(
            //                reader,
            //                i,
            //                new SimpleTextExtractionStrategy()
            //            )
            //        );
            //    }

            //    return output.ToString();
            //}

            var ocr = new IronTesseract();

            using (var ocrInput = new OcrInput())
            {
                ocrInput.AddPdf(filePath);

                // Optionally Apply Filters if needed:
                // ocrInput.Deskew();  // use only if image not straight
                // ocrInput.DeNoise(); // use only if image contains digital noise

                var ocrResult = ocr.Read(ocrInput);
                return ocrResult.Text;
            }
        }

        public static int GetNumberOfPages(string filePath)
        {
            using (PdfReader reader = new PdfReader(filePath))
            {

                return reader.NumberOfPages;
            }

        }
    }
}
