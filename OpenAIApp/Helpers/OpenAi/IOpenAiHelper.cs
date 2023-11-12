namespace OpenAIApp.Helpers.OpenAi
{
    public interface IOpenAiHelper
    {
        Task<string> GetTags(string text);
    }
}
