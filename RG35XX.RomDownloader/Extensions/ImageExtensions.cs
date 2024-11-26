using RG35XX.Core.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace RomDownloader.Extensions
{
    internal static class ImageExtensions
    {
        public static Bitmap ToBitmap(this Image image)
        {
            // Convert the image to a known pixel type (e.g., Rgba32)
            Image<Rgba32> rgbaImage = image.CloneAs<Rgba32>();

            Bitmap bitmap = new(rgbaImage.Width, rgbaImage.Height);

            // Iterate through the pixels
            for (int y = 0; y < rgbaImage.Height; y++)
            {
                Memory<Rgba32> pixelRowSpan = rgbaImage.DangerousGetPixelRowMemory(y);

                for (int x = 0; x < rgbaImage.Width; x++)
                {
                    Rgba32 pixel = pixelRowSpan.Span.ToArray()[x];

                    bitmap.SetPixel(x, y, new RG35XX.Core.Drawing.Color(pixel.R, pixel.G, pixel.B));
                }
            }

            return bitmap;
        }
    }
}