// See https://aka.ms/new-console-template for more information

// Create the text image

using System.Drawing;
using Pixelize.Components;

// Initialize the paths
var imagePath = "Image.png";
var firstEdit = "ImageEdited1.png";
var secondEdit = "ImageEdited2.png";

// Initialize the text image
var image = Image.FromFile(imagePath);
var textImage = new TextImage(image);

// Do the necessary pixel manipulation and save progress in the meanwhile
textImage.RemoveYellowLine();
textImage.Export().Save(firstEdit);
textImage.ReplacePixelsAboveBrightnessLimit(0.3f, Color.White);
textImage.Export().Save(secondEdit);

// Split into characters
var charactersImages = textImage.SplitIntoCharacterImages();

// Do something with the produced character bitmaps
var images = charactersImages.Select(characterImage => characterImage.Export());