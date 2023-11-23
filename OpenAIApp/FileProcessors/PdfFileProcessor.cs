using Ghostscript.NET.Rasterizer;
using iTextSharp.text.pdf;
using OpenAIApp.Common;
using OpenAIApp.Managers;
using System.Drawing.Imaging;
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

                numberOfPages = GetNumberOfPages(tempFilePath);

                extractedText = ExtractTextFromPdf(tempFilePath, numberOfPages);

                CreateThumbnail(tempFilePath, thumbnailFilePath, 100);
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

        private string ExtractTextFromPdf(string tempFilePath, int totalNumberOfPages)
        {
            var text = string.Empty;
            var dpi = 100;

            for (var pageIndex = 1; pageIndex <= totalNumberOfPages; pageIndex++)
            {
                var image = GetImage(tempFilePath, dpi, pageIndex);

                if (image == null)
                {
                    _logger.LogDebug($"Image is null for page {pageIndex} file: {tempFilePath}");
                    continue;
                }

                var pageText = _tesseractManager.ExtractTextFromImage(image);

                if (string.IsNullOrWhiteSpace(pageText))
                {
                    _logger.LogDebug($"Text is empty for page {pageIndex} file: {tempFilePath}");
                    continue;
                }

                text += pageIndex;
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

        public void CreateThumbnail(string pdfPath, string thumbnailPath, int dpi)
        {
            try
            {
                var image = GetImage(pdfPath, dpi, 1);
                image.Save(thumbnailPath, ImageFormat.Png); //ONLY SUPPORTED ON WINDOWS
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not create thumbnail error: {ex.Message}");
            }
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
