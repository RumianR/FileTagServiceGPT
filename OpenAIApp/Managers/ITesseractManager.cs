using System.Drawing;
using Image = System.Drawing.Image;

namespace OpenAIApp.Managers
{
    public interface ITesseractManager
    {
        string ExtractTextFromImage(Image image);

        string ExtractTextFromImage(string filepath);

        string ExtractTextFromImage(Bitmap bitmap);
    }
}
