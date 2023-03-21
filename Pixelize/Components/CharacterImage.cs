// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Drawing;
using Pixelize.Utils;

namespace Pixelize.Components;

public class CharacterImage
{
    public Color[,] Pixels { get; private set; }
    public bool[,] PixelBits { get; private set; }
    public int Place { get; }

    public CharacterImage(Bitmap bitmap, int place)
    {
        Place = place;
        Format(bitmap);
    }

    // Exports
    public (CharacterImage, CharacterImage) SplitInto2Characters(int rate, int firstCharacterPlace,
        int secondCharacterPlace)
    {
        var imageToSplit = ImageUtils.PixelsToBitmap(Pixels);
        var imageWidth = imageToSplit.Width;
        var imageHeight = imageToSplit.Height;
        var xToCutAt = Convert.ToInt32(rate * imageWidth / 100);

        var firstRectangle = new Rectangle(0, 0, xToCutAt, imageHeight);
        var secondRectangle = new Rectangle(xToCutAt, 0, imageWidth - xToCutAt, imageHeight);

        var firstImage = imageToSplit.Clone(firstRectangle, imageToSplit.PixelFormat);
        var secondImage = imageToSplit.Clone(secondRectangle, imageToSplit.PixelFormat);

        var firstCharacter = new CharacterImage(firstImage, firstCharacterPlace);
        var secondCharacter = new CharacterImage(secondImage, secondCharacterPlace);

        return (firstCharacter, secondCharacter);
    }
        
    public Bitmap Export()
    {
        return ImageUtils.PixelsToBitmap(Pixels);
    }

    // Analyzing tools
    public bool HasMoreThan1CharacterInside() =>
        PixelBits.GetLength(1) > Constants.MaximumWidthForACharacter;

    public bool IsALostFragment() =>
        ImageUtils.GetBlackPixels(PixelBits) < Constants.MinimalBlackPixelsForACharacter;


    // Formatting to essential size
    private void Format(Bitmap bitmap)
    {
        var leftCut = GetLeftCutXLine(bitmap);
        var rightCut = GetRightCutXLine(bitmap);
        var upCut = GetUpCutYLine(bitmap);
        var downCut = GetDownCutYLine(bitmap);

        if (rightCut - leftCut <= 0 || downCut - upCut <= 0) return;

        var rectangle = new Rectangle(leftCut, upCut, rightCut - leftCut, downCut - upCut);
        var image = bitmap.Clone(rectangle, bitmap.PixelFormat);

        Pixels = ImageUtils.ConvertToPixels(image);
        PixelBits = ImageUtils.ConvertToBits(Pixels);
    }

    private int GetLeftCutXLine(Bitmap bitmap)
    {
        for (var x = 0; x < bitmap.Width; x++)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).Name != "ffffffff") return x;
            }
        }

        return 0;
    }

    private int GetRightCutXLine(Bitmap bitmap)
    {
        for (var x = bitmap.Width - 1; x > 0; x--)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).Name != "ffffffff") return x;
            }
        }

        return 0;
    }

    private int GetUpCutYLine(Bitmap bitmap)
    {
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Name != "ffffffff") return y;
            }
        }

        return 0;
    }

    private int GetDownCutYLine(Bitmap bitmap)
    {
        for (var y = bitmap.Height - 1; y > 0; y--)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Name != "ffffffff") return y;
            }
        }

        return 0;
    }
}