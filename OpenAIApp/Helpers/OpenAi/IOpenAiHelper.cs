namespace OpenAIApp.Helpers.OpenAi
{
    public interface IOpenAiHelper
    {
        Task<string> CompleteSentence(string text);

        Task<string> CompleteSentenceAdvanced(string text);

        Task<string> GetTags(string text);
    }
}
