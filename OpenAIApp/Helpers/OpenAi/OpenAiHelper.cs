using Microsoft.Extensions.Options;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAIApp.Configurations;
using Supabase;

namespace OpenAIApp.Helpers.OpenAi
{
    public class OpenAiHelper : IOpenAiHelper
    {
        private readonly OpenAiConfig _openAiConfig;

        private readonly string _queryBase =
            "Return me in JSON format, and it absolutely must be in JSON, "
            + "a list of items called tags that can categorize the text with the top 50 more meaningful classifiers."
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

            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);

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
    }
}
