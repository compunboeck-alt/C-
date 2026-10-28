namespace Tetris.Core;

public sealed class GameState
{
    public Board Board { get; } = new();
    public Tetromino? Current { get; private set; }
    public TetrominoType NextType { get; private set; }
    public int CurrentX { get; private set; }
    public int CurrentY { get; private set; }
    public int Score { get; private set; }
    public int Level { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    private readonly Random _random;
    private readonly Queue<TetrominoType> _queue = new();

    public GameState(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        NextType = RandomType();
        Spawn();
    }

    private TetrominoType RandomType() =>
        (TetrominoType)_random.Next(0, 7);

    private void Spawn()
    {
        Current = Tetromino.Create(NextType);
        NextType = RandomType();
        CurrentX = Board.Width / 2 - 2;
        CurrentY = 0;
        if (!CanPlace(Current, CurrentX, CurrentY))
        {
            IsGameOver = true;
        }
    }

    public bool CanPlace(Tetromino t, int x, int y)
    {
        foreach (var (cx, cy) in t.Cells)
        {
            int bx = x + cx, by = y + cy;
            if (bx < 0 || bx >= Board.Width || by >= Board.Height) return false;
            if (by >= 0 && !Board.IsEmpty(bx, by)) return false;
        }
        return true;
    }

    public bool MoveLeft()
    {
        if (Current is null || IsGameOver || IsPaused) return false;
        if (!CanPlace(Current, CurrentX - 1, CurrentY)) return false;
        CurrentX--;
        return true;
    }

    public bool MoveRight()
    {
        if (Current is null || IsGameOver || IsPaused) return false;
        if (!CanPlace(Current, CurrentX + 1, CurrentY)) return false;
        CurrentX++;
        return true;
    }

    public bool Rotate()
    {
        if (Current is null || IsGameOver || IsPaused) return false;
        var test = Current.Clone();
        test.RotateCW();
        // 简单踢墙：原地 → 左1 → 右1 → 上1
        int[] offsets = { 0, -1, 1 };
        foreach (var dx in offsets)
        {
            if (CanPlace(test, CurrentX + dx, CurrentY))
            {
                Current.SetRotation(test.Rotation);
                CurrentX += dx;
                return true;
            }
        }
        if (CanPlace(test, CurrentX, CurrentY - 1))
        {
            Current.SetRotation(test.Rotation);
            CurrentY -= 1;
            return true;
        }
        return false;
    }

    public bool Tick()
    {
        if (Current is null || IsGameOver || IsPaused) return false;
        if (CanPlace(Current, CurrentX, CurrentY + 1))
        {
            CurrentY++;
            return true;
        }
        LockAndSpawn();
        return false;
    }

    public void HardDrop()
    {
        if (Current is null || IsGameOver || IsPaused) return;
        while (CanPlace(Current, CurrentX, CurrentY + 1)) CurrentY++;
        LockAndSpawn();
    }

    public GameState Clone()
{
    var clone = new GameState();

    clone.Board.Clear();

    for (int x = 0; x < Board.Width; x++)
    {
        for (int y = 0; y < Board.Height; y++)
        {
            if (Board[x, y] is { } type)
                clone.Board.Set(x, y, type);
        }
    }

    clone.Current = Current?.Clone();
    clone.NextType = NextType;
    clone.CurrentX = CurrentX;
    clone.CurrentY = CurrentY;
    clone.Score = Score;
    clone.Level = Level;
    clone.LinesCleared = LinesCleared;
    clone.IsGameOver = IsGameOver;
    clone.IsPaused = IsPaused;

    return clone;
}
    private void LockAndSpawn()
    {
        foreach (var (cx, cy) in Current!.Cells)
        {
            int bx = CurrentX + cx, by = CurrentY + cy;
            if (by >= 0) Board.Set(bx, by, Current.Type);
        }
        int lines = Board.ClearFullLines();
        if (lines > 0)
        {
            LinesCleared += lines;
            Score += 100 * lines;
            Level = 1 + LinesCleared / 10;
        }
        Current = null;
        Spawn();
    }

    public void TogglePause()
    {
        if (IsGameOver) return;
        IsPaused = !IsPaused;
    }

    /// <summary>当前等级对应的下落间隔（毫秒）。等级越高越快，最低 100ms。</summary>
    public int TickIntervalMs => Math.Max(100, 800 - (Level - 1) * 70);
}
