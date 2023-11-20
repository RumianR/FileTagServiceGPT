namespace OpenAIApp.Clients.HttpClients
{
    public class CustomHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(1000);

            return httpClient;
        }
    }
}
