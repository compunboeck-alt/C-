namespace Tetris.Core;

public sealed class Board
{
    public const int Width = 12;
    public const int Height = 16;

    private readonly TetrominoType?[,] _cells = new TetrominoType?[Height, Width];

    public TetrominoType? this[int x, int y] => _cells[y, x];

    public bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public bool IsEmpty(int x, int y) => !IsInside(x, y) || _cells[y, x] is null;

    public void Set(int x, int y, TetrominoType type)
    {
        if (IsInside(x, y)) _cells[y, x] = type;
    }

    public void Clear() => Array.Clear(_cells, 0, _cells.Length);

    /// <summary>清除满行，返回清除的行数。上方行下移。</summary>
    public int ClearFullLines()
    {
        int cleared = 0;
        for (int y = Height - 1; y >= 0; y--)
        {
            if (IsRowFull(y))
            {
                RemoveRow(y);
                cleared++;
                y++; // 同一 y 位置需要重新检查（上方下移来的行）
            }
        }
        return cleared;
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < Width; x++)
            if (_cells[y, x] is null) return false;
        return true;
    }

    private void RemoveRow(int y)
    {
        for (int row = y; row > 0; row--)
            for (int x = 0; x < Width; x++)
                _cells[row, x] = _cells[row - 1, x];
        for (int x = 0; x < Width; x++)
            _cells[0, x] = null;
    }

    /// <summary>测试用：直接填充一行，方便构造消行场景。</summary>
    public void FillRowForTest(int y, TetrominoType type = TetrominoType.I)
    {
        for (int x = 0; x < Width; x++) _cells[y, x] = type;
    }
}