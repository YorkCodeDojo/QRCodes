using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using var image = Image.Load<Rgba32>("/Users/davidbetteridge/Personal/QR/dojo.png");

Console.WriteLine(image.Width);
Console.WriteLine(image.Height);

// 730
// 732
// 25
var squareWidth = 8;//(image.Width) / 35;
var squareHeight = 8;//(image.Height) / 35;

using var file = File.CreateText("/Users/davidbetteridge/Personal/QR/raw.txt");

var rows = image.Width / 8;
var cols = image.Height / 8;
var grid = new bool[cols, rows];

var version = (rows - 17) / 4;

var rowNumber = 0;
for (int y = 0; y < image.Height; y+=squareHeight)
{
    var columnNumber = 0;
    for (var x = 0; x < image.Width; x+=squareWidth)
    {
        if (image[x, y].B != 255)
            Console.ForegroundColor = ConsoleColor.Black;
        else
            Console.ForegroundColor = ConsoleColor.White;
        Console.Write('\u2588');
        
        if (image[x, y].B != 255)
            file.Write('B');
        else
            file.Write('W');

        grid[columnNumber, rowNumber] = image[x, y].B != 255;
        columnNumber++;
    }
    Console.WriteLine();
    rowNumber++;
}

Console.ForegroundColor = ConsoleColor.White;

// Format 5 bits
var formatText = "";
formatText += Read(grid, 0,8);
formatText += Read(grid, 1,8);
formatText += Read(grid, 2,8);
formatText += Read(grid, 3,8);
formatText += Read(grid, 4,8);
Console.WriteLine(formatText);
var format = Convert.ToInt32(formatText, 2);
Console.WriteLine(format);
format ^= 0b10101;
var mask = Convert.ToString(format, 2)[^3..];
Console.WriteLine(mask);

// Last 4 bits are the encoding type
var encodingText = "";
encodingText += ReadAndMask(grid, cols - 1, rows - 1);
encodingText += ReadAndMask(grid, cols - 2, rows - 1);
encodingText += ReadAndMask(grid, cols - 1, rows - 2);
encodingText += ReadAndMask(grid, cols - 2, rows - 2);
Console.WriteLine(encodingText);
var encoding = Convert.ToInt32(encodingText, 2);
Console.WriteLine(encoding);


// Length
var lengthText = "";
lengthText += ReadAndMask(grid, cols - 1, rows - 1 -2);
lengthText += ReadAndMask(grid, cols - 2, rows - 1 -2);
lengthText += ReadAndMask(grid, cols - 1, rows - 2 -2);
lengthText += ReadAndMask(grid, cols - 2, rows - 2 -2);
lengthText += ReadAndMask(grid, cols - 1, rows - 3 -2);
lengthText += ReadAndMask(grid, cols - 2, rows - 3 -2);
lengthText += ReadAndMask(grid, cols - 1, rows - 4 -2);
lengthText += ReadAndMask(grid, cols - 2, rows - 4 -2);
Console.WriteLine(lengthText);
var length = Convert.ToInt32(lengthText, 2);
Console.WriteLine(length);

// Last 8
var chr = "";
chr += ReadAndMask(grid, cols - 1, rows - 1 - 6);
chr += ReadAndMask(grid, cols - 2, rows - 1 - 6);
chr += ReadAndMask(grid, cols - 1, rows - 2 - 6);
chr += ReadAndMask(grid, cols - 2, rows - 2 - 6);
chr += ReadAndMask(grid, cols - 1, rows - 3 - 6);
chr += ReadAndMask(grid, cols - 2, rows - 3 - 6);
chr += ReadAndMask(grid, cols - 1, rows - 4 - 6);
chr += ReadAndMask(grid, cols - 2, rows - 4 - 6);
Console.WriteLine((char)Convert.ToInt32(chr, 2));

var chr2 = "";
chr2 += ReadAndMask(grid, cols - 1, rows - 11);
chr2 += ReadAndMask(grid, cols - 2, rows - 11);
chr2 += ReadAndMask(grid, cols - 1, rows - 12);
chr2 += ReadAndMask(grid, cols - 2, rows - 12);
chr2 += ReadAndMask(grid, cols - 3, rows - 12);
chr2 += ReadAndMask(grid, cols - 4, rows - 12);
chr2 += ReadAndMask(grid, cols - 3, rows - 11);
chr2 += ReadAndMask(grid, cols - 4, rows - 11);
Console.WriteLine((char)Convert.ToInt32(chr2, 2));



string Read(bool[,] g, int x, int y)
{
    // if (x%3==0)
    //     if (g[x, y])
    //         return "0";
    //     else
    //         return "1";
    // else
    if (g[x, y])
        return "1";
    else
        return "0";
}

string ReadAndMask(bool[,] g, int x, int y)
{
    if (Mask6(y,x))
        if (g[x, y])
            return "0";
        else
            return "1";
    else
        if (g[x, y])
            return "1";
        else
            return "0";
}

bool Mask6(int i, int j)
{
    return ((i * j) % 3 + i * j) % 2 == 0;
}
