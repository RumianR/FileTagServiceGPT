namespace OpenAIApp.Helpers
{
    public static class UrlHelper
    {
        public static (string userId, string fileFolder) ParseUserIdAndFolder(string url)
        {
            try
            {
                // Split the URL by '/'
                string[] parts = url.Split('/');

                // Find the index of the part that contains 'files'
                int filesIndex = Array.FindIndex(parts, part => part.Equals("files"));
                if (filesIndex == -1 || filesIndex >= parts.Length - 2)
                {
                    // If 'files' is not found or there are not enough elements after 'files'
                    return (string.Empty, string.Empty);
                }

                // Extract the userId and fileFolder using the index of 'files'
                string userId = parts[filesIndex + 1];
                string fileFolder = parts[filesIndex + 2].Replace(".pdf", string.Empty);

                return (userId, fileFolder);
            }
            catch
            {
                // Return empty strings if any error occurs
                return (string.Empty, string.Empty);
            }
        }
    }
}
