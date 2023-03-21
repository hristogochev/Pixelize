using System.Drawing;

namespace Pixelize.Utils;

public static class ColorUtils
{
    public static bool HasLetterInNameInPosition(this Color color, string letter, int position)
    {
        if (color.Name == "Black") return false;
        return color.Name[position].ToString() == letter;
    }
}