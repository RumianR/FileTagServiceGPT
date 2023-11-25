using System.Drawing;
using System.Threading.Tasks;
using Image = System.Drawing.Image;

namespace OpenAIApp.Managers
{
    public interface ITesseractManager
    {
        Task<string> ExtractTextFromImageAsync(Image image, CancellationToken cancellationToken = default);

        Task<string> ExtractTextFromImageAsync(string filepath, CancellationToken cancellationToken = default);

        Task<string> ExtractTextFromImageAsync(Bitmap bitmap, CancellationToken cancellationToken = default);
    }
}
