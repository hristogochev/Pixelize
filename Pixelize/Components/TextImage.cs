// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Drawing;
using Pixelize.Utils;

namespace Pixelize.Components;

public class TextImage
{
    private Color[,] Pixels { get; }
    private bool[,] PixelBits { get; set; }

    public TextImage(Image imageInput)
    {
        Pixels = ImageUtils.ConvertToPixels(imageInput);
        PixelBits = ImageUtils.ConvertToBits(Pixels);
    }

    // Exports
    public Bitmap Export()
    {
        return ImageUtils.PixelsToBitmap(Pixels);
    }

    // Pixel manipulation
    public void RemoveYellowLine()
    {
        ReplacePixelsWithLetterInPosition("0", 6, Color.White);
        ReplacePixelColor(Color.White, Color.Black);
        RemoveLonelyArtifacts();
    }

    public void RemoveLonelyArtifacts()
    {
        var height = Pixels.GetLength(0);
        var width = Pixels.GetLength(1);

        var mask = new int[9];

        for (var ii = 0; ii < width; ii++)
        {
            for (var jj = 0; jj < height; jj++)
            {
                Color c;
                if (ii - 1 >= 0 && jj - 1 >= 0)
                {
                    c = Pixels[jj - 1, ii - 1];
                    mask[0] = Convert.ToInt16(c.R);
                }
                else
                {
                    mask[0] = 0;
                }

                if (jj - 1 >= 0 && ii + 1 < width)
                {
                    c = Pixels[jj - 1, ii + 1];
                    mask[1] = Convert.ToInt16(c.R);
                }
                else
                    mask[1] = 0;

                if (jj - 1 >= 0)
                {
                    c = Pixels[jj - 1, ii];
                    mask[2] = Convert.ToInt16(c.R);
                }
                else
                    mask[2] = 0;

                if (ii + 1 < width)
                {
                    c = Pixels[jj, ii + 1];
                    mask[3] = Convert.ToInt16(c.R);
                }
                else
                    mask[3] = 0;

                if (ii - 1 >= 0)
                {
                    c = Pixels[jj, ii - 1];
                    mask[4] = Convert.ToInt16(c.R);
                }
                else
                    mask[4] = 0;

                if (ii - 1 >= 0 && jj + 1 < height)
                {
                    c = Pixels[jj + 1, ii - 1];
                    mask[5] = Convert.ToInt16(c.R);
                }
                else
                    mask[5] = 0;

                if (jj + 1 < height)
                {
                    c = Pixels[jj + 1, ii];
                    mask[6] = Convert.ToInt16(c.R);
                }
                else
                    mask[6] = 0;


                if (ii + 1 < width && jj + 1 < height)
                {
                    c = Pixels[jj + 1, ii + 1];
                    mask[7] = Convert.ToInt16(c.R);
                }
                else
                    mask[7] = 0;

                c = Pixels[jj, ii];
                mask[8] = Convert.ToInt16(c.R);
                Array.Sort(mask);
                var mid = mask[4];
                Pixels[jj, ii] = Color.FromArgb(mid, mid, mid);
            }
        }

        PixelBits = ImageUtils.ConvertToBits(Pixels);
    }

    public void ReplacePixelsWithLetterInPosition(string letter, int position, Color color) =>
        ReplacePixels(c => c.HasLetterInNameInPosition(letter, position), color);

    public void ReplacePixelColor(Color currentColor, Color color) =>
        ReplacePixels(c => c == currentColor, color);

    public void ReplacePixelsAboveBrightnessLimit(float brightnessLimit, Color color) =>
        ReplacePixels(c => c.GetBrightness() > brightnessLimit, color);

    public void ReplacePixels(Func<Color, bool> condition, Color color)
    {
        for (var y = 0; y < Pixels.GetLength(0); y++)
        {
            for (var x = 0; x < Pixels.GetLength(1); x++)
            {
                if (condition(Pixels[y, x])) Pixels[y, x] = color;
            }
        }

        PixelBits = ImageUtils.ConvertToBits(Pixels);
    }

