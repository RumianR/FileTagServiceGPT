namespace OpenAIApp.Common
{
    public class FileMetadata
    {
        public string FileContentText { get; set; } = string.Empty;

        public long FileLengthInBytes { get; set; }

        public int NumberOfPages { get; set; }

        public string TempPathToThumbnail { get; set; } = string.Empty;

    }
}
