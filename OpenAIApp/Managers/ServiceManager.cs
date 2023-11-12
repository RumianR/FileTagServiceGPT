using OpenAIApp.Services;

namespace OpenAIApp.Managers
{
    public class ServiceManager : IHostedService
    {
        private readonly IEnumerable<IService> _services;

        public ServiceManager(IEnumerable<IService> services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var service in _services)
            {
                service.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var service in _services)
            {
                service.Stop();
            }
        }
    }
}
