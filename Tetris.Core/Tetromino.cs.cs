namespace Tetris.Core;

public enum TetrominoType { I, O, T, S, Z, J, L }

/// <summary>
/// 每种方块用 4 个旋转状态表示，每个状态是 4 个 (x,y) 偏移。
/// 用偏移数组而不是矩阵，旋转时直接取下一组，避免旋转矩阵计算出错。
/// </summary>
public sealed class Tetromino
{
    public TetrominoType Type { get; }
    public int Rotation { get; private set; }
    public IReadOnlyList<(int X, int Y)> Cells => _states[Rotation];

    private readonly (int X, int Y)[][] _states;

    private Tetromino(TetrominoType type, (int X, int Y)[][] states)
    {
        Type = type;
        _states = states;
        Rotation = 0;
    }

    public void RotateCW() => Rotation = (Rotation + 1) % 4;
    public void RotateCCW() => Rotation = (Rotation + 3) % 4;
    public void SetRotation(int r) => Rotation = ((r % 4) + 4) % 4;

    public Tetromino Clone()
    {
        var t = new Tetromino(Type, _states);
        t.Rotation = Rotation;
        return t;
    }

    public static Tetromino Create(TetrominoType type) => type switch
    {
        TetrominoType.I => new(type, new[]
        {
            new[]{(0,1),(1,1),(2,1),(3,1)},
            new[]{(2,0),(2,1),(2,2),(2,3)},
            new[]{(0,2),(1,2),(2,2),(3,2)},
            new[]{(1,0),(1,1),(1,2),(1,3)},
        }),
        TetrominoType.O => new(type, new[]
        {
            new[]{(1,0),(2,0),(1,1),(2,1)},
            new[]{(1,0),(2,0),(1,1),(2,1)},
            new[]{(1,0),(2,0),(1,1),(2,1)},
            new[]{(1,0),(2,0),(1,1),(2,1)},
        }),
        TetrominoType.T => new(type, new[]
        {
            new[]{(0,1),(1,1),(2,1),(1,0)},
            new[]{(1,0),(1,1),(1,2),(2,1)},
            new[]{(0,1),(1,1),(2,1),(1,2)},
            new[]{(1,0),(1,1),(1,2),(0,1)},
        }),
        TetrominoType.S => new(type, new[]
        {
            new[]{(1,1),(2,1),(0,2),(1,2)},
            new[]{(1,0),(1,1),(2,1),(2,2)},
            new[]{(1,1),(2,1),(0,2),(1,2)},
            new[]{(1,0),(1,1),(2,1),(2,2)},
        }),
        TetrominoType.Z => new(type, new[]
        {
            new[]{(0,1),(1,1),(1,2),(2,2)},
            new[]{(2,0),(1,1),(2,1),(1,2)},
            new[]{(0,1),(1,1),(1,2),(2,2)},
            new[]{(2,0),(1,1),(2,1),(1,2)},
        }),
        TetrominoType.J => new(type, new[]
        {
            new[]{(0,0),(0,1),(1,1),(2,1)},
            new[]{(1,0),(2,0),(1,1),(1,2)},
            new[]{(0,1),(1,1),(2,1),(2,2)},
            new[]{(1,0),(1,1),(0,2),(1,2)},
        }),
        TetrominoType.L => new(type, new[]
        {
            new[]{(2,0),(0,1),(1,1),(2,1)},
            new[]{(1,0),(1,1),(1,2),(2,2)},
            new[]{(0,1),(1,1),(2,1),(0,2)},
            new[]{(0,0),(1,0),(1,1),(1,2)},
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}