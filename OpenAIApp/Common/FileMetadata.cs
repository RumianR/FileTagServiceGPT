using OpenAIApp.Enums;

namespace OpenAIApp.Common
{
    public class FileMetadata
    {
        public string FileContentText { get; set; } = string.Empty;

        public long FileLengthInBytes { get; set; }

        public int NumberOfPages { get; set; }

        public string ThumbnailBase64 { get; set; } = string.Empty;


        public FileType FileType { get; set; } = FileType.UNSUPPORTED;

    }
}
