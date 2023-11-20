using System.Drawing;
using System.Drawing.Imaging;
using Ghostscript.NET.Rasterizer;
using IronOcr;
using iTextSharp.text.pdf;
using OpenAIApp.Common;

namespace OpenAIApp.Helpers.Files
{
    public static class PdfHelper
    {
        public static async Task<FileMetadata> GetFileMetadataAsync(string url, Guid fileId, int maxPages = int.MaxValue)
        {
            string tempFilePath = System.IO.Path.GetTempFileName().Replace(".tmp", ".pdf");

            string extractedText = string.Empty;
            long lengthOfFileInBytes = 0;
            int numberOfPages = 0;
            var thumbnailFilePath = tempFilePath.Replace(".pdf", string.Empty) + $"{fileId}_thumbnail.png";

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
                CreateThumbnail(tempFilePath, thumbnailFilePath, 300);
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
                NumberOfPages = numberOfPages,
                TempPathToThumbnail = thumbnailFilePath
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

        public static void CreateThumbnail(string pdfPath, string thumbnailPath, int dpi)
        {
            using (var rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(pdfPath);

                // Assuming you want the first page
                var pdfPageImage = rasterizer.GetPage(dpi, 1);

                //// You can resize the image here if needed
                //var thumbnail = ResizeImage(pdfPageImage, new System.Drawing.Size(1024, 1463)); // Example size

                pdfPageImage.Save(thumbnailPath, ImageFormat.Png); //ONLY SUPPORTED ON WINDOWS
            }
        }

        private static System.Drawing.Image ResizeImage(System.Drawing.Image image, System.Drawing.Size size)
        {
            var resizedImage = new Bitmap(size.Width, size.Height);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.DrawImage(image, 0, 0, size.Width, size.Height); //ONLY SUPPORTED ON WINDOWS
            }
            return resizedImage;
        }

        public static byte[] ConvertToBase64(string imagePath)
        {
            try
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        // Convert Image to byte[]
                        image.Save(m, ImageFormat.Png);
                        byte[] imageBytes = m.ToArray();

                        return imageBytes;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

    }
}
