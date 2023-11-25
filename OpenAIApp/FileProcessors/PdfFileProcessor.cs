using Ghostscript.NET.Rasterizer;
using iTextSharp.text.pdf;
using OpenAIApp.Common;
using OpenAIApp.Managers;
using System.Drawing.Imaging;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using Image = System.Drawing.Image;


namespace OpenAIApp.FileProcessors
{
    public class PdfFileProcessor
    {
        public ILogger<PdfFileProcessor> _logger;

        public ITesseractManager _tesseractManager;

        public PdfFileProcessor(ILogger<PdfFileProcessor> logger, ITesseractManager tesseractManager)
        {
            _logger = logger;
            _tesseractManager = tesseractManager;
        }

        public async Task<FileMetadata> GetFileMetadataAsync(string url, Guid fileId, int maxPages = int.MaxValue)
        {
            string tempFilePath = System.IO.Path.GetTempFileName().Replace(".tmp", ".pdf");

            string extractedText = string.Empty;
            long lengthOfFileInBytes = 0;
            int numberOfPages = 0;
            var thumbnailBase64 = string.Empty;

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

                numberOfPages = GetNumberOfPages(tempFilePath);

                extractedText = await ExtractTextFromPdf(tempFilePath, numberOfPages);

                thumbnailBase64 = GetBase64Thumbnail(tempFilePath, 50);
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
                ThumbnailBase64 = thumbnailBase64,
                FileType = Enums.FileType.PDF
            };
        }

        private async Task<string> ExtractTextFromPdf(string tempFilePath, int totalNumberOfPages)
        {
            var text = string.Empty;
            var dpi = 300;

            for (var pageIndex = 1; pageIndex <= totalNumberOfPages; pageIndex++)
            {
                var image = GetImage(tempFilePath, dpi, pageIndex);

                if (image == null)
                {
                    _logger.LogDebug($"Image is null for page {pageIndex} file: {tempFilePath}");
                    continue;
                }

                var pageText = await _tesseractManager.ExtractTextFromImageAsync(image);

                if (string.IsNullOrWhiteSpace(pageText))
                {
                    _logger.LogDebug($"Text is empty for page {pageIndex} file: {tempFilePath}");
                    continue;
                }

                text += pageText;
            }

            return text;
        }

        public int GetNumberOfPages(string filePath)
        {
            using (PdfReader reader = new PdfReader(filePath))
            {

                return reader.NumberOfPages;
            }

        }

        public string GetBase64Thumbnail(string pdfPath, int dpi)
        {
            try
            {
                var image = GetImage(pdfPath, dpi, 1);
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Position = 0;
                    image.Save(memoryStream, ImageFormat.Png);

                    // Convert the MemoryStream to a byte array
                    byte[] imageBytes = memoryStream.ToArray();

                    // Convert the byte array to a Base64 string
                    string base64String = Convert.ToBase64String(imageBytes);

                    return base64String;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not create base 64 thumbnail error: {ex.Message}");
            }

            return string.Empty;
        }

        public Image GetImage(string pdfPath, int dpi, int page)
        {
            //using (var rasterizer = new GhostscriptRasterizer())
            //{
            //    rasterizer.Open(pdfPath);

            //    return rasterizer.GetPage(dpi, page);
            //}

            try
            {
                using (var rasterizer = new GhostscriptRasterizer())
                {
                    rasterizer.Open(pdfPath);

                    return rasterizer.GetPage(dpi, page);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not get image from pdf error: {ex.Message}");
                return null;
            }
        }
    }
}
