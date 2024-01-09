namespace QR.Reader;

public class GridReader
{
    private readonly bool[,] _grid;
    private readonly Func<int, int, bool> _mask;

    public GridReader(bool[,] grid, Func<int, int, bool> mask)
    {
        _grid = grid;
        _mask = mask;
    }

    public byte ReadByteUp(int x, int y)
    {
        //87
        //56
        //43
        //21
        var b = Read(x, y);
        b = (b << 1) | Read(x - 1, y);
        b = (b << 1) | Read(x, y - 1);
        b = (b << 1) | Read(x - 1, y - 1);
        b = (b << 1) | Read(x, y - 2);
        b = (b << 1) | Read(x - 1, y - 2);
        b = (b << 1) | Read(x, y - 3);
        b = (b << 1) | Read(x - 1, y - 3);
        return (byte)b;
    }

    public byte ReadByteDown(int x, int y)
    {
        //21
        //43
        //65
        //78
        var b = Read(x, y);
        b = (b << 1) | Read(x - 1, y);
        b = (b << 1) | Read(x, y + 1);
        b = (b << 1) | Read(x - 1, y + 1);
        b = (b << 1) | Read(x, y + 2);
        b = (b << 1) | Read(x - 1, y + 2);
        b = (b << 1) | Read(x, y + 3);
        b = (b << 1) | Read(x - 1, y + 3);
        return (byte)b;
    }

    public byte ReadByteAntiClockwise(int x, int y)
    {
        //   6543
        //   8721
        var b = Read(x, y);
        b = (b << 1) | Read(x - 1, y);
        b = (b << 1) | Read(x, y - 1);
        b = (b << 1) | Read(x - 1, y - 1);
        b = (b << 1) | Read(x - 2, y - 1);
        b = (b << 1) | Read(x - 3, y - 1);
        b = (b << 1) | Read(x - 2, y);
        b = (b << 1) | Read(x - 3, y);
        return (byte)b;
    }

    public byte ReadByteClockwise(int x, int y)
    {
        //   8721
        //   6543
        var b = Read(x, y);
        b = (b << 1) | Read(x - 1, y);
        b = (b << 1) | Read(x, y + 1);
        b = (b << 1) | Read(x - 1, y + 1);
        b = (b << 1) | Read(x - 2, y + 1);
        b = (b << 1) | Read(x - 3, y + 1);
        b = (b << 1) | Read(x - 2, y);
        b = (b << 1) | Read(x - 3, y);
        return (byte)b;
    }

    public byte ReadNibbleUp(int x, int y)
    {
        //43
        //21
        var b = Read(x, y);
        b = (b << 1) | Read(x - 1, y);
        b = (b << 1) | Read(x, y - 1);
        b = (b << 1) | Read(x - 1, y - 1);
        return (byte)b;
    }

    public int ReadBits(int x, int y, int length)
    {
        //123..length
        var b = Read(x, y);
        for (var i = 1; i < length; i++)
            b = (b << 1) | Read(x + i, y);
        return b;
    }

    private int Read(int x, int y)
    {
        return _grid[x, y] ^ _mask(y, x) ? 1 : 0;
    }
}