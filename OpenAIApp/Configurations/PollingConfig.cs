using OpenAIApp.Enums;

namespace OpenAIApp.Configurations
{
    public static class PollingConfig
    {
        public static readonly int JobServicePollingIntervalInSeconds = 3;

        public static readonly int FileProcessingServicePollingIntervalInSeconds = 3;

        public static readonly FileState PollingState = FileState.UPLOADED;
    }
}
