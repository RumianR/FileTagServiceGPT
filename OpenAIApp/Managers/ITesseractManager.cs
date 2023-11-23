using Image = System.Drawing.Image;

namespace OpenAIApp.Managers
{
    public interface ITesseractManager
    {
        string ExtractTextFromImage(Image image);

    }
}
