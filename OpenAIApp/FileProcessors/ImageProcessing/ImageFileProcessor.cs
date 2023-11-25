using Ghostscript.NET.Rasterizer;
using ImageMagick;
using iTextSharp.text.pdf;
using OpenAIApp.Common;
using OpenAIApp.Managers;
using System.Drawing;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;


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

                var convertedStream = GetConvertedStream(tempFilePath);

                extractedText = await ExtractTextFromImage(convertedStream);
                thumbnailBase64 = convertedStream.Base64Png;

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

        private Task<string> ExtractTextFromImage(ConvertedStream convertedStream)
        {
            try
            {
                return _tesseractManager.ExtractTextFromImageAsync(convertedStream.Bitmap);

            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not read image from converted stream error: {ex.Message}");
                return Task.FromResult(string.Empty);
            }

        }

        private ConvertedStream GetConvertedStream(string tempFilePath)
        {
            try
            {
                using var magickImage = new MagickImage(tempFilePath);
                return ConvertMagickImageToBitmap(magickImage);
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

        public ConvertedStream ConvertMagickImageToBitmap(MagickImage magickImage)
        {
            var convertedStream = new ConvertedStream();

            using (var memoryStream = new MemoryStream())
            {
                // Save the MagickImage to MemoryStream in a Bitmap-compatible format (e.g., PNG
                magickImage.Format = MagickFormat.Png;
                magickImage.Write(memoryStream);
                memoryStream.Position = 0;

                // Create a Bitmap from the MemoryStream
                convertedStream.Bitmap = new Bitmap(memoryStream);
            }

            using (var memoryStream = new MemoryStream())
            {
                // Save the MagickImage to MemoryStream in a Bitmap-compatible format (e.g., PNG
                magickImage = ReduceImageSize(magickImage, 0.40);
                magickImage.Format = MagickFormat.Png;
                magickImage.Write(memoryStream);
                memoryStream.Position = 0;

                // Create a Bitmap from the MemoryStream

                // Convert the MemoryStream to a byte array
                byte[] imageBytes = memoryStream.ToArray();
                convertedStream.Base64Png = Convert.ToBase64String(imageBytes);
            }

            return convertedStream;
        }

    }
}
