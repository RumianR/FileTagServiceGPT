using System.Drawing;
using Tesseract;
using Image = System.Drawing.Image;

namespace OpenAIApp.Managers
{


    public class TesseractManager : ITesseractManager
    {
        private readonly TesseractEngine _engine;
        private readonly ILogger<TesseractManager> _logger;
        private readonly object _lockObj = new object(); // Object for locking


        public TesseractManager(ILogger<TesseractManager> logger)
        {
            _logger = logger;
            // Initialize the Tesseract engine. You need to specify the path to the tessdata folder.
            // Ensure that tessdata is included in your project and its build action is set to "Content".
            _engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        }

        public string ExtractTextFromImage(Image image)
        {
            lock (_lockObj) // Only one thread can enter this block at a time
            {
                try
                {
                    using (var bitmap = new Bitmap(image))
                    {
                        using (var page = _engine.Process(bitmap))
                        {
                            return page.GetText();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    _logger.LogDebug("Error: " + ex.Message);
                    return string.Empty;
                }
            }
        }

        public string ExtractTextFromImage(Bitmap bitmap)
        {
            lock (_lockObj) // Only one thread can enter this block at a time
            {
                try
                {
                    using (var page = _engine.Process(bitmap))
                    {
                        return page.GetText();
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    _logger.LogDebug("Error: " + ex.Message);
                    return string.Empty;
                }
            }
        }

        public string ExtractTextFromImage(string filepath)
        {
            lock (_lockObj) // Only one thread can enter this block at a time
            {
                try
                {
                    using (var bitmap = new Bitmap(filepath))
                    {
                        using (var page = _engine.Process(bitmap))
                        {
                            return page.GetText();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    _logger.LogDebug("Error: " + ex.Message);
                    return string.Empty;
                }
            }
        }
    }
}
