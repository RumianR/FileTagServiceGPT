using System.Collections.Concurrent;

namespace OpenAIApp.Concurrency
{
    public class AsyncProducerConsumerQueue<T>
    {
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly ILogger _logger;

        public AsyncProducerConsumerQueue(ILogger logger)
        {
            _logger = logger;
        }

        public void Enqueue(T item)
        {
            if (item == null)
            {
                _logger.LogDebug("Cannot enqueue a null item.");
                return;
            }

            _queue.Enqueue(item);
            _signal.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
                _queue.TryDequeue(out T item);
                return item;
            }

            catch (Exception ex)
            {
                _logger.LogDebug($"An error occurred while waiting to dequeue an item. error: {ex.Message}");
                return default;
            }
        }
    }

}
