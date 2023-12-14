using OpenAIApp.Concurrency;
using Tesseract;

namespace OpenAIApp.Managers
{
    public class TesseractManager : ITesseractManager
    {
        private readonly AsyncProducerConsumerQueue<TesseractEngine> _engines;
        private readonly ILogger<TesseractManager> _logger;
        private const int EngineCount = 10; // Number of TesseractEngine instances

        public TesseractManager(ILogger<TesseractManager> logger)
        {
            _logger = logger;
            _engines = new AsyncProducerConsumerQueue<TesseractEngine>(logger);

            // Initialize a pool of TesseractEngine instances
            for (int i = 0; i < EngineCount; i++)
            {
                _engines.Enqueue(new TesseractEngine(@"./tessdata", "eng", EngineMode.Default));
            }
        }

        private async Task<string> ProcessImageAsync(byte[] image, CancellationToken cancellationToken)
        {
            TesseractEngine engine = null;
            try
            {
                engine = await _engines.DequeueAsync(cancellationToken);


                using (var page = engine.Process(Pix.LoadFromMemory(image)))
                {
                    return page.GetText();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Error: " + ex.Message);
                return string.Empty;
            }
            finally
            {
                if (engine != null)
                {
                    _engines.Enqueue(engine);
                }
            }
        }

        public async Task<string> ExtractTextFromImageAsync(byte[] image, CancellationToken cancellationToken = default)
        {
            return await ProcessImageAsync(image, cancellationToken);
        }

        //public async Task<string> ExtractTextFromImageAsync(Bitmap bitmap, CancellationToken cancellationToken = default)
        //{
        //    return await ProcessBitmapAsync(bitmap, cancellationToken);
        //}

        //public async Task<string> ExtractTextFromImageAsync(string filepath, CancellationToken cancellationToken = default)
        //{
        //    using (var bitmap = new Bitmap(filepath))
        //    {
        //        return await ProcessBitmapAsync(bitmap, cancellationToken);
        //    }
        //}
    }

}
