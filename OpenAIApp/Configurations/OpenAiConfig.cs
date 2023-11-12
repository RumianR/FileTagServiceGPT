namespace OpenAIApp.Configurations
{
    public class OpenAiConfig
    {
        public OpenAiConfig()
        {
            Key = Environment.GetEnvironmentVariable("OpenAI_KEY");
        }

        public string Key { get; } = string.Empty;
    }
}
