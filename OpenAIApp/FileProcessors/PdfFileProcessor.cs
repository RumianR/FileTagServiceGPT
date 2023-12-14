using ImageMagick;
using iTextSharp.text.pdf;
using OpenAIApp.Common;
using OpenAIApp.Managers;


namespace OpenAIApp.FileProcessors
{
    public class PdfFileProcessor
    {
        public ILogger<PdfFileProcessor> _logger;

        public ITesseractManager _tesseractManager;

        private readonly int _maxCharacters = 2500 * 4;  // 4 characters per token


        public PdfFileProcessor(ILogger<PdfFileProcessor> logger, ITesseractManager tesseractManager)
        {
            _logger = logger;
            _tesseractManager = tesseractManager;
            //MagickNET.SetGhostscriptDirectory(@"/usr/lib/x86_64-linux-gnu");
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

                extractedText = await ExtractTextFromPdf(tempFilePath, numberOfPages, fileId);

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

        private async Task<string> ExtractTextFromPdf(string tempFilePath, int totalNumberOfPages, Guid fileId)
        {
            var text = string.Empty;
            var dpi = 300;

            var images = GetImages(tempFilePath, dpi, totalNumberOfPages);

            for (var pageIndex = 0; pageIndex < images.Count; pageIndex++)
            {
                _logger.LogDebug($"Processing page {pageIndex} file: {fileId}");
                if (text.Length > _maxCharacters)
                {
                    break;
                }

                var image = images.ElementAt(pageIndex);

                if (image == null)
                {
                    _logger.LogDebug($"Image is null for page {pageIndex} file: {fileId}");
                    continue;
                }

                var pageText = await _tesseractManager.ExtractTextFromImageAsync(image.ToByteArray());

                if (string.IsNullOrWhiteSpace(pageText))
                {
                    _logger.LogDebug($"Text is empty for page {pageIndex} file: {fileId}");
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

        public int GetNumberOfPages(byte[] fileContents)
        {
            using (PdfReader reader = new PdfReader(fileContents))
            {

                return reader.NumberOfPages;
            }

        }

        public string GetBase64Thumbnail(string pdfPath, int dpi)
        {
            try
            {
                var images = GetImages(pdfPath, dpi, 1);

                var firstImage = images.First();

                var reducedImage = ReduceImageSize(new MagickImage(firstImage), 0.40);

                return firstImage.ToBase64();
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not create base 64 thumbnail error: {ex.Message}");
            }

            return string.Empty;
        }

        public MagickImage ReduceImageSize(MagickImage image, double scale)
        {
            try
            {
                // Calculate new dimensions
                int newWidth = (int)(image.Width * scale);
                int newHeight = (int)(image.Height * scale);

                // Resize the image
                image.Resize(newWidth, newHeight);

                return image;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not reduce image size error: {ex.Message}");
            }

            return image;
        }

        public MagickImageCollection GetImages(string pdfPath, int dpi, int page)
        {
            try
            {
                // Settings the density to 300 dpi will create an image with a better quality
                var settings = new MagickReadSettings
                {
                    Density = new Density(300, 300)
                };

                var images = new MagickImageCollection();

                // Read only the first page of the pdf file
                images.Read(pdfPath, settings);
                _logger.LogDebug($"Image count: {images.Count}");
                foreach (MagickImage image in images)
                {
                    image.Format = MagickFormat.Png;
                }
                return images;

            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not get image from pdf error: {ex.Message}");
                return new MagickImageCollection();
            }
        }
    }
}
