namespace QR.Reader;

public class GridReader
{
    private readonly bool[,] _grid;
    private readonly Func<int,int,bool> _mask;

    public GridReader(bool[,] grid, Func<int,int,bool> mask)
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
        var b = Read(_grid, x,  y);
        b = (b << 1) | Read(_grid, x -1, y);
        b = (b << 1) | Read(_grid, x, y-1);
        b = (b << 1) | Read(_grid, x -1, y-1);
        b = (b << 1) | Read(_grid, x, y-2);
        b = (b << 1) | Read(_grid, x -1, y-2);
        b = (b << 1) | Read(_grid, x, y-3);
        b = (b << 1) | Read(_grid, x -1, y-3);
        return (byte)b;
    }
    
    public byte ReadByteDown(int x, int y)
    {
        //21
        //43
        //65
        //78
        var b = Read(_grid, x,  y);
        b = (b << 1) | Read(_grid, x -1, y);
        b = (b << 1) | Read(_grid, x, y+1);
        b = (b << 1) | Read(_grid, x -1, y+1);
        b = (b << 1) | Read(_grid, x, y+2);
        b = (b << 1) | Read(_grid, x -1, y+2);
        b = (b << 1) | Read(_grid, x, y+3);
        b = (b << 1) | Read(_grid, x -1, y+3);
        return (byte)b;
    }
    
    public byte ReadByteAntiClockwise(int x, int y)
    {
        //   6543
        //   8721
        var b = Read(_grid, x,  y);
        b = (b << 1) | Read(_grid, x -1, y);
        b = (b << 1) | Read(_grid, x, y-1);
        b = (b << 1) | Read(_grid, x -1, y-1);
        b = (b << 1) | Read(_grid, x-2, y-1);
        b = (b << 1) | Read(_grid, x -3, y-1);
        b = (b << 1) | Read(_grid, x-2, y);
        b = (b << 1) | Read(_grid, x -3, y);
        return (byte)b;
    }

    public byte ReadByteClockwise(int x, int y)
    {
        //   8721
        //   6543
        var b = Read(_grid, x,  y);
        b = (b << 1) | Read(_grid, x -1, y);
        b = (b << 1) | Read(_grid, x, y+1);
        b = (b << 1) | Read(_grid, x -1, y+1);
        b = (b << 1) | Read(_grid, x-2, y+1);
        b = (b << 1) | Read(_grid, x -3, y+1);
        b = (b << 1) | Read(_grid, x-2, y);
        b = (b << 1) | Read(_grid, x -3, y);
        return (byte)b;
    }
    
    public byte ReadNibbleNorth(int x, int y)
    {
        //43
        //21
        var b = Read(_grid, x,  y);
        b = (b << 1) | Read(_grid, x -1, y);
        b = (b << 1) | Read(_grid, x, y-1);
        b = (b << 1) | Read(_grid, x -1, y-1);
        return (byte)b;
    }
    
    public int ReadBits(int x, int y, int length)
    {
        var b = Read(_grid, x,  y);
        for (var i = 1; i < length; i++)
            b = (b << 1) | Read(_grid, x +i, y);
        return b;
    }
    
    private int Read(bool[,] g, int x, int y)
    {
        return g[x, y] ^ _mask(y, x) ? 1 : 0;
    }
}