using ImageMagick;
using OpenAIApp.Common;
using OpenAIApp.Managers;



namespace OpenAIApp.FileProcessors.ImageProcessing
{
    public class ImageFileProcessor
    {
        public ILogger<ImageFileProcessor> _logger;

        public ITesseractManager _tesseractManager;

        public ImageFileProcessor(ILogger<ImageFileProcessor> logger, ITesseractManager tesseractManager)
        {
            _logger = logger;
            _tesseractManager = tesseractManager;
        }

        public async Task<FileMetadata> GetFileMetadataAsync(string url, Guid fileId, int maxPages = int.MaxValue)
        {
            string tempFilePath = Path.GetTempFileName();

            string extractedText = string.Empty;
            long lengthOfFileInBytes = 0;
            int numberOfPages = 1;
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

                var image = GetImage(tempFilePath);

                extractedText = await ExtractTextFromImage(image.ToByteArray());
                thumbnailBase64 = ReduceImageSize(image, 0.40).ToBase64();
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
                FileType = Enums.FileType.IMAGE
            };
        }

        private Task<string> ExtractTextFromImage(byte[] image)
        {
            try
            {
                return _tesseractManager.ExtractTextFromImageAsync(image);

            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not read image from converted stream error: {ex.Message}");
                return Task.FromResult(string.Empty);
            }

        }

        private MagickImage GetImage(string tempFilePath)
        {
            try
            {
                var image = new MagickImage(tempFilePath);
                image.Format = MagickFormat.Png;

                return image;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not create converted stream from filepath error: {ex.Message}");
            }

            return null;
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

    }
}
