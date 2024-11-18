using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace RomDownloader
{
    public class ImageResizer
    {
        public static async Task<Image> ResizeImageAsync(byte[] imageBuffer, int maxWidth, int maxHeight)
        {
            Image image = Image.Load(imageBuffer);
            // Calculate aspect ratio
            float aspectRatio = (float)image.Width / image.Height;

            // Calculate new dimensions while maintaining aspect ratio
            int newWidth = image.Width;
            int newHeight = image.Height;

            if (newWidth > maxWidth)
            {
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspectRatio);
            }

            if (newHeight > maxHeight)
            {
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspectRatio);
            }

            // Perform the resize operation
            image.Mutate(x => x.Resize(newWidth, newHeight));

            return image;
        }
    }
}
