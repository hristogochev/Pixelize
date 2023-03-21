using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Pixelize.Utils;

public static class ImageUtils
{
    public static Color[,] ConvertToPixels(Image imageInput)
    {
        var bitmapImageInput = (Bitmap)imageInput;
        var output = new Color[bitmapImageInput.Height, bitmapImageInput.Width];
        for (var i = 0; i < output.GetLength(0); i++)
        {
            for (var k = 0; k < output.GetLength(1); k++)
            {
                output[i, k] = bitmapImageInput.GetPixel(k, i);
                if (output[i, k].Name == "ff000000") output[i, k] = Color.Black;
            }
        }

        return output;
    }

    public static bool[,] ConvertToBits(Image image)
    {
        var pixelsInput = ConvertToPixels(image);
        return ConvertToBits(pixelsInput);
    }

    public static bool[,] ConvertToBits(Color[,] pixels)
    {
        var output = new bool[pixels.GetLength(0), pixels.GetLength(1)];
        for (var y = 0; y < pixels.GetLength(0); y++)
        {
            for (var x = 0; x < pixels.GetLength(1); x++)
            {
                output[y, x] = pixels[y, x]!=Color.White;
            }
        }

        return output;
    }

    public static Bitmap PixelBitsToBitmap(bool[,] pixelBits)
    {
        var bitmap = new Bitmap(pixelBits.GetLength(1), pixelBits.GetLength(0));

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                bitmap.SetPixel(x, y, pixelBits[y, x] ? Color.Black : Color.White);
            }
        }

        return bitmap;
    }

    public static Bitmap PixelsToBitmap(Color[,] pixels)
    {
        var bitmap = new Bitmap(pixels.GetLength(1), pixels.GetLength(0));

        for (int i = 0; i < bitmap.Height; i++)
        {
            for (int k = 0; k < bitmap.Width; k++)
            {
                bitmap.SetPixel(k, i, pixels[i, k]);
            }
        }

        return bitmap;
    }

    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using var graphics = Graphics.FromImage(destImage);
        
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var wrapMode = new ImageAttributes();
        
        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

        return destImage;
    }

    public static double Compare(Image firstImage, Image secondImage)
    {
        var resizedSecondImage = ResizeImage(secondImage, firstImage.Width, firstImage.Height);

        var firstImageConvertedPixels = ConvertToBits(firstImage);
        var secondImageConvertedPixels = ConvertToBits(resizedSecondImage);

        return ComparePixelBits(firstImageConvertedPixels, secondImageConvertedPixels);
    }

    public static Bitmap CombineBitmaps(Bitmap firstImage, Bitmap secondImage)
    {
        var width = firstImage.Width + secondImage.Width;
        var height = Math.Max(firstImage.Height, secondImage.Height);

        var bitmap = new Bitmap(width, height);

        using var g = Graphics.FromImage(bitmap);

        g.DrawImage(firstImage, 0, 0);
        g.DrawImage(secondImage, firstImage.Width, 0);

        return bitmap;
    }


    public static string[] PixelsToPixelNames(Color[,] pixels)
    {
        var output = new List<string>();
        for (var y = 0; y < pixels.GetLength(0); y++)
        {
            for (var x = 0; x < pixels.GetLength(1); x++)
            {
                if (output.Contains(pixels[y, x].Name) == false) output.Add(pixels[y, x].Name);
            }
        }

        return output.ToArray();
    }

    public static double ComparePixelBits(bool[,] firstPixelBits, bool[,] secondPixelBits)
    {
        var counter = 0;

        for (var y = 0; y < firstPixelBits.GetLength(0); y++)
        {
            for (var x = 0; x < firstPixelBits.GetLength(1); x++)
            {
                if (firstPixelBits[y, x] == secondPixelBits[y, x]) counter++;
            }
        }

        double size = firstPixelBits.GetLength(0) * firstPixelBits.GetLength(1);

        var percentage = 100 * counter / size;

        return percentage;
    }

    public static double CompareImageAndPixelBits(bool[,] pixelBits, Image image)
    {
        var resizedImage = ResizeImage(image, pixelBits.GetLength(1), pixelBits.GetLength(0));

        var resizedImagePixelBits = ConvertToBits(resizedImage);

        return ComparePixelBits(resizedImagePixelBits, pixelBits);
    }


    private static int GetVerticalBlackLayers(bool[,] pixelBits, int x)
    {
        var lineCoordinates = new List<int>();
        for (var y = 0; y < pixelBits.GetLength(0); y++)
        {
            if (pixelBits[y, x])
            {
                lineCoordinates.Add(y);
            }
        }

        var counter = 1;
        for (var i = 0; i < lineCoordinates.Count - 1; i++)
        {
            if (lineCoordinates[i + 1] != lineCoordinates[i] + 1) counter++;
        }

        return counter;
    }

    private static int GetHorizontalBlackLayers(bool[,] pixelBits, int y)
    {
        var lineCoordinates = new List<int>();
        for (var x = 0; x < pixelBits.GetLength(1); x++)
        {
            if (pixelBits[y, x])
            {
                lineCoordinates.Add(x);
            }
        }

        var counter = 1;
        for (var i = 0; i < lineCoordinates.Count - 1; i++)
        {
            if (lineCoordinates[i + 1] != lineCoordinates[i] + 1) counter++;
        }

        return counter;
    }

    public static int GetBlackPixels(Image imageInput)
    {
        var output = 0;
        var imageBits = ConvertToBits(imageInput);
        for (var y = 0; y < imageBits.GetLength(0); y++)
        {
            for (var x = 0; x < imageBits.GetLength(1); x++)
            {
                if (imageBits[y, x]) output++;
            }
        }

        return output;
    }

    public static int GetBlackPixels(bool[,] imageBits)
    {
        var output = 0;
        for (var y = 0; y < imageBits.GetLength(0); y++)
        {
            for (var x = 0; x < imageBits.GetLength(1); x++)
            {
                if (imageBits[y, x]) output++;
            }
        }

        return output;
    }


    public static int GetBlackPixelsInColumn(bool[,] pixelBits, int x)
    {
        var counter = 0;
        for (int y = 0; y < pixelBits.GetLength(0); y++)
        {
            if (pixelBits[y, x]) counter++;
        }

        return counter;
    }
}