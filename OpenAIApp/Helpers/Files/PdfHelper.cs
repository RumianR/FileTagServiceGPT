using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace OpenAIApp.Helpers.Files
{
    public static class PdfHelper
    {
        public static async Task<string> GetTextAsync(string url, int maxPages = int.MaxValue)
        {
            string tempFilePath = System.IO.Path.GetTempFileName();
            string extractedText;

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
                    }
                }

                // Extract text from the PDF file
                extractedText = ExtractTextFromPdf(tempFilePath, maxPages);
            }
            finally
            {
                // Clean up: Delete the temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return extractedText;
        }

        private static string ExtractTextFromPdf(string filePath, int maxPages)
        {
            using (PdfReader reader = new PdfReader(filePath))
            {
                StringWriter output = new StringWriter();

                int numberOfPages = Math.Min(reader.NumberOfPages, maxPages);
                for (int i = 1; i <= numberOfPages; i++)
                {
                    output.WriteLine(
                        PdfTextExtractor.GetTextFromPage(
                            reader,
                            i,
                            new SimpleTextExtractionStrategy()
                        )
                    );
                }

                return output.ToString();
            }
        }
    }
}