    // Character splitting
    public CharacterImage[] SplitIntoCharacterImages()
    {
        var initialSeparatedCharacterImages = new Queue<Bitmap>();
        var characterLines = new List<int>();
        var characterXCoordinates = new List<int>();
        for (var x = 5; x < PixelBits.GetLength(1) - 3; x++)
        {
            for (var y = 3; y < PixelBits.GetLength(0) - 3; y++)
            {
                if (PixelBits[y, x])
                {
                    characterLines.Add(x);
                    break;
                }
            }
        }

        for (var i = 0; i < characterLines.Count - 1; i++)
        {
            if (characterLines[i] + 1 == characterLines[i + 1]) continue;

            if (!IsFragmentOfALetter(characterLines[i])) characterXCoordinates.Add(characterLines[i]);
           
        }

        var imageToCutFrom = ImageUtils.PixelsToBitmap(Pixels);

        for (var i = 0; i <= characterXCoordinates.Count; i++)
        {
            var character = i switch
            {
                0 => GetFirstCharacter(imageToCutFrom, characterXCoordinates),

                _ when i < characterXCoordinates.Count => GetMiddleCharacter(imageToCutFrom, characterXCoordinates, i),

                _ when i == characterXCoordinates.Count => GetLastCharacter(imageToCutFrom, characterXCoordinates),

                _ => throw new ArgumentOutOfRangeException()
            };

            initialSeparatedCharacterImages.Enqueue(character);
        }

        var temp2 = new List<Bitmap>();

        while (initialSeparatedCharacterImages.Count != 0)
        {
            var tempImage = initialSeparatedCharacterImages.Dequeue();
            if (tempImage.Width > 6)
            {
                temp2.Add(tempImage);
            }
            else
            {
                temp2[temp2.Count - 1] = ImageUtils.CombineBitmaps(temp2[temp2.Count - 1], tempImage);
            }
        }

        var output = new CharacterImage[temp2.Count];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = new CharacterImage(temp2[i], i);
        }

        return output;
    }

    public bool IsFragmentOfALetter(int x)
    {
        var blackPixelsAtColumn = new int[6];

        for (var i = 0; i < blackPixelsAtColumn.Length; i++)
        {
            blackPixelsAtColumn[i] = ImageUtils.GetBlackPixelsInColumn(PixelBits, x - i);
        }

        return blackPixelsAtColumn.All(t => t <= 6);
    }

    public Bitmap GetFirstCharacter(Bitmap image, IReadOnlyList<int> coordinates)
    {
        var firstCharacterRectangle = new Rectangle(5, 2, coordinates[0] - 2, 46);
        var firstCharacter = image.Clone(firstCharacterRectangle, image.PixelFormat);
        return FixFirstCharacter( firstCharacter);
    }

    public Bitmap GetMiddleCharacter(Bitmap image, IReadOnlyList<int> coordinates, int i)
    {
        var middleCharacterRectangle = new Rectangle(coordinates[i - 1] + 3, 2,
            coordinates[i] - coordinates[i - 1], 46);
        return image.Clone(middleCharacterRectangle, image.PixelFormat);
    }

    public Bitmap GetLastCharacter(Bitmap image, IReadOnlyList<int> coordinates)
    {
        var lastCharacterRectangle = new Rectangle(coordinates[coordinates.Count - 1] + 3,
            2,
            Pixels.GetLength(1) - coordinates[coordinates.Count - 1] - 3, 46);
        var lastCharacter = image.Clone(lastCharacterRectangle, image.PixelFormat);
        return FixLastCharacter( lastCharacter);
    }
    
    private Bitmap FixFirstCharacter(Bitmap firstCharacter)
    {
        var firstCharacterColumn = 0;
       
        var pixelBits = ImageUtils.ConvertToBits(firstCharacter);

        for (var x = 0; x < pixelBits.GetLength(1); x++)
        {
            var exit = false;
            for (var y = 0; y < pixelBits.GetLength(0); y++)
            {
                if (pixelBits[y, x])
                {
                    firstCharacterColumn = x;
                    exit = true;
                    break;
                }
            }

            if (exit) break;
        }

        var rectangle = new Rectangle(firstCharacterColumn, 0, firstCharacter.Width - firstCharacterColumn ,
            firstCharacter.Height);
        
        return firstCharacter.Clone(rectangle, firstCharacter.PixelFormat);
    }
    
    private Bitmap FixLastCharacter(Bitmap lastCharacter)
    {
        var lastCharacterColumn = 0;

        var pixelBits = ImageUtils.ConvertToBits(lastCharacter);

        for (var x = 0; x < pixelBits.GetLength(1); x++)
        {
            for (var y = 0; y < pixelBits.GetLength(0); y++)
            {
                if (pixelBits[y, x])
                {
                    lastCharacterColumn = x;
                    break;
                }
            }
        }

        var rectangle = new Rectangle(0, 0, lastCharacterColumn, lastCharacter.Height);

        return lastCharacter.Clone(rectangle, lastCharacter.PixelFormat);
    }
}