using QR.Reader;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using var image = Image.Load<Rgba32>("/Users/davidbetteridge/Personal/QRCodes/dojo.png");
using var file = File.CreateText("/Users/davidbetteridge/Personal/QRCodes/raw.txt");

const int squareWidth = 8;
const int squareHeight = 8;

var rows = image.Width / squareWidth;
var cols = image.Height / squareHeight;
var grid = new bool[cols, rows];

var version = (rows - 17) / 4;

var rowNumber = 0;
for (int y = 0; y < image.Height; y += squareHeight)
{
    var columnNumber = 0;
    for (var x = 0; x < image.Width; x += squareWidth)
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
    file.WriteLine();
    Console.WriteLine();
    rowNumber++;
}

Console.ForegroundColor = ConsoleColor.White;


var plainReader = new GridReader(grid, NoMask);
var format = plainReader.ReadBits(x: 0, y: 8, length: 5);
var formatXored = format ^ 0b10101;
var mask = formatXored & 0b111;

Console.Write("Mask: ");
Console.WriteLine(mask);


var reader = new GridReader(grid, Mask6);

Console.Write("Encoding: ");
Console.WriteLine(reader.ReadNibbleUp(cols - 1, rows - 1));

Console.Write("Length: ");
Console.WriteLine(reader.ReadByteUp(cols - 1, rows - 3));

Console.Write("Text: ");
// Char 1
Console.Write((char)reader.ReadByteUp(cols - 1, rows - 7));

// Char 2
Console.Write((char)reader.ReadByteAntiClockwise(cols - 1, rows - 11));

// Char 3
Console.Write((char)reader.ReadByteDown(cols - 3, rows - 10));

// Char 4
Console.Write((char)reader.ReadByteDown(cols - 3, rows - 6));

// Char 5
Console.Write((char)reader.ReadByteClockwise(cols - 3, rows - 2));

// Char 6
Console.Write((char)reader.ReadByteUp(cols - 5, rows - 3));

// Char 7
Console.Write((char)reader.ReadByteUp(cols - 5, rows - 7));

// Char 8
Console.Write((char)reader.ReadByteAntiClockwise(cols - 5, rows - 11));

// Char 9
Console.Write((char)reader.ReadByteDown(cols - 7, rows - 10));

// Char 10
Console.Write((char)reader.ReadByteDown(cols - 7, rows - 6));

// Char 11
Console.Write((char)reader.ReadByteClockwise(cols - 7, rows - 2));

// Char 12
Console.Write((char)reader.ReadByteUp(cols - 9, rows - 3));
Console.WriteLine();


bool Mask6(int i, int j)
{
    return ((i * j) % 3 + i * j) % 2 == 0;
}

bool NoMask(int i, int j)
{
    return false;
}