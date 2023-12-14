namespace OpenAIApp.Configurations
{
    public class OpenAiConfig
    {
        public OpenAiConfig()
        {
            Key = Environment.GetEnvironmentVariable("OPENAI_KEY");
        }

        public string Key { get; } = string.Empty;
    }
}
