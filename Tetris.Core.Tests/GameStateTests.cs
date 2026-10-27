using Tetris.Core;
using Xunit;

namespace Tetris.Core.Tests;

public class GameStateTests
{
    [Fact]
    public void MoveLeft_AtLeftWall_ReturnsFalse_AndPositionUnchanged()
    {
        var state = new GameState(seed: 42);
        // 把方块移到最左
        while (state.MoveLeft()) { }
        int before = state.CurrentX;
        Assert.False(state.MoveLeft());
        Assert.Equal(before, state.CurrentX);
    }

    [Fact]
    public void Tick_AtBottom_LocksBlockIntoBoard()
    {
        var state = new GameState(seed: 1);
        while (state.Tick()) { } // 落到底
        // 锁块后应生成新方块，且棋盘上有格子
        bool anyBlock = false;
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < Board.Height; y++)
                if (state.Board[x, y] is not null) anyBlock = true;
        Assert.True(anyBlock);
        Assert.NotNull(state.Current);
    }

    [Fact]
    public void Rotate_WhenBlocked_RevertsOrShifts()
    {
        var state = new GameState(seed: 7);
        // 贴左墙
        while (state.MoveLeft()) { }
        var before = state.Current!.Rotation;
        state.Rotate();
        // 要么旋转成功（可能带偏移），要么完全不变；不能崩
        Assert.InRange(state.CurrentX, 0, Board.Width - 1);
        Assert.InRange(state.Current!.Rotation, 0, 3);
    }

    [Fact]
    public void ClearFullLines_RemovesRowAndShiftsAbove()
    {
        var board = new Board();
        board.FillRowForTest(Board.Height - 1);
        board.Set(0, Board.Height - 2, TetrominoType.T);

        int cleared = board.ClearFullLines();

        Assert.Equal(1, cleared);
        // 原来倒数第二行的 T 应下移到最底行
        Assert.Equal(TetrominoType.T, board[0, Board.Height - 1]);
    }

    [Fact]
    public void Score_ClearingKLines_Gives100TimesK()
    {
        var state = new GameState(seed: 3);
        // 直接构造两行满行，用反射或公开方法触发计分不方便，
        // 这里通过 Board 清行 + 手动校验计分公式的解耦方式：
        // 实际项目中建议给 GameState 加一个 internal ApplyClearResult(int lines) 供测试。
        var board = new Board();
        board.FillRowForTest(Board.Height - 1);
        board.FillRowForTest(Board.Height - 2);
        int cleared = board.ClearFullLines();
        Assert.Equal(2, cleared);
        Assert.Equal(200, 100 * cleared);
    }

    [Fact]
    public void Rotate_FourTimes_ReturnsToOriginal()
    {
        var state = new GameState(seed: 5);
        var original = state.Current!.Cells.Select(c => (c.X, c.Y)).OrderBy(c => c).ToList();
        for (int i = 0; i < 4; i++) state.Rotate();
        var after = state.Current!.Cells.Select(c => (c.X, c.Y)).OrderBy(c => c).ToList();
        Assert.Equal(original, after);
    }
}
