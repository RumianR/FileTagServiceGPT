using System.Drawing;
using System.Threading.Tasks;

namespace OpenAIApp.Managers
{
    public interface ITesseractManager
    {
        Task<string> ExtractTextFromImageAsync(byte[] image, CancellationToken cancellationToken = default);

        //Task<string> ExtractTextFromImageAsync(string filepath, CancellationToken cancellationToken = default);

        //Task<string> ExtractTextFromImageAsync(Bitmap bitmap, CancellationToken cancellationToken = default);
    }
}
