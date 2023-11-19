using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAIApp.Clients.HttpClients;
using OpenAIApp.Configurations;

namespace OpenAIApp.Helpers.OpenAi
{
    public class OpenAiHelper : IOpenAiHelper
    {
        private readonly OpenAiConfig _openAiConfig;

        private readonly string _queryBase =
            "Return me in JSON format, and it absolutely must be in JSON, "
            + "a list of items called tags that can categorize the text with the top 50 more meaningful classifiers. "
            + "and a property called name which best titles this document based on it's content "
            + "This text is from a pdf document or a mail letter and the purpose is to have helpful keywords/tags that are meaningful "
            + "to associate this document in a bin."
            + "Here is the document text: ";

        public OpenAiHelper(OpenAiConfig openAiConfig)
        {
            _openAiConfig = openAiConfig;
        }

        public async Task<string> GetTags(string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            text = TrimToTokenLimit(text, 2500);

            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            api.HttpClientFactory = new CustomHttpClientFactory();

            var results = await api.Chat.CreateChatCompletionAsync(
                new ChatRequest()
                {
                    Model = Model.ChatGPTTurbo,
                    Temperature = 0.1,
                    Messages = new ChatMessage[]
                    {
                        new ChatMessage(ChatMessageRole.User, _queryBase + text)
                    }
                }
            );

            return results.Choices[0].Message.Content;
        }

        public string TrimToTokenLimit(string input, int maxTokens)
        {
            int maxCharacters = maxTokens * 4;  // 4 characters per token
            if (input.Length > maxCharacters)
            {
                // Trim the string to the maximum character count
                string trimmedInput = input.Substring(0, maxCharacters);

                // Optional: Trim to the last complete word to avoid cutting in the middle of a word
                int lastSpaceIndex = trimmedInput.LastIndexOf(' ');
                if (lastSpaceIndex > 0)
                {
                    trimmedInput = trimmedInput.Substring(0, lastSpaceIndex);
                }
                return trimmedInput;
            }
            return input;
        }

    }
}
